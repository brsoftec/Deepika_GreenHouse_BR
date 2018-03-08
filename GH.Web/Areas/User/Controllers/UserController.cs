using System;
using System.Web.Mvc;
using GH.Core.Models;
using GH.Core.Services;
using Microsoft.AspNet.Identity;
using System.Web;
using GH.Core.BlueCode.Entity.Invite;
using Microsoft.Owin.Security;
using GH.Core.IServices;
using MongoDB.Bson;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NLog;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.BusinessLogic;
using GH.Web.Areas.User.ViewModels;
using NLog.Fluent;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        IAccountService _accountService;
        private IDisabledUserService disabledUserService { get; set; }

        public UserController()
        {
            disabledUserService = new DisabledUserService();
            _accountService = new AccountService();
        }

        public ActionResult Error()
        {
            return View();
        }


        // GET: User/
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAccountService accountService = new AccountService();
                Account account = accountService.GetByAccountId(User.Identity.GetUserId());

                if (account?.AccountType == AccountType.Business)
                {
                    return Redirect("/BusinessAccount");
                }

                try
                {
                    var model = new HomeViewModel
                    {
                        Status = account.Status,
                        NoSecurityQuestions = account.AccountStatus?.NoSecurityQuestions ?? false,
                        Avatar = account.Profile.PhotoUrl,
                        Email = account.Profile.Email,
                        PhoneNumber = account.Profile.PhoneNumber,
                        EmailVerified = account.EmailVerified,
                        PhoneVerified = account.PhoneNumberVerified,
                        ViewPreferences = account.ViewPreferences ?? new AccountViewPreferences
                        {
                            ShowIntroSlides = false,
                            ShowIntroVault = false,
                            ShowBusinessIntroSlides = false,
                            ShowIntroBusiness = false
                        },
                        AccountIncomplete = string.IsNullOrEmpty(account.Profile.PhoneNumber)
                                            || !account.EmailVerified || !account.PhoneNumberVerified
                    };
                    if (account.AccountStatus?.LocationDetected == true)
                    {
                        model.CountryDetected = account.Profile.Country;
                        var country = new LocationBusinessLogic().GetCountryByName(account.Profile.Country);
                        if (country != null)
                        {
                            model.CountryCodeDetected = country.Code.ToLower();
                        }
                    }

                    if (account.Status == "new")
                    {

                        if (!string.IsNullOrEmpty(account.InviteId))
                        {
                            new InviteService().ConvertInviteById(account.InviteId);
                        }

                        accountService.UpdateStatus(account, "starting");
                    }
                    else
                    {
/*                        var check = accountService.IsConfirmSMS(User.Identity.GetUserId());
                        var otp = account.PhoneVerifiedToken ?? EnumAccount.otpDisable;
                        if (otp == EnumAccount.otpDisable)
                            check = true;
                        if (!check)
                        {
                            return Redirect("/User/SignIn");
                        }*/
                    }
                    //else
                    //{
                    //    new NotificationBusinessLogic().SendNotificationMessageGoverment(User.Identity.GetUserId());
                    //}


                    return View(model);
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Debug($"Home controller error: {e.Message}: {e.StackTrace}");
                    return View();
                }

            }
            else
            {
                return Redirect("/About");
            }

        }

        [AllowAnonymous]
        public ActionResult SignUp(string invite)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (!string.IsNullOrEmpty(invite))
                ViewBag.InviteId = invite;
            return View();
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [AllowAnonymous]
        public ActionResult SignUpTimeout()
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

            return Redirect("Index");
        }

        [AllowAnonymous]
        public ActionResult SignUpEx(string id)
        {
            if (!string.IsNullOrEmpty(id))
                ViewBag.Id = id;

            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            return View("Signup");
        }

        [AllowAnonymous]
        public ActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.UserAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                var phone = ViewBag.UserAccount.Profile.PhoneNumber;
                if (!string.IsNullOrEmpty(phone))
                ViewBag.PhoneLast2 = phone.Substring(phone.Length - 2);

                //check
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                var check = _accountService.IsConfirmSMS(User.Identity.GetUserId());
                var otp = account.PhoneVerifiedToken ?? EnumAccount.otpDisable;
                if (otp == EnumAccount.otpDisable)
                    check = true;
                if (check)
                    return Redirect("/User/index");
            }


            return View();
        }


        [AllowAnonymous]
        public ActionResult Trouble()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult VerifyingEmail(string email)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
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
        public ActionResult VerifyEmail(string email, string token)
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.GetOwinContext()
                    .Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType,
                        OAuthDefaults.AuthenticationType);
                return Redirect(HttpContext.Request.Url.OriginalString);
            }

            IAccountService _accountService = new AccountService();
            var model = _accountService.VerifyEmail(email, token);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult DisableUser(string id)
        {
            ObjectId objectId;
            var user = new DisabledUser();
            if (ObjectId.TryParse(id, out objectId))
            {
                user = disabledUserService.GetDisabledUserById(id);
            }

            return View(user);
        }

        [AllowAnonymous]
        public ActionResult DisableUserByEmail(string email)
        {
            var user = disabledUserService.GetDisabledUserByEmail(email);
            return View(user);
        }

        public ActionResult ConfirmEmailSuccess()
        {
            return View();
        }

        public ActionResult ConfirmEmailFailed()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string token)
        {
            return View((object) token);
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SMSAuthencation()
        {
            if (Account != null)
            {
                AccountController acc = new AccountController();
                ViewBag.NoPin = string.IsNullOrEmpty(Account.Profile.PinCode);
                ViewBag.NoPin = acc.IsCheckPinVault();
            }

            return View();
        }
        
        public ActionResult Settings()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAccountService _accountService = new AccountService();
                Account account = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountType == AccountType.Business)
                {
                    return Redirect("/BusinessAccount/Settings");
                }

                ViewBag.ViewPreferences = account.ViewPreferences;
            }

            ViewBag.Title = "Settings";

            return View();
        }

        public ActionResult Network()
        {
            if (User.Identity.IsAuthenticated)
            {
                IAccountService _accountService = new AccountService();
                if (_accountService.GetByAccountId(User.Identity.GetUserId()).AccountType == AccountType.Business)
                {
                    return Redirect("/BusinessAccount");
                }
            }

            ViewBag.Title = "Network";
            return View();
        }

        public ActionResult Notifications()
        {
            return View();
        }

        public ActionResult Support()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Profile(string id)
        {
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            ViewBag.UserAccount = account;
            ViewBag.AccountTier = account.AccountType.ToString();
            return View((object) id);
        }

        [HttpGet]
        public ActionResult Invite()
        {
            return View();
        }


        public ActionResult acceptinvite(string id)
        {
            if (!User.Identity.IsAuthenticated) return Redirect("/About");

            var _networkService = new NetworkService();

            var Id = new MongoDB.Bson.ObjectId(id);

            var invite = _networkService.GetInvitationById(Id);

            var accepter = _accountService.GetById(invite.To);

            if (User.Identity.GetUserId() != accepter.AccountId)
            {
                TempData["error"] = new WebErrorViewModel
                {
                    Name = "Invalid Invitation ID",
                    Message =
                        "You are not authorized to do this operation. Possible cause: you are following an email invitation and log in with the wrong account."
                };

                return View("~/Areas/User/Views/Error/Index.cshtml");
            }

            if (accepter.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You accepted an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(accepter.AccountId, title, type);
            }

            _networkService.AcceptInvitation(Id, accepter.Id);

            return Redirect("/User/Network");
        }
    }
}