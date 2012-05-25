using System.Linq;
using System.Web.Mvc;
using ConsolR.Models;
using ConsolR.Services;
using Roslyn.Compilers;
using Newtonsoft.Json;

namespace ConsolR.Web.Controllers
{
	public class ConsoleController : AsyncController
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Validate(string command, string classes)
		{
			var compiler = new CSharpValidator(new CSharpCompilationProvider());
			var post = new SourceCode
			{
				Classes = classes,
				Content = command
			};

			var errors = compiler.GetCompilationErrors(post)
				.Where(x => x.Info.Severity > DiagnosticSeverity.Warning)
				.Select(x => new
				{
					Location = x.Location.GetLineSpan(true),
					Message = x.Info.GetMessage()
				});

			return Json(new { status = "ok", data = errors });
		}
	}
}
