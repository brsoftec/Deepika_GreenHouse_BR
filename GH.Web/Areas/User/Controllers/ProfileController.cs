using GH.Core.Services;
using System.Web.Mvc;
using GH.Web.Areas.User.ViewModels;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    public class ProfileController : BaseController
    {
        private static readonly IUserCreatedBusinessService UcbService = new UserCreatedBusinessService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ProfileController()
        {
        }

        // GET: /profile/id
        public ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return ErrorView("The profile you are looking for is not found.", "Invalid Profile",
                     "profile", "user");
            }
            return View(id);
        }
        public ActionResult UserCreated(string id)
        {
            UserCreatedBusiness ucb = UcbService.GetUcbById(id);
            if (string.IsNullOrEmpty(id) || ucb == null)
            {
                return ErrorView("You are looking for an invalid user created business that is not found.", "Invalid User Created Business",
                     "profile", "user");
            }
            return View("Index", new UserCreatedBusinessModel(ucb));
        }       

    }
}