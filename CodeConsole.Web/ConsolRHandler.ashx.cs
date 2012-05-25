using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsolR.Web
{
	public class ConsolRHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/html";
			context.Response.WriteFile("~/consolr.html");
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}