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
			routes.MapHttpHandler<ConsolRHandler>("consolr/validate");
			routes.MapHttpHandler<ConsolRHandler>("consolr");
			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
		}
	}
}
