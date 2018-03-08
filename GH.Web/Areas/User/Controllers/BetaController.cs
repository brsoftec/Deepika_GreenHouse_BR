using System.Web.Mvc;
using GH.Core.Services;
using GH.Core.IServices;

namespace GH.Web.Areas.Beta.Controllers
{
    [Authorize]
    public class BetaController : Controller
    {
        private IDisabledUserService disabledUserService { get; set; }
        public BetaController()
        {
            disabledUserService = new DisabledUserService();
        }

        // GET: User/User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Signup()
        {
            return View();
        }

        public ActionResult BusinessPage()
        {
            return View();
        }

        public ActionResult Notifications()
        {
            return View();
        }

        public ActionResult Calendar()
        {
            return View();
        }
        public ActionResult CalendarEvent()
        {
            return View();
        }

        public ActionResult Vault()
        {
            return View();
        }


        public ActionResult IndexBusiness()
        {

            return View();
        }
       
        
        public ActionResult NotificationsBusiness()
        {
            return View();
        }

        public ActionResult InteractionModule()
        {
            return View();
        }

        public ActionResult TestUpdatePinCode()
        {
            return View();
        }
    }
}