using System.Web.Mvc;
using Foo.Core.Model;
using Foo.Core.Persistence;

namespace ConsolR.Web.Controllers
{
	public class UserController : Controller
	{
		private readonly Context _context = new Context();

		public ActionResult Index()
		{
			return View(_context.Users);
		}

		public ActionResult Create(User user)
		{
			_context.Users.Add(user);
			_context.SaveChanges();

			return RedirectToAction("Index");
		}
	}
}
