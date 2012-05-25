using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ConsolR.Web.EndPoints;
using ConsolR.Web.Infrastructure.Extensions;
using SignalR;
using RequireHttpsAttribute = ConsolR.Web.Infrastructure.RequireHttpsAttribute;

namespace CodeConsole.Web
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			ViewEngines.Engines.Clear();
			ViewEngines.Engines.Add(new RazorViewEngine());

			MvcHandler.DisableMvcResponseHeader = true;

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterBundles(BundleTable.Bundles);
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

		private static void RegisterBundles(BundleCollection bundles)
		{
			var css = new Bundle("~/css");
			css.AddFile("~/assets/css/vendor/bootstrap-2.0.2.css");
			css.AddFile("~/assets/css/vendor/font-awesome.css");
			css.AddFile("~/assets/css/vendor/codemirror-2.23.css");
			css.AddFile("~/assets/css/vendor/codemirror-neat-2.23.css");
			css.AddFile("~/assets/css/compilify.css");
			bundles.Add(css);

			var js = new Bundle("~/js");
			js.AddFile("~/assets/js/vendor/json2.js");
			js.AddFile("~/assets/js/vendor/underscore-1.3.1.js");
			js.AddFile("~/assets/js/vendor/backbone-0.9.2.js");
			js.AddFile("~/assets/js/vendor/bootstrap-2.0.2.js");
			js.AddFile("~/assets/js/vendor/codemirror-2.23.js");
			js.AddFile("~/assets/js/vendor/codemirror-clike-2.23.js");
			js.AddFile("~/assets/js/vendor/jquery.signalr-0.5rc.js");
			js.AddFile("~/assets/js/vendor/jquery.validate-1.8.0.js");
			js.AddFile("~/assets/js/vendor/jquery.validate.unobtrusive.js");
			js.AddFile("~/assets/js/vendor/jquery.validate-hooks.js");
			js.AddFile("~/assets/js/vendor/shortcut.js");

			js.AddFile("~/assets/js/compilify.validation.js");
			js.AddFile("~/assets/js/compilify.js");
			bundles.Add(js);

			if (!HttpContext.Current.IsDebuggingEnabled)
			{
				css.Transform = new CssMinify();
				js.Transform = new JsMinify();
			}
		}
	}
}
