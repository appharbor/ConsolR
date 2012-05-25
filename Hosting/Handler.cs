using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using Roslyn.Compilers;

namespace ConsolR.Hosting
{
	public class Handler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			var contextWrapper = new HttpContextWrapper(context);
			BasicAuthenticator.Authenticate(contextWrapper);

			switch (context.Request.Path.ToLower())
			{
				case "/consolr":
					context.Response.ContentType = "text/html";
					context.Response.WriteFile("~/assets/consolr/index.html");
					break;
				case "/consolr/validate":
					context.Response.ContentType = "application/json";
					context.Response.Write(GetValidationResult(contextWrapper));
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private string GetValidationResult(HttpContextWrapper context)
		{
			var compiler = new CSharpValidator(new CSharpCompilationProvider());
			var serializer = new JavaScriptSerializer();

			SourceCode sourceCode;
			using (var reader = new StreamReader(context.Request.InputStream))
			{
				var foo = reader.ReadToEnd();
				sourceCode = serializer.Deserialize<SourceCode>(foo);
			}

			var errors = compiler.GetCompilationErrors(sourceCode)
				.Where(x => x.Info.Severity > DiagnosticSeverity.Warning)
				.Select(x => new
				{
					Location = x.Location.GetLineSpan(true),
					Message = x.Info.GetMessage(),
				});

			return serializer.Serialize(new { status = "ok", data = errors });
		}

		public bool IsReusable { get { return false; } }
	}
}
