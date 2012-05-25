using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using Newtonsoft.Json;
using Roslyn.Compilers;
using SignalR;
using SignalR.Hosting;

[assembly: WebActivator.PostApplicationStartMethod(typeof(ConsolR.Web.Bootstrapper), "PreApplicationStart")]

namespace ConsolR.Web
{
	public static class Bootstrapper
	{
		public static void PreApplicationStart()
		{
			var routes = RouteTable.Routes;
			routes.MapHttpHandler<ConsolRHandler>("consolr/validate");
			routes.MapHttpHandler<ConsolRHandler>("consolr");
			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
		}
	}

	public class ConsolRHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			var contextWrapper = new HttpContextWrapper(context);
			BasicAuthenticator.Authenticate(contextWrapper);

			switch (context.Request.Path.ToLower())
			{
				case "/consolr":
					context.Response.ContentType = "text/html";
					context.Response.WriteFile("~/assets/consolr/index.html");
					break;
				case "/consolr/validate":
					context.Response.ContentType = "application/json";
					context.Response.Write(GetValidationResult(contextWrapper));
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private string GetValidationResult(HttpContextWrapper context)
		{
			var compiler = new CSharpValidator(new CSharpCompilationProvider());
			var serializer = new JavaScriptSerializer();

			SourceCode sourceCode;
			using (var reader = new StreamReader(context.Request.InputStream))
			{
				var foo = reader.ReadToEnd();
				sourceCode =  serializer.Deserialize<SourceCode>(foo);
			}

			var errors = compiler.GetCompilationErrors(sourceCode)
				.Where(x => x.Info.Severity > DiagnosticSeverity.Warning)
				.Select(x => new
				{
					Location = x.Location.GetLineSpan(true),
					Message = x.Info.GetMessage(),
				});

			return serializer.Serialize(new { status = "ok", data = errors });
		}

		public bool IsReusable { get { return false; } }
	}

	public class ExecuteEndPoint : PersistentConnection
	{
		public static TimeSpan ExecutionTimeout;
		private static readonly CSharpExecutor Executer = new CSharpExecutor();

		static ExecuteEndPoint()
		{
			int timeout;
			if (!int.TryParse(ConfigurationManager.AppSettings["ConsolR.ExecutionTimeout"], out timeout))
			{
				timeout = 30;
			};
			ExecutionTimeout = TimeSpan.FromSeconds(timeout);
		}

		public override Task ProcessRequestAsync(HostContext context)
		{
			var httpContext = (HttpContextWrapper)context.Items["System.Web.HttpContext"];
			BasicAuthenticator.Authenticate(httpContext);

			return base.ProcessRequestAsync(context);
		}

		protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
		{
			var sourceCode = JsonConvert.DeserializeObject<SourceCode>(data);
			var result = Executer.Execute(sourceCode, ExecutionTimeout);

			return Connection.Send(connectionId, new
			{
				status = "ok",
				data = result.Result,
			});
		}
	}

	public class BasicAuthenticator
	{
		private const string AuthCookieName = "consolr-auth";
		private const string Username = "foo";
		private const string Password = "baz";

		public static bool Authenticate(HttpContextBase context)
		{
			var authCookie = context.Request.Cookies[AuthCookieName];
			var authCookieValue = authCookie == null ? null : authCookie.Value;

			var credentials = GetCredentials(context.Request.Headers);

			if (credentials != null && credentials.UserName == Username && credentials.Password == Password)
			{
				authCookie = new HttpCookie(AuthCookieName, GetAuthToken());
				context.Response.Cookies.Add(authCookie);
				return true;
			}
			else if (authCookieValue != GetAuthToken())
			{
				context.Response.StatusCode = 401;
				context.Response.AddHeader("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", "ConsolR"));

				context.Response.Flush();
				return false;
			}

			return true;
		}

		private static NetworkCredential GetCredentials(NameValueCollection headers)
		{
			var httpAuthorizationHeader = headers["Authorization"];
			if (string.IsNullOrEmpty(httpAuthorizationHeader))
			{
				return null;
			}

			string[] httpAuthorization = httpAuthorizationHeader.Split(' ');
			if (httpAuthorization.Length != 2)
			{
				return null;
			}

			if (httpAuthorization[0] != "Basic")
			{
				return null;
			}

			return ParseCredentials(httpAuthorization[1]);
		}

		private static NetworkCredential ParseCredentials(string value)
		{
			byte[] encodedBytes = Convert.FromBase64String(value);

			string unencoded = Encoding.GetEncoding("iso-8859-1").GetString(encodedBytes);
			if (unencoded.IndexOf(':') < 0)
			{
				return null;
			}

			string username = unencoded.Remove(unencoded.IndexOf(':'));
			string password = unencoded.Substring(unencoded.IndexOf(':') + 1);

			return new NetworkCredential(username, password);
		}

		private static string GetAuthToken()
		{
			using (HashAlgorithm hashAlgorithm = new SHA1Managed())
			{
				var preToken = string.Format("{0}:{1}", Username, Password);

				return GetHashed(hashAlgorithm, preToken);
			}
		}

		public static string GetHashed(HashAlgorithm hashAlgorithm, string value, Encoding encoding = null)
		{
			encoding = encoding ?? Encoding.UTF8;
			var valueBytes = encoding.GetBytes(value);

			var hashBytes = hashAlgorithm.ComputeHash(valueBytes);

			var output = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				output.Append(hashBytes[i].ToString("x2"));
			}
			return output.ToString();
		}
	}

	public static class HttpHandlerExtensions
	{
		public static void MapHttpHandler<THandler>(this RouteCollection routes, string url) where THandler : IHttpHandler, new()
		{
			routes.MapHttpHandler<THandler>(null, url, null, null);
		}

		public static void MapHttpHandler<THandler>(this RouteCollection routes,
			string name, string url, object defaults, object constraints)
			where THandler : IHttpHandler, new()
		{
			var route = new Route(url, new HttpHandlerRouteHandler<THandler>());
			route.Defaults = new RouteValueDictionary(defaults);
			route.Constraints = new RouteValueDictionary(constraints);
			routes.Add(name, route);
		}

		private class HttpHandlerRouteHandler<THandler>
			: IRouteHandler where THandler : IHttpHandler, new()
		{
			public IHttpHandler GetHttpHandler(RequestContext requestContext)
			{
				return new THandler();
			}
		}
	}
}
