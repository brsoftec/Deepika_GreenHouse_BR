using System;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.User.Controllers
{
    public class SupportController : BaseController
    {
        // GET: Help/
        public ActionResult Index()
        {
            return View();
        }

        //[ActionName("privacy-policy")]
        public ActionResult Help()
        {
            return View();
        }

    }
}