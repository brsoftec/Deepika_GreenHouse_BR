using GH.Core.Services;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GH.Core.BlueCode.BusinessLogic;
using GH.Web.Areas.User.ViewModels;
using GH.Core.Models;
using System.Linq;
using System.Collections.Generic;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class CampaignController : BusinessController
    {
        private readonly IAccountService _accountService;
        private IRoleService _roleService;
        public CampaignController()
        {
            _accountService = new AccountService();
            _roleService = new RoleService();
        }
        // GET: User/BusinessAccount
        public ActionResult ManagerHandshakeUsers()
        {
            return View();
        }
        public ActionResult ManagerCampaign()
        {

            //WRITE LOG
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You accessed interaction manager.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
            return View();
        }


        private bool Checkpermission(string pagenamecheck)
        {
            List<Role> roles = new List<Role>();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            var baAccount = account;
            if (account.AccountType == AccountType.Personal)
            {
                try
                {
                    baAccount = _accountService.GetById(account.BusinessAccountRoles[0].AccountId);
                    roles = _roleService.GetRolesOfAccount(account, baAccount.Id);
                    if (roles == null || roles.Count <= 0)
                        return false;
                }
                catch { }
            }
            else
                return true;
            var check = true;
            foreach (Role role in roles)
            {
                if (role.Name == Role.ROLE_EDITOR)
                {
                    check = (new string[] { "ManagerCampaign", "InsertAdvertisingCampaign", "InsertRegistrationCampaign", "InsertRegistrationCampaign", "InsertEventCampaign", "InsertPushToVault" , "InsertHandshakeCampaign" }).Any(x => x == pagenamecheck);
                    if (check)
                        break;
                }
                else if (role.Name == Role.ROLE_REVIEWER)
                {
                    check = (new string[] { "ManagerCampaign", "ApprovedAdvertisingCampaign", "ApprovedRegistrationCampaign", "ApprovedEventCampaign", "ApprovedPushToVaultCampaign", "ApprovedHandShakeCampaign" }).Any(x => x == pagenamecheck);
                    if (check)
                        break;
                }
            }
            return check;

        }
    }
}