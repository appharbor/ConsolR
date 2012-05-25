using System.Web.Routing;
using ConsolR.Hosting.Web;
using SignalR;

namespace ConsolR.Hosting
{
	[assembly: WebActivator.PostApplicationStartMethod(typeof(ConsolR.Hosting.Bootstrapper), "PreApplicationStart")]

	public static class Bootstrapper
	{
		public static void PreApplicationStart()
		{
			var routes = RouteTable.Routes;
			routes.MapHttpHandler<Handler>("consolr/validate");
			routes.MapHttpHandler<Handler>("consolr");
			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
		}
	}
}
