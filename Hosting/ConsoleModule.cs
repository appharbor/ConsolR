using System.IO;
using System.Linq;
using System.Text;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using Nancy;
using Nancy.Json;
using Roslyn.Compilers;

namespace ConsolR.Hosting
{
	public class ConsoleModule : NancyModule
	{
		public ConsoleModule()
			: base("consolr")
		{
			Get["/"] = parameters =>
			{
				return View["assets/consolr/index.html"];
			};

			Post["/validate"] = parameters =>
			{
				var jsonBytes = Encoding.UTF8.GetBytes(GetValidationResult(Request.Body));
				return new Response
				{
					ContentType = "application/json",
					Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length),
				};
			};

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
