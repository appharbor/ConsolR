using System.Web.Hosting;
using Nancy;

namespace ConsolR.Hosting.Nancy
{
	public class AspNetRootSourceProvider : IRootPathProvider
	{
		public string GetRootPath()
		{
			return HostingEnvironment.MapPath("~/");
		}
	}
}
