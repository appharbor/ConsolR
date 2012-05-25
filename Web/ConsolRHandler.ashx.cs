using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using ConsolR.Core.Web;
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
