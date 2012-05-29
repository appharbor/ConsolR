using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using Roslyn.Compilers;

namespace ConsolR.Hosting.Nancy
{
	public class ConsoleHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var contextWrapper = new HttpContextWrapper(context);
			if (!BasicAuthenticator.Authenticate(contextWrapper))
			{
				return;
			}

			var response = contextWrapper.Response;
			var request = contextWrapper.Request;

			var rootPath = string.Format("/{0}", ConfigurationManager.AppSettings["consolr.rootPath"]);

			if (request.Path.StartsWith(rootPath))
			{
				switch (request.Path.Remove(0, rootPath.Length))
				{
					case "":
						contextWrapper.Response.WriteFile(HostingEnvironment.MapPath("~/assets/consolr/index.html"));
						break;
					case "/validate":
						response.ContentType = "application/json";
						response.Write(GetValidationResult(request.InputStream));
						break;
					default:
						response.StatusCode = (int)HttpStatusCode.NotFound;
						break;
				};
			}
			response.Flush();
		}

		private string GetValidationResult(Stream requestStream)
		{
			var compiler = new CSharpValidator(new CSharpCompilationProvider());
			var serializer = new JavaScriptSerializer();

			SourceCode sourceCode;
			using (var reader = new StreamReader(requestStream))
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
	}
}
