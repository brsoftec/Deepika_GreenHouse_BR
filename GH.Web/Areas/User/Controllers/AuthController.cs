using GH.Core.Services;
using System.Web.Mvc;
using GH.Web.Areas.User.ViewModels;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    public class AuthController : BaseController
    {
       
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public AuthController()
        {
        }

        // GET: /profile/id
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Callback()
        {
            
            return View("Gplus","");
        }       

    }
}