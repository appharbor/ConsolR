using System.Linq;
using System.Web.Mvc;
using ConsolR.Models;
using ConsolR.Services;
using ConsolR.Web.Models;
using Roslyn.Compilers;

namespace ConsolR.Web.Controllers
{
	public class ConsoleController : AsyncController
	{
		public ConsoleController()
		{
			compiler = new CSharpValidator(new CSharpCompilationProvider());
		}

		private readonly CSharpValidator compiler;

		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Validate(string command, string classes)
		{
			var post = new SourceCode { Classes = classes, Content = command };

			var errors = compiler.GetCompilationErrors(post)
								 .Where(x => x.Info.Severity > DiagnosticSeverity.Warning)
								 .Select(x => new EditorError
											  {
												  Location = x.Location.GetLineSpan(true),
												  Message = x.Info.GetMessage()
											  });

			return Json(new { status = "ok", data = errors });
		}
	}
}
