using System.Web.Mvc;
using GH.Web.Areas.User.ViewModels;

namespace GH.Web.Areas.User.Controllers
{
    public class WebErrorController : BaseController
    {
        public ActionResult Index1()
        {
            return View("~/Areas/User/Views/Error/Index.cshtml");
        }

        public ActionResult Index(WebErrorViewModel model)
        {
            return
            View("~/Areas/User/Views/Error/Index.cshtml");
        }       
        public ActionResult Unauthorized(WorkflowErrorViewModel model)
        {
            return
            View("~/Areas/User/Views/Error/Unauthorized.cshtml", model);
        }
    }
}
