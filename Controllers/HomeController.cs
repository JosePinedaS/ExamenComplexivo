using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Salud_Vida_2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string alertMessage = null)
        {
            ViewBag.AlertMessage = alertMessage;
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
    }
}