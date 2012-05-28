using System.Web;
using System.Web.Routing;

namespace ConsolR.Hosting
{
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
			route.Constraints = new RouteValueDictionary();
			route.Constraints.Add(name, constraints);
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
