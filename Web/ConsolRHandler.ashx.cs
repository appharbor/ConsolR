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
using Newtonsoft.Json;
using Roslyn.Compilers;
using SignalR;
using SignalR.Hosting;


namespace ConsolR.Web
{
	public class ConsolRHandler : IHttpHandler
	{
		static ConsolRHandler()
		{
			var routes = RouteTable.Routes;

			routes.MapHttpHandler<ConsolRHandler>("consolr/validate");
			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
		}

		public void ProcessRequest(HttpContext context)
		{
			switch (context.Request.Path.ToLower())
			{
				case "/consolrhandler.ashx":
					context.Response.ContentType = "text/html";
					context.Response.WriteFile("~/assets/consolr/index.html");
					break;
				case "/consolr/validate":
					;
					context.Response.ContentType = "application/json";
					context.Response.Write(GetValidationResult(context));
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private string GetValidationResult(HttpContext context)
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
			var timeout = int.Parse(ConfigurationManager.AppSettings["ConsolR.ExecutionTimeout"]);
			ExecutionTimeout = TimeSpan.FromSeconds(timeout);
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