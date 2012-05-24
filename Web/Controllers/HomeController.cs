using System.Linq;
using System.Text;
using System.Web.Mvc;
using Compilify.Models;
using Compilify.Services;
using Compilify.Web.Models;
using Roslyn.Compilers;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public HomeController()
        {
            compiler = new CSharpValidator(new CSharpCompilationProvider());
        }

        private readonly CSharpValidator compiler;
        
        [HttpGet]
        public ActionResult Index()
        {
            var post = BuildSamplePost();
            
            var errors = compiler.GetCompilationErrors(post)
                                 .Select(x => new EditorError
                                              {
                                                  Location = x.Location.GetLineSpan(true),
                                                  Message = x.Info.GetMessage()
                                              });

            var viewModel = new PostViewModel(post)
                            {
                                Errors = errors
                            };

            return View(viewModel);
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

        private static Post BuildSamplePost()
        {
            var post = new Post();
            var builder = new StringBuilder();

            post.Classes = builder.AppendLine("class Person")
                                  .AppendLine("{")
                                  .AppendLine("    public Person(string name)")
                                  .AppendLine("    {")
                                  .AppendLine("        Name = name;")
                                  .AppendLine("    }")
                                  .AppendLine()
                                  .AppendLine("    public string Name { get; private set; }")
                                  .AppendLine()
                                  .AppendLine("    public string Greet()")
                                  .AppendLine("    {")
                                  .AppendLine("        if (Name == null)")
                                  .AppendLine("            return \"Hello, stranger!\";")
                                  .AppendLine()
                                  .AppendLine("        return string.Format(\"Hello, {0}!\", Name);")
                                  .AppendLine("    }")
                                  .AppendLine("}")
                                  .ToString();

            post.Content = builder.Clear()
                                  .AppendLine("var person = new Person(name: null);")
                                  .AppendLine("")
                                  .AppendLine("return person.Greet();")
                                  .ToString();
            

            

            return post;
        }
    }
}
