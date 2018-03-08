using GH.Core.Services;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GH.Core.BlueCode.BusinessLogic;


namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class CampaignUserController : BaseController
    {
        private readonly IAccountService _accountService;
        private IRoleService _roleService;
        public CampaignUserController()
        {
            _accountService = new AccountService();
            _roleService = new RoleService();
        }
        // GET: User/BusinessAccount
        public ActionResult ManagerCampaign()
        {
            return View();
        }

        public ActionResult ManageSyncs()
        {
            return View();
        }

     
    }
}