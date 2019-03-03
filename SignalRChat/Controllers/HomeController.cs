using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRChat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.UserGroups = new String[] { "Group1", "Group2", "Group3" };
            }
            else
            {
                ViewBag.UserGroups = new String[] { "Group1" };
            }
            return View();
        }

    }
}