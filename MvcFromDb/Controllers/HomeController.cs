﻿using System.Web;
using System.Web.Mvc;
using MvcLib.Common;

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
            CacheWrapper.Clear();
            HttpRuntime.UnloadAppDomain();
            return RedirectToAction("Index", new { q = "Reset success" });
        }
    }
}