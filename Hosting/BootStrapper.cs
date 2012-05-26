using System.Web.Routing;
using Nancy.Hosting.Aspnet;
using SignalR;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ConsolR.Hosting.Bootstrapper), "PreApplicationStart")]

namespace ConsolR.Hosting
{
	public static class Bootstrapper
	{
		public static void PreApplicationStart()
		{
			var routes = RouteTable.Routes;

			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
			routes.MapHttpHandler<NancyHttpRequestHandler>("consolr/{*path}");
		}
	}
}
