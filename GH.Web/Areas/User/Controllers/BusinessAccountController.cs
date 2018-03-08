using GH.Core.Services;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GH.Core.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using GH.Core.BlueCode.BusinessLogic;
using Microsoft.Owin.Security;
using MongoDB.Bson;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class BusinessAccountController : BusinessController
    {
        IAccountService _accountService;

        public BusinessAccountController()
        {
            _accountService = new AccountService();
        }

        // GET: User/BusinessAccount
        public ActionResult Index()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            
            var check = _accountService.IsConfirmSMS(User.Identity.GetUserId());
            var otp = currentUser.PhoneVerifiedToken?? EnumAccount.otpDisable;
            if (otp == EnumAccount.otpDisable)
                check = true;
            if (currentUser.AccountType != AccountType.Business)
            {
                var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
                var businessAccount = linkedAccounts.FirstOrDefault(x => !x.EmailVerified);
                if (businessAccount != null)
                {
                    var url = Url.Content("~/") + "BusinessAccount/VerifyingEmail?email=" + businessAccount.Profile.Email;
                    return Redirect(url);

                }
                if (!linkedAccounts.Any(l => l.EmailVerified))
                {
                    return Redirect("/BusinessAccount/Signup");
                }
            }
            if (!check)
            {
               
                return Redirect("/BusinessAccount/SignIn");
            }
            
            if (string.IsNullOrEmpty(currentUser.Profile.PhotoUrl))
            {
                ViewBag.NoAvatar = true;
            }

            if (currentUser.ViewPreferences != null)
            {
                ViewBag.ViewPreferences = currentUser.ViewPreferences;
            }
            else
            {
                ViewBag.ViewPreferences = new AccountViewPreferences
                {
                    ShowBusinessIntroSlides = false
                };
            }

            return View();

        }

        [AllowAnonymous]
        public ActionResult VerifyingEmail(string email)
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.GetOwinContext()
                    .Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType,
                        OAuthDefaults.AuthenticationType);
                return Redirect(HttpContext.Request.Url.OriginalString);
            }

            IAccountService _accountService = new AccountService();

            var account = _accountService.GetByEmail(email);
            if (account == null || account.EmailVerified)
            {
                return Redirect("/");
            }

            return View((object) email);
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

                if (currentUser.AccountType != AccountType.Business)
                {
                    var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
                    if (linkedAccounts.Any())
                    {
                        return RedirectToAction("Index");
                    }
                }
                else if (currentUser.AccountType == AccountType.Business)
                {
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        public ActionResult Settings()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (currentUser.AccountType != AccountType.Business)
            {
                var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
                if (!linkedAccounts.Any(l => l.EmailVerified))
                {
                    return Redirect("/");
                }
            }
            return View();
        }


        public ActionResult Profile(string id)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            var businessAccount = _accountService.GetById(new ObjectId(id));
            ViewBag.Id = "";
            if(!string.IsNullOrEmpty(id))
                ViewBag.Id = id;
            return View(JObject.FromObject(new {Id = id, AccountId = businessAccount.AccountId, CurrentUserAccountType = currentUser.AccountType.ToString()}));
        }


        public ActionResult Billing()
        {
            SubcriptionLogic sl = new SubcriptionLogic();
            var currentuser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (sl.GetSubcriptionByUserId(currentuser.AccountId) == null)
            {
                sl.InsertSubcription(currentuser.AccountId);
            }
            return View();
        }
        public ActionResult ManageRequest()
        {
          
            return View();
        }
        public ActionResult ConfirmSMS(string requestId)
        {
            if (!string.IsNullOrEmpty(requestId))
                ViewBag.RequestId = requestId;
            return View();
        }

        [AllowAnonymous]
        public ActionResult SignIn()
        {
          
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.UserAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                var phone = ViewBag.UserAccount.Profile.PhoneNumber;
                ViewBag.PhoneLast2 = phone.Substring(phone.Length - 2);
                //check
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                var check = _accountService.IsConfirmSMS(User.Identity.GetUserId());
                var otp = account.PhoneVerifiedToken ?? EnumAccount.otpDisable;
                if (otp == EnumAccount.otpDisable)
                    check = true;
                if (check)
                    return Redirect("/BusinessAccount/Index");
              
            }
            return View();
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }
        public ActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUserAccountId = User.Identity.GetUserId();
                var account = new AccountService().GetByAccountId(currentUserAccountId);

                if (User != null || User.Identity != null)
                {
                    new AccountService().ClearSession(User.Identity.GetUserId());
                }
                Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            }
            return RedirectToAction("Index", "User");
        }
     
    }
}