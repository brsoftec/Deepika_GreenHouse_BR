using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.About.Controllers
{
    public class AboutController : Controller
    {
        // GET: About/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult New()
        {
            return View();
        }


        // GET: About/Business
        public ActionResult Business()
        {
            return View();
        }


        // GET: about/privacypolicy

        [ActionName("privacy-policy")]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [ActionName("terms-conditions")]
        public ActionResult TermsConditions()
        {
            return View();
        }

        [ActionName("business-terms-conditions")]
        public ActionResult BusinessTermsConditions()
        {
            return View();
        }

/*        [ActionName("developers")]
        public ActionResult Developers()
        {
            return View();
        }*/
    }
}