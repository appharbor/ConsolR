using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ConsolR.Web.EndPoints;
using ConsolR.Web.Infrastructure.Extensions;
using SignalR;
using RequireHttpsAttribute = ConsolR.Web.Infrastructure.RequireHttpsAttribute;

namespace ConsolR.Web
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			ViewEngines.Engines.Clear();
			ViewEngines.Engines.Add(new RazorViewEngine());

			MvcHandler.DisableMvcResponseHeader = true;

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}

		private static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new RequireHttpsAttribute());
		}

		private static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapLowercaseRoute(
				name: "Root",
				url: "",
				defaults: new { controller = "User", action = "Index" },
				constraints: new { httpMethod = new HttpMethodConstraint("GET") }
			);

			routes.MapLowercaseRoute(
	name: "create-user",
	url: "",
	defaults: new { controller = "User", action = "Create" },
	constraints: new { httpMethod = new HttpMethodConstraint("POST") }
);

			routes.MapLowercaseRoute(
				name: "validate",
				url: "validate",
				defaults: new { controller = "Console", action = "Validate" },
				constraints: new { httpMethod = new HttpMethodConstraint("POST") }
			);

			routes.MapLowercaseRoute(
				name: "console",
				url: "console",
				defaults: new { controller = "Console", action = "Index" },
				constraints: new { httpMethod = new HttpMethodConstraint("GET") }
);

			routes.MapConnection<ExecuteEndPoint>("execute", "execute/{*operation}");
			routes.MapRoute(
				"Error",
				"Error/{status}",
				 new { controller = "Error", action = "Index", status = UrlParameter.Optional }
			);
		}
	}
}
