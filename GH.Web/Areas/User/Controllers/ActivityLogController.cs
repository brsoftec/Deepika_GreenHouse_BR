using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.User.Controllers
{
    public class ActivityLogController : BaseController
    {
        // GET: User/ActivityLog
//        public ActionResult Index()
//        {
//            return View();
//        }
        public ActionResult Index()
        {
            if (ViewBag.IsBusinessView)
            {
                return View("Business");
            }
            else
            {
                return View("Index");
            }
            
        }     

        public ActionResult Business()
        {
            return View();
        }

        //

    }
}
