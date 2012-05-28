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

			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
			routes.MapHttpHandler<NancyHttpRequestHandler>("consolr", "consolr/{*path}", null, new IncomingOnlyRouteConstraint());
		}
	}
}
