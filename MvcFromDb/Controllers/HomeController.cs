using System.Web;
using System.Web.Mvc;
using MvcLib.Common;
using MvcLib.Common.Cache;
using MvcLib.FsDump;

namespace MvcFromDb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public ActionResult Reset()
        {
            WebCacheWrapper.Instance.Clear();
            HttpRuntime.UnloadAppDomain();
            return RedirectToAction("Index", new { q = "Reset success" });
        }

        public ActionResult Refresh()
        {
            DbToLocal.Execute();
            
            return RedirectToAction("Index", new { q = "Refresh success" });
        }
    }
}