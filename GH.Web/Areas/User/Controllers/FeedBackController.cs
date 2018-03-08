using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.User.Controllers
{
    public class FeedBackController : Controller
    {
        //// GET: User/FeedBack
        [Authorize(Roles = "FeedBack")]
        public ActionResult Index()
        {
            return View();
        }
    }
}