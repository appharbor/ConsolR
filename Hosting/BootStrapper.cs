using System.Configuration;
using System.Web;
using System.Web.Routing;
using ConsolR.Hosting.Nancy;
using SignalR;
using SignalR.Hosting.AspNet.Routing;

[assembly: PreApplicationStartMethod(typeof(ConsolR.Hosting.Bootstrapper), "PreApplicationStart")]

namespace ConsolR.Hosting
{
	public static class Bootstrapper
	{
		public static void PreApplicationStart()
		{
			var routes = RouteTable.Routes;

			routes.MapConnection<ExecuteEndPoint>("consolr-execute", GetPath("execute/{*operation}"));
			routes.MapHttpHandler<NancyHttpRequestHandler>("consolr", GetPath("{*path}"), null, new IncomingOnlyRouteConstraint());
		}

		private static string GetPath(string relativePath)
		{
			var rootPath = string.Format("{0}/", ConfigurationManager.AppSettings["consolr.rootPath"]);
			return string.Concat(rootPath, relativePath);
		}
	}
}
