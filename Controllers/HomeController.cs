using System.Web.Mvc;

namespace ChatManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult About()
        {
            return View();
        }
    }
}