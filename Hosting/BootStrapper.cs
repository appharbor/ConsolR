using System.Web.Routing;
using SignalR;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ConsolR.Hosting.Bootstrapper), "PreApplicationStart")]

namespace ConsolR.Hosting
{
	public static class Bootstrapper
	{
		public static void PreApplicationStart()
		{
			var routes = RouteTable.Routes;

			routes.MapHttpHandler<Handler>("consolr");
			routes.MapHttpHandler<Handler>("consolr/validate");
			routes.MapConnection<ExecuteEndPoint>("consolr-execute", "consolr/execute/{*operation}");
		}
	}
}
