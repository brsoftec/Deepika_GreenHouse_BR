using GH.Core.BlueCode.BusinessLogic;
using GH.Core.Services;
using GH.Util;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.User.Controllers
{
  
    [Authorize]
    public class DelegationManagerController : BaseController
    {
        private IAccountService _accountService;
        // GET: User/DelegationManager
        public ActionResult Index()
        {
            _accountService = new AccountService();
            if (User.Identity.IsAuthenticated)
            {
                if (!ConfigHelp.GetBoolValue("IsCheckPinVault"))
                    return View();
              
                var rs = _accountService.IsSMSAuthenticated(User.Identity.GetUserId());
                if (rs == false)
                    return Redirect("/User/SMSAuthencation?TypeRedirect=Delegate");
                   
            }
            return View();
        }

    }
}
