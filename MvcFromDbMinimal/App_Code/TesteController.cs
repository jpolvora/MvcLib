using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// ReSharper disable once CheckNamespace
namespace MvcFromDbMinimal.Controllers
{
    public class TesteController: Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/appcode.cshtml");
        }
    }
}