using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GH.Core.Models;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class BusinessUserSystemController : BusinessController
    {
        private readonly IAccountService _accountService;

        public BusinessUserSystemController()
        {
            _accountService = new AccountService();
        }

        // GET: User/BusinessAccount
        public ActionResult AnalyticsCampaign()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            //if (currentUser.AccountType != AccountType.Business)
            //{
            //    var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
            //    if (!linkedAccounts.Any())
            //    {
            //        return Redirect("/");
            //    }
            //}

            return View();
        }
        // GET: User/BusinessAccount
        public ActionResult AnalyticsCustomerList()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            //if (currentUser.AccountType != AccountType.Business)
            //{
            //    var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
            //    if (!linkedAccounts.Any())
            //    {
            //        return Redirect("/");
            //    }
            //}

            return View();
        }

    }
}