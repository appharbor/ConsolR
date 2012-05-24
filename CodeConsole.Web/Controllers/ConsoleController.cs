using System.Linq;
using System.Web.Mvc;
using Compilify.Models;
using Compilify.Services;
using Compilify.Web.Models;
using Roslyn.Compilers;

namespace Compilify.Web.Controllers
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
		public ActionResult Validate(ValidateViewModel viewModel)
		{
			var post = new Post { Classes = viewModel.Classes, Content = viewModel.Command };

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
