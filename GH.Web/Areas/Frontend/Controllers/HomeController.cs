using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.Frontend.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult ExternalLoginSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult UpdateDatabase()
        {
            IDbMigrationService _migration = new DbMigrationService();
            _migration.UpdateDatabase();
            return null;
        }
    }
}
