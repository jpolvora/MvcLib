﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
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
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Reset()
        {
            CacheWrapper.Clear();
            HttpRuntime.UnloadAppDomain();
            return new HttpStatusCodeResult(200);
        }
    }
}