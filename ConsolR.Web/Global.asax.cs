using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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

			routes.MapRoute(
				name: "Root",
				url: "",
				defaults: new { controller = "User", action = "Index" },
				constraints: new { httpMethod = new HttpMethodConstraint("GET") }
			);

			routes.MapRoute(
				name: "create-user",
				url: "",
				defaults: new { controller = "User", action = "Create" },
				constraints: new { httpMethod = new HttpMethodConstraint("POST") }
			);

			routes.MapRoute(
				"Error",
				"Error/{status}",
				 new { controller = "Error", action = "Index", status = UrlParameter.Optional }
			);
		}
	}
}
