using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using GH.Core.Models;
using GH.Web.Providers;
using GH.Web.Results;
using GH.Core.Services;
using System.Linq;
using GH.Web.Extensions;
using GH.Core.Helpers;
using GH.Core.Configurations;
using Newtonsoft.Json.Linq;
using GH.Web.Areas.User.ViewModels;
using GH.Core.Exceptions;
using GH.Core.Collectors;
using GH.Core.IServices;
using GH.Lang;
using MongoDB.Driver;
using GH.Core.BlueCode.BusinessLogic;
using System.IO;
using GH.Core.ViewModels;
using GH.Util;
using GH.Core.BlueCode.Entity.InformationVault;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.Net;
using System.Net.Http.Headers;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.BlueCode.Entity.Notification;
using MongoDB.Bson;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.Entity.ProfilePrivacy;
using NLog;
using System.Reflection;
using System.Text.RegularExpressions;
using Elmah;
using GH.Core.BlueCode.Entity.ManageTokenDevice;
using GH.Core.Adapters;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.AuthToken;
using GH.Core.BlueCode.Entity.Post;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nexmo.Api.Request;
using NLog.Fluent;
using PayPal.Api;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [ApiAuthorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";

        public const string PROFILE_PICTURE_DIRECTORY = "~/Content/ProfilePictures";

        //Authentication
        private ApplicationUserManager _userManager;

        private IAccountService _accountService;
        private INetworkService _networkService;
        private IDisabledUserService _disableUserService;
        private IInfomationVaultBusinessLogic _informationVault;
        private IDelegationBusinessLogic _delegation;
        private IDisabledUserService _disableUserRepository;
        private Logger log = LogManager.GetCurrentClassLogger();

        public AccountController()
        {
            _accountService = new AccountService();
            _networkService = new NetworkService();
            _disableUserRepository = new DisabledUserService();
            _informationVault = new InfomationVaultBusinessLogic();
            _delegation = new DelegationBusinessLogic();
            _disableUserService = new DisabledUserService();
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        [HttpGet, Route("GetPushVaultUser")]
        public async Task<IHttpActionResult> GetPushVaultUser()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            string busid = currentUser.AccountId.ToString();
            var campaignList = new CampaignBusinessLogic()
                .GetActiveCampaignsForUser("", "", "PushToVault", true, "", false).Where(x => x.BusinessUserId == busid)
                .ToList();
            var campaignListPushToVault = campaignList.Where(x => x.CampaignType == "PushToVault").ToList();
            var newFeedList = new List<NewFeedsViewModel>();
            foreach (var item in campaignListPushToVault.OrderByDescending(x => x.Id))
            {
                var newfeed = new NewFeedsViewModel();
                newfeed.UserId = currentUser.AccountId;
                newfeed.CampaignId = item.Id;
                newfeed.CampaignType = item.CampaignType;
                newfeed.Name = item.CampaignName;
                newfeed.CampaignName = item.CampaignName;
                newfeed.Description = item.Description;
                newFeedList.Add(newfeed);
            }

            var rs = new BusinessProfileViewModel
            {
                Id = busid,
                DisplayName = currentUser.Profile.DisplayName,
                ListPushToVault = newFeedList
            };
            return Ok(rs);
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        [AllowAnonymous]
        [HttpGet, Route("ConfirmEmail")]
        public async Task<IHttpActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("User", "User not found");
            }
            else if (user.EmailConfirmed)
            {
                ModelState.AddModelError("AlreadyConfirmed", "Email has already been confirmed");
            }

            if (!ModelState.IsValid)
            {
                return Redirect(CommonFunctions.GetAppHostDomain() + "/User/ConfirmEmailFailed");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                return Redirect(CommonFunctions.GetAppHostDomain() + "/User/ConfirmEmailSuccess");
            }
            else
            {
                return Redirect(CommonFunctions.GetAppHostDomain() + "/User/ConfirmEmailFailed");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SendVerifyEmail")]
        public async Task<IHttpActionResult> SendVerifyEmail(SendVerifyEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByEmail(model.Email);
            if (account == null)
            {
                return BadRequest(Regit.Account_Not_Found_Message);
            }

            account.EmailVerifyToken = Guid.NewGuid().ToString().Replace("-", "");
            account.EmailVerifyTokenDate = DateTime.Now;
            var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}", account.Profile.Email,
                account.EmailVerifyToken);
            var verify = _accountService.SendVerifyEmail(account, url);
            if (verify.Type != VerifyType.SendMailSuccessfully)
            {
                return BadRequest(verify.Message);
            }

            return Ok(verify);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("GetUserInfoExport")]
        public async Task<List<UserInfoExportViewModel>> GetUserInfoExport()
        {
            return _accountService.GetListALlUserEmailVerified().Select(x => new UserInfoExportViewModel
            {
                Email = x.Profile.Email,
                UserName = string.IsNullOrEmpty(x.Profile.DisplayName)
                    ? x.Profile.FirstName + " " + x.Profile.LastName
                    : x.Profile.DisplayName
            }).ToList();
        }

        [HttpGet]
        [Route("GetListAccount")]
        public async Task<IHttpActionResult> GetListAccount(int skip = 0, int take = 50, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var rs = new List<UserViewforAdmin>();

            var lstAccount = _accountService.GetListUser(skip, take, startDate, endDate);

            foreach (var item in lstAccount)
            {
                var acc = GetAccountByAdmin(item.AccountId);

                if (acc != null)
                {
                    acc.CreateDateFormat = String.Format("{0:d/M/yyyy}", item.CreatedDate);

                    rs.Add(acc);
                }
            }

            //var result = rs;
            //foreach(var item in rs)
            //{
            //    var isBelongBusinees = rs.Where(x => x.BusinessAccountRoles.Where(b => b.AccountId.Equals(item.AccountId)).Any() == true).Any();
            //    result.Where(x => x.AccountId.Equals(item.AccountId)).ToList().ForEach(x => x.IsBelongBusinees = isBelongBusinees);
            //}
            return Ok(rs);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAccountByAdmin")]
        public UserViewforAdmin GetAccountByAdmin(string userId)
        {
            try
            {
                var rs = new UserViewforAdmin();
                var identity = ContextPerRequest.Db.Users.FirstOrDefault(x => x.Id.Equals(userId));
                if (identity == null) return null;
                var currentUser = _accountService.GetByAccountId(userId);
                rs.PasswordHash = identity.PasswordHash;
                rs.SecurityStamp = identity.SecurityStamp;
                rs.AccountId = identity.Id;
                rs.AccountType = currentUser.AccountType;
                rs.Profile = currentUser.Profile;
                var lstRole = currentUser.BusinessAccountRoles;
                var Roles = new List<UserRole>();
                foreach (var r in lstRole)
                {
                    var role = new UserRole();
                    var user = _accountService.GetById(r.AccountId);
                    role.AccountId = user.AccountId;
                    if (r.RoleId != null)
                    {
                        role.RoleId = r.RoleId;
                        role.RoleName = _accountService.GetRoleById(r.RoleId);
                    }

                    if (role != null)
                        Roles.Add(role);
                }

                var lstRolesPending = new List<JoiningBusinessInvitation>();
                var invitation = new JoiningBusinessInvitationService();
                if (rs.AccountType == AccountType.Business)
                {
                    lstRolesPending = invitation.GetAllWorkflowInvitationsFromBusiness(currentUser.Id);
                    var RolesPending = _accountService.GetUserRolePending(lstRolesPending);
                    foreach (var rol in RolesPending)
                    {
                        Roles.Add(rol);
                    }
                }
                else
                {
                    if (Roles.Count > 0)
                        rs.IsBelongBusinees = true;
                }

                rs.BusinessAccountRoles = Roles;

                rs.SecurityQuesion1 = currentUser.SecurityQuesion1;
                rs.SecurityQuesion2 = currentUser.SecurityQuesion2;
                rs.SecurityQuesion3 = currentUser.SecurityQuesion3;
                return rs;
            }
            catch (Exception ex)
            {
                log.Debug("Sync:  " + ex.ToString());
            }

            return new UserViewforAdmin {AccountId = userId};
        }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<UserInfoViewModel> GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            var result = new UserInfoViewModel
            {
                UserName = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };

            if (result.HasRegistered)
            {
                var accessToken = GenerateLocalAccessTokenResponse(User.Identity as ClaimsIdentity);
                result.AccessToken = accessToken.GetValue("access_token").Value<string>();
            }

            return result;
        }

        // POST api/Account/Logout
        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout(TokenDeviceViewModel tokenDevice)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var context = HttpContext.Current.GetOwinContext();
            var token = context.Request.Headers["Authorization"].Substring(7);

            var account = _accountService.GetByAccountId(currentUserAccountId);
            var accountId = account.AccountId;
            if (account.AccountActivityLogSettings.RecordAccess)
            {
                string title = "You signed out.";
                string type = "user";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            if (tokenDevice != null)
            {
                if (!string.IsNullOrEmpty(tokenDevice.TokenDevice))
                {
                    var tokenDeviceLogic = new ManageTokenDeviceBusinessLogic();
                    tokenDeviceLogic.UpdateStatus(accountId, tokenDevice.TokenDevice, EnumStatusTokenDevice.Offline);
                }
            }

            new AuthTokensLogic().CloseTokenAsync(token);

            if (User != null || User.Identity != null)
            {
                _accountService.ClearSession(User.Identity.GetUserId());
            }

            
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);

            return Ok();
        }


        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();
            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        [HttpGet, Route("HasLocalPassword")]
        public async Task<bool> HasLocalPassword()
        {
            return await UserManager.HasPasswordAsync(User.Identity.GetUserId());
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

            var currentUserId = User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserId);
            if (!account.PhoneNumberVerified)
            {
                throw new CustomException("Invalid request");
            }

            var identity = ContextPerRequest.Db.Users.Find(currentUserId);

            if (string.IsNullOrEmpty(identity.PasswordHash))
            {
                if (!string.IsNullOrEmpty(model.OldPassword))
                {
                    throw new CustomException("Old password is incorrect");
                }

                IdentityResult result = await UserManager.AddPasswordAsync(currentUserId, model.NewPassword);
                if (!result.Succeeded)
                {
                    throw new CustomException(GetErrorViewModels(result));
                }
            }
            else if (!UserManager.CheckPassword(identity, model.OldPassword))
            {
                throw new CustomException("Old password is incorrect");
            }
            else
            {
                IdentityResult result = await UserManager.ChangePasswordAsync(currentUserId, model.OldPassword,
                    model.NewPassword);

                if (!result.Succeeded)
                {
                    throw new CustomException(GetErrorViewModels(result));
                }

                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "ChangePassword");
            }

            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "You changed password.";
                string type = "settings";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            if (account.AccountActivityLogSettings.RecordAccess)
            {
                string title = "You set password.";
                string type = "settings";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                                                              && ticket.Properties.ExpiresUtc.HasValue
                                                              && ticket.Properties.ExpiresUtc.Value <
                                                              DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [HttpPost]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [Route("SignOutExternal")]
        [AllowAnonymous]
        public async Task SignOutExternal()
        {
            if (User.Identity.IsAuthenticated)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            }
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                var account = _accountService.GetByAccountId(user.Id);
                var isDisabled = _disableUserService.IsDisabled(account.Profile.Email);

                if (isDisabled)
                {
                    var disabledUser = _disableUserService.GetDisabledUserByEmail(account.Profile.Email);
                    var url = Url.Content("~/") + "User/DisableUser?id=" + disabledUser.Id;
                    return Redirect(url);
                }

                var accountInfo = _accountService.GetByAccountId(user.Id);
                if (!accountInfo.EmailVerified)
                {
                    var url = Url.Content("~/") + "User/VerifyingEmail?email=" + accountInfo.Profile.Email;
                    return Redirect(url);
                }

                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties =
                    ApplicationOAuthProvider.CreateProperties(user.UserName, accountInfo);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);

                HttpContext.Current.Response.SetCookie(new HttpCookie("regit-language", accountInfo.Language));
            }
            else
            {
                IList<Claim> claims = externalLogin.GetClaims();
                if (externalLogin.LoginProvider == "Twitter")
                {
                    ITwitterCollector _twitterService = new TwitterCollector();
                    var credentials = await _twitterService.VerifyCredentials(externalLogin.ExternalAccessToken,
                        externalLogin.ExternalAccessTokenSecret, false, false, true);
                    string email = credentials.email;
                    string firstName = credentials.name;
                    string avatar = credentials.profile_image_url;
                    claims.Add(new Claim(ClaimTypes.Email, email));
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        claims.Add(new Claim("FirstName", firstName));
                        claims.Add(new Claim("DisplayName", firstName));
                    }

                    if (!string.IsNullOrEmpty(avatar))
                    {
                        claims.Add(new Claim("Avatar", avatar));
                    }
                }

                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            //WRITE LOG
            if (user != null)
            {
                var account = _accountService.GetByAccountId(user.Id);
                if (account.AccountActivityLogSettings.RecordAccess)
                {
                    string title = "You logged in Regit with other social account.";
                    string type = "user";
                    if (externalLogin.LoginProvider != null)
                        title += externalLogin.LoginProvider.ToString();
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(user.Id, title, type);
                }
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();
            string state;
            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl),
                        state = state
                    }),

                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        [AllowAnonymous]
        [HttpGet, Route("SecurityQuestions")]
        public IEnumerable<SecurityQuestion> GetAllSecurityQuestions()
        {
            ISecurityQuestionService _questionService = new SecurityQuestionService();
            return _questionService.GetAll();
        }

        #region API dataexample

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegisterNormalExampleData")]
        public IHttpActionResult RegisterNormalExampleData(RegisterBindingModel model, int randomkeyword)
        {
            ValidateRegistrationInfoExampleData(model.Account);
            ISecurityQuestionService _questionService = new SecurityQuestionService();
            var allQuestions = _questionService.GetAll();
            var formatedNumber = model.Account.PhoneNumber;
            var user = new ApplicationUser() {UserName = model.Account.Email, Email = model.Account.Email};
            IdentityResult result = UserManager.Create(user, model.Account.Password);
            if (!result.Succeeded)
            {
                throw new CustomException(GetErrorViewModels(result));
            }

            string name = user.Id + "_profile_pic_" + DateTime.Now.Ticks;
            name += Path.GetExtension(model.Account.FileName);

            if (!string.IsNullOrEmpty(model.Account.Avatar))
            {
                FileAccessHelper.SaveBase64String(model.Account.Avatar, PROFILE_PICTURE_DIRECTORY, name);
            }

            var account = new Account
            {
                AccountActivityLogSettings = new AccountActivityLogSettings
                {
                    RecordInteraction = true,
                    RecordSocialActivity = true
                },
                AccountId = user.Id,
                AccountPrivacies = new AccountPrivacies
                {
                    AutoDeleteMessage = false,
                    FindMe = true,
                    NotFollowBusinessSendMeAds = true,
                    SendMeMessage = SendMeMessagePrivacy.All,
                    ShareMyActivity = true,
                    ViewMyProfile = true
                },
                EmailVerified = !ConfigHelp.GetBoolValue("IsCheckValidEmail"),
                PhoneNumberVerified = true,
                CreatedDate = DateTime.Now,
                Profile = new Profile
                {
                    Gender = model.Account.Gender,
                    Email = user.Email,
                    FirstName = model.Account.FirstName,
                    LastName = model.Account.LastName,
                    DisplayName = model.Account.FirstName + " " + model.Account.LastName,
                    Birthdate = model.Account.Birthday,
                    City = model.Account.City,
                    Country = model.Account.Country,
                    PhoneNumber = formatedNumber,
                    PhotoUrl = !string.IsNullOrEmpty(model.Account.Avatar)
                        ? PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name
                        : string.Empty,
                    Description = model.Account.AboutMe
                },

                NotificationSettings = new AccountNotificationSettings()
                {
                    EventAndReminders = true,
                    Interactions = true,
                    NetworkRequest = true,
                    Workflow = true
                }
            };
            var langCookie = HttpContext.Current.Request.Cookies.Get("regit-language");
            if (langCookie != null)
            {
                var languages = JArray
                    .Parse(File.ReadAllText(CommonFunctions.MapPath("~/Areas/User/Configs/languages.json")))
                    .ToObject<dynamic[]>();
                if (languages.Any(l => l.Code == langCookie.Value))
                {
                    account.Language = langCookie.Value;
                }
            }

            _accountService.Insert(account);
            _informationVault.AddInformationVault(account.AccountId);
            List<FieldinformationVault> listFieldinformationVault = new List<FieldinformationVault>();
            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.firstName",
                type = "textbox",
                model = model.Account.FirstName,
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.title",
                type = "textbox",
                model = model.Account.FirstName + model.Account.LastName + "title",
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.lastName",
                type = "textbox",
                model = model.Account.LastName,
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.gender",
                type = "textbox",
                model = model.Account.Gender,
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.dob",
                type = "textbox",
                model = model.Account.Birthday.HasValue ? model.Account.Birthday.Value.ToString("dd-MM-yyyy") : ""
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.country",
                type = "textbox",
                model = model.Account.Country
            });
            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".basicInformation.city",
                type = "textbox",
                model = model.Account.City
            });
            //vault keywords
            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".others.preference.food",
                type = "smartinput",
                model = string.Join(",", ExampleDataHelper.GetRandomListKeywords("food", randomkeyword))
            });


            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".others.favourite.colour",
                type = "smartinput",
                model = string.Join(",", ExampleDataHelper.GetRandomListKeywords("colour", randomkeyword))
            });


            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".others.favourite.holiday",
                type = "smartinput",
                model = string.Join(",", ExampleDataHelper.GetRandomListKeywords("holiday", randomkeyword))
            });

            listFieldinformationVault.Add(new FieldinformationVault
            {
                jsPath = ".others.favourite.music_type",
                type = "smartinput",
                model = string.Join(",", ExampleDataHelper.GetRandomListKeywords("music", randomkeyword))
            });

            _informationVault.UpdateInformationVaultById(account.AccountId, listFieldinformationVault);

            var normalNetwork = new Network
            {
                NetworkOwnerAccountId = User.Identity.GetUserId(),
                Code = Network.NORMAL,
                Name = Network.NORMAL_NETWORK,
                NetworkOwner = account.Id
            };

            var trustedNetwork = new Network
            {
                NetworkOwnerAccountId = User.Identity.GetUserId(),
                Code = Network.TRUSTED,
                Name = Network.TRUSTED_NETWORK,
                NetworkOwner = account.Id
            };
            _networkService.InsertNetwork(normalNetwork);
            _networkService.InsertNetwork(trustedNetwork);

            if (user != null)
            {
                if (account.AccountActivityLogSettings.RecordAccess)
                {
                    string title = "You registered a business account.";
                    string type = "user";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(user.Id, title, type);
                }
            }

            if (!account.EmailVerified)
            {
                account.EmailVerifyToken = Guid.NewGuid().ToString().Replace("-", "");
                account.EmailVerifyTokenDate = DateTime.Now;
                var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                    account.Profile.Email,
                    account.EmailVerifyToken);

                var verify = _accountService.SendVerifyEmail(account, url);

                if (verify.Type != VerifyType.SendMailSuccessfully)
                {
                    return BadRequest(verify.Message);
                }

                return Ok(verify);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost, Route("ValidateRegistrationInfoExampleData")]
        public IHttpActionResult ValidateRegistrationInfoExampleData(RegistrationInfo model)
        {
            var errors = new List<ErrorViewModel>();

            bool dupEmail = _accountService.CheckDuplicateEmail(model.Email, null);
            if (dupEmail)
            {
                errors.Add(new ErrorViewModel
                {
                    Error = ErrorCode.AUTH_EmailDuplicated,
                    Message = GH.Lang.Regit.Email_Error_Not_Available
                });
            }

            if (errors.Any())
            {
                throw new CustomException(errors.ToArray());
            }

            return Ok();
        }


        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegistersManyUsersWithExampleData")]
        public IHttpActionResult RegistersManyUsersWithExampleData(RegistersManyUsersWithExampleDataModel model)
        {
            if (model == null)
            {
                model = new RegistersManyUsersWithExampleDataModel();
            }

            if (model.CountUsers == 0)
                model.CountUsers = 100;
            if (model.NmberRandomKeyword == 0)
                model.NmberRandomKeyword = 4;
            if (string.IsNullOrEmpty(model.Firstname))
                model.Firstname = "User";
            if (string.IsNullOrEmpty(model.Lastname))
                model.Lastname = "Normal";
            var listcountry = new GH.Core.BlueCode.Entity.Profile.Country[]
            {
                new GH.Core.BlueCode.Entity.Profile.Country
                {
                    Name = "Singapore",
                    Code = "SG"
                },
                new GH.Core.BlueCode.Entity.Profile.Country
                {
                    Name = "United States",
                    Code = "US"
                },
            };

            LocationBusinessLogic locationBusinessLogic = new LocationBusinessLogic();

            ExampleDataHelper.Alls = 0;
            ExampleDataHelper.Females = 0;
            ExampleDataHelper.Males = 0;

            for (int i = 0; i < model.CountUsers; i++)
            {
                var countryrandom = ExampleDataHelper.GetRandomObjectFromList(1, listcountry.ToList<object>())
                    .Select(x => (GH.Core.BlueCode.Entity.Profile.Country) x).ToList();
                var listcity = locationBusinessLogic.GetCityByCountryCode(countryrandom[0].Code);

                var cityrandom = ExampleDataHelper.GetRandomObjectFromList(1, listcity.ToList<object>())
                    .Select(x => (GH.Core.BlueCode.Entity.Profile.City) x).ToList();

                var modeluser = new RegisterBindingModel
                {
                    Account = new RegistrationInfo
                    {
                        Gender = ExampleDataHelper.GetRandomGender(model.CountUsers),
                        Birthday = DateTime.ParseExact(ExampleDataHelper.GetRandomDateDOB(), "dd-MM-yyyy",
                            CultureInfo.InvariantCulture),
                        FirstName = model.Firstname + i,
                        LastName = model.Lastname + i,
                        Email = model.Firstname + model.Lastname + i + "@gmail.com",
                        PhoneNumber = ExampleDataHelper.GetRandomPhoneNumber("+84"),
                        Password = "Test@123",
                        Country = countryrandom[0].Name,
                        City = cityrandom[0].Name
                    }
                };
                var result = RegisterNormalExampleData(modeluser, model.NmberRandomKeyword);
            }

            return Ok();
        }

        #endregion

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SignupModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        // POST api/Account/Signup
        [AllowAnonymous]
        [Route("Signup")]
        public async Task<HttpResponseMessage> SignupAsync(SignupModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);
            var email = model.Email?.ToLower();
            if (string.IsNullOrEmpty(email))
                return Request.CreateApiErrorResponse("Missing email address");
            if (string.IsNullOrEmpty(model.FirstName))
                return Request.CreateApiErrorResponse("Missing first name");
            if (string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing password");
            if (model.Password.Length < 8)
                return Request.CreateApiErrorResponse("Password too short");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Request.CreateApiErrorResponse(result.Errors?.FirstOrDefault(), error: "signup.create.error");
            }

            var account = new Account
            {
                Status = "new",
                AccountActivityLogSettings = new AccountActivityLogSettings
                {
                    RecordAccess = true,
                    RecordProfile = true,
                    RecordAccount = true,
                    RecordNetwork = true,
                    RecordVault = true,
                    RecordDelegation = true,
                    RecordInteraction = true,
                    RecordSocialActivity = true,

                    RecordProfileBusiness = true,
                    RecordAccountBusiness = true,
                    RecordBusinessSystem = true,
                    RecordCampaign = true,
                    RecordWorkflow = true
                },
                AccountId = user.Id,
                AccountStatus = new AccountStatus
                {
                    NoSecurityQuestions = true
                },
                AccountPrivacies = new AccountPrivacies
                {
                    AutoDeleteMessage = false,
                    FindMe = true,
                    NotFollowBusinessSendMeAds = true,
                    SendMeMessage = SendMeMessagePrivacy.All,
                    ShareMyActivity = true,
                    ViewMyProfile = true
                },
                EmailVerified = false,
                PhoneNumberVerified = false,
                CreatedDate = DateTime.Now,
                Profile = new Profile
                {
                    Email = email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DisplayName = model.FirstName + " " + model.LastName,
                },
                ViewPreferences = new AccountViewPreferences
                {
                    ShowIntroSlides = true,
                    ShowBusinessIntroSlides = true,
                    ShowIntroVault = true,
                    ShowIntroBusiness = true,
                },
                NotificationSettings = new AccountNotificationSettings()
                {
                    EventAndReminders = true,
                    Interactions = true,
                    NetworkRequest = true,
                    Workflow = true
                }
            };

            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
            if (ip == "::1") ip = "116.102.131.64";
            if (!string.IsNullOrEmpty(ip))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync($"https://ipinfo.io/{ip}/geo");

                    var json = await response.Content.ReadAsStringAsync();
                    JObject resp;
                    try
                    {
                        resp = JObject.Parse(json);
                    }
                    catch
                    {
                        resp = null;
                    }

                    var cc = resp?.SelectToken("country").ToString();
                    //var city = resp.SelectToken("city");
                    if (cc != null)
                    {
                        var country = new LocationBusinessLogic().GetCountryByCountryCode(cc);
                        if (country != null)
                        {
                            account.Profile.Country = country.Name;
                            account.AccountStatus.LocationDetected = true;
                        }
                    }
                }
            }

            account.EmailVerifyToken = Guid.NewGuid().ToString().Replace("-", "");
            account.EmailVerifyTokenDate = DateTime.Now;
            var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                account.Profile.Email,
                account.EmailVerifyToken);
            var verify = _accountService.SendVerifyEmail(account, url);
            if (verify.Type != VerifyType.SendMailSuccessfully)
            {
                //   return Request.CreateApiErrorResponse(verify.Message);
            }

            _accountService.Insert(account);

            _informationVault.AddInformationVault(account.AccountId);
            BasicProfile basicP = new BasicProfile();
            basicP.UserId = account.AccountId;
            basicP.FirstName = model.FirstName;
            basicP.LastName = model.LastName;
            basicP.Email = email;
            _informationVault.StartByAccountId(account.AccountId, basicP);

            var acc = _accountService.GetByAccountId(account.AccountId);
            var normalNetwork = new Network
            {
                NetworkOwnerAccountId = account.AccountId,
                Code = Network.NORMAL,
                Name = Network.NORMAL_NETWORK,
                NetworkOwner = acc.Id
            };

            var trustedNetwork = new Network
            {
                NetworkOwnerAccountId = account.AccountId,
                Code = Network.TRUSTED,
                Name = Network.TRUSTED_NETWORK,
                NetworkOwner = acc.Id
            };

            _networkService.InsertNetwork(normalNetwork);
            _networkService.InsertNetwork(trustedNetwork);

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "New Account");


            return Request.CreateSuccessResponse(new
            {
                accountId = account.AccountId
            }, "User signed up successfully");
        }

        [AllowAnonymous]
        [Route("VerifyEmail/Resend")]
        public HttpResponseMessage ResendVerifyEmailPublic(string email)
        {
            var account = _accountService.GetByEmail(email);
            if (account == null)
                return Request.CreateApiErrorResponse("Account not found", error: "account.notFound");
            var token = account.EmailVerifyToken ?? Guid.NewGuid().ToString().Replace("-", "");
            account.EmailVerifyTokenDate = DateTime.Now;
            var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                account.Profile.Email,
                token);
            var verify = _accountService.SendVerifyEmail(account, url);
            if (verify.Type != VerifyType.SendMailSuccessfully)
            {
                return Request.CreateApiErrorResponse("Error sending verification email");
            }

            return Request.CreateSuccessResponse(new
            {
                tokenId = token
            }, "Verification email resent successfully");
        }

        [Route("VerifyEmail/Resend")]
        public HttpResponseMessage ResendVerifyEmail()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);
            var token = account.EmailVerifyToken ?? Guid.NewGuid().ToString().Replace("-", "");
            account.EmailVerifyTokenDate = DateTime.Now;
            var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                account.Profile.Email,
                token);
            var verify = _accountService.SendVerifyEmail(account, url);
            if (verify.Type != VerifyType.SendMailSuccessfully)
            {
                return Request.CreateApiErrorResponse("Error sending verification email");
            }

            return Request.CreateSuccessResponse(new
            {
                tokenId = token
            }, "Verification email resent successfully");
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<HttpResponseMessage> Register(RegisterBindingModel model)
        {
//            if (!ModelState.IsValid)
//            {
//                throw new CustomException(GetErrorViewModels(ModelState));
//            }

            //validate account info
            await ValidateRegistrationInfo(model.Account);
            //validate security questions id are exist in system
            //            ISecurityQuestionService _questionService = new SecurityQuestionService();
            //            var allQuestions = _questionService.GetAll();
            //            var questionIds = new string[] { model.SecurityQuestions.Question1.QuestionId, model.SecurityQuestions.Question2.QuestionId, model.SecurityQuestions.Question3.QuestionId };
            //            if (questionIds.Any(qid => !allQuestions.Any(q => q.Id.ToString() == qid)))
            //            {
            //                throw new CustomException(new ErrorViewModel { Message = "Security question does not exist" });
            //            }

            var formatedNumber =
                PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.Account.PhoneNumberCountryCallingCode +
                                                         model.Account.PhoneNumber);

            //start create account process
            var user = new ApplicationUser()
            {
                UserName = model.Account.Email.ToLower(),
                Email = model.Account.Email.ToLower()
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Account.Password);

            if (!result.Succeeded)
            {
                return Request.CreateApiErrorResponse(result.Errors?.FirstOrDefault());
            }


            string name = user.Id + "_profile_pic_" + DateTime.Now.Ticks;
            name += Path.GetExtension(model.Account.FileName);

            if (!string.IsNullOrEmpty(model.Account.Avatar))
                try
                {
                    FileAccessHelper.SaveBase64String(model.Account.Avatar, PROFILE_PICTURE_DIRECTORY, name);
                }
                catch (Exception e)
                {
                    model.Account.Avatar = string.Empty;
                }


            var account = new Account
            {
                Status = "new",
                InviteId = model.InviteId,

                AccountActivityLogSettings = new AccountActivityLogSettings
                {
                    RecordAccess = true,
                    RecordProfile = true,
                    RecordAccount = true,
                    RecordNetwork = true,
                    RecordVault = true,
                    RecordDelegation = true,
                    RecordInteraction = true,
                    RecordSocialActivity = true,

                    RecordProfileBusiness = true,
                    RecordAccountBusiness = true,
                    RecordBusinessSystem = true,
                    RecordCampaign = true,
                    RecordWorkflow = true
                },
                AccountId = user.Id,
                AccountStatus = new AccountStatus
                {
                    ToConvertInvite = !String.IsNullOrEmpty(model.InviteId),
                    NoSecurityQuestions = true
                },
                AccountPrivacies = new AccountPrivacies
                {
                    AutoDeleteMessage = false,
                    FindMe = true,
                    NotFollowBusinessSendMeAds = true,
                    SendMeMessage = SendMeMessagePrivacy.All,
                    ShareMyActivity = true,
                    ViewMyProfile = true
                },
                EmailVerified = !ConfigHelp.GetBoolValue("IsCheckValidEmail"),
                PhoneNumberVerified = true,
                CreatedDate = DateTime.Now,
                Profile = new Profile
                {
                    Gender = model.Account.Gender,
                    Email = user.Email.ToLower(),
                    FirstName = model.Account.FirstName,
                    LastName = model.Account.LastName,
                    DisplayName = model.Account.FirstName + " " + model.Account.LastName,
                    Birthdate = model.Account.Birthday,
                    City = model.Account.City,
                    Country = model.Account.Country,
                    PinCode = "", //HttpContext.Current.Session["PinCode"].ToString(),
                    PhoneNumber = formatedNumber,
                    PhotoUrl = !string.IsNullOrEmpty(model.Account.Avatar)
                        ? PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name
                        : string.Empty,
                    Description = model.Account.AboutMe
                },
                ViewPreferences = new AccountViewPreferences
                {
                    ShowIntroSlides = true,
                    ShowBusinessIntroSlides = true,
                    ShowIntroVault = true,
                    ShowIntroBusiness = true,
                },

                NotificationSettings = new AccountNotificationSettings()
                {
                    EventAndReminders = true,
                    Interactions = true,
                    NetworkRequest = true,
                    Workflow = true
                }
            };

            var langCookie = HttpContext.Current.Request.Cookies.Get("regit-language");
            if (langCookie != null)
            {
                var languages = JArray
                    .Parse(File.ReadAllText(CommonFunctions.MapPath("~/Areas/User/Configs/languages.json")))
                    .ToObject<dynamic[]>();
                if (languages.Any(l => l.Code == langCookie.Value))
                {
                    account.Language = langCookie.Value;
                }
            }

            var invitedDelegationId = "";
            try
            {
                invitedDelegationId =
                    Util.UrlHelper.GetParameterFromUrlString(HttpContext.Current.Request.Headers["Referer"],
                        "delegationid");
            }
            catch
            {
            }

            _accountService.Insert(account, invitedDelegationId);
            _accountService.FollowRegit(account.AccountId);

            string warning = null;
            var sqa = model.SecurityQuestionsAnswers;
            if (sqa != null)
            {
                if (sqa.Count != 3)
                    warning = "expects exactly 3 security questions";
                else
                {
                    var sqResult = await _accountService.SetSecurityQuestionsAsync(sqa, account);
                    if (!sqResult.Success)
                        warning = sqResult.Message;
                }
            }

            _informationVault.AddInformationVault(account.AccountId);
            BasicProfile basicP = new BasicProfile();
            basicP.UserId = account.AccountId;
            basicP.FirstName = string.IsNullOrEmpty(model.Account.FirstName) ? "" : model.Account.FirstName;
            basicP.LastName = string.IsNullOrEmpty(model.Account.LastName) ? "" : model.Account.LastName;
            basicP.Gender = string.IsNullOrEmpty(model.Account.Gender) ? "" : model.Account.Gender;
            basicP.City = string.IsNullOrEmpty(model.Account.City) ? "" : model.Account.City;
            basicP.Country = string.IsNullOrEmpty(model.Account.Country) ? "" : model.Account.Country;
            basicP.Email = string.IsNullOrEmpty(model.Account.Email) ? "" : model.Account.Email;
            basicP.Phone = string.IsNullOrEmpty(formatedNumber) ? "" : formatedNumber;
            if (model.Account.Birthday != null)
            {
                basicP.BirthDay = model.Account.Birthday;
            }

            _informationVault.StartByAccountId(account.AccountId, basicP);
            var acc = _accountService.GetByAccountId(account.AccountId);
            var normalNetwork = new Network
            {
                NetworkOwnerAccountId = account.AccountId,
                Code = Network.NORMAL,
                Name = Network.NORMAL_NETWORK,
                NetworkOwner = account.Id
            };

            var trustedNetwork = new Network
            {
                NetworkOwnerAccountId = account.AccountId,
                Code = Network.TRUSTED,
                Name = Network.TRUSTED_NETWORK,
                NetworkOwner = account.Id
            };

            _networkService.InsertNetwork(normalNetwork);
            _networkService.InsertNetwork(trustedNetwork);

            if (!string.IsNullOrWhiteSpace(model.OutsiteId))
            {
                InsertInfoFromOutsite(model.OutsiteId, account);
            }

            if (!string.IsNullOrEmpty(invitedDelegationId))
            {
                var invitedDelegation = _delegation.GetDelegationById(invitedDelegationId);
                if (invitedDelegation != null)
                    _networkService.DelegateeJoinsTrustedNetwork(invitedDelegation.ToAccountId,
                        invitedDelegation.FromAccountId);
            }

            if (user != null)
            {
                if (account.AccountActivityLogSettings.RecordAccess)
                {
                    string title = "You registered an individual account.";
                    string type = "user";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }
            //delta.regit.today:5610/api/syncaccount/5b327d4d-abdc-4741-b33d-70d95c869f4a

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "New Account");

            if (!account.EmailVerified)
            {
                account.EmailVerifyToken = Guid.NewGuid().ToString().Replace("-", "");
                account.EmailVerifyTokenDate = DateTime.Now;
                var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                    account.Profile.Email,
                    account.EmailVerifyToken);
                var verify = _accountService.SendVerifyEmail(account, url);
                if (verify.Type != VerifyType.SendMailSuccessfully)
                {
                    return Request.CreateApiErrorResponse(verify.Message);
                }

                return Request.CreateSuccessResponse(verify, "User signed up successfully. Verification email sent");
            }

            //HttpClient client = new HttpClient();
            //client.PostAsync("http:localhost:50/",new {
            //    FirstName
            //})
            //
            if (warning != null)
                return Request.CreateSuccessResponse(new
                {
                    accountId = account.AccountId,
                    warning = $"Security answers not updated: {warning}"
                }, "User signed up succesfully");
            return Request.CreateSuccessResponse(new {accountId = account.AccountId}, "User signed up succesfully");
        }

        [AllowAnonymous]
        [HttpGet, Route("Exists")]
        public IHttpActionResult CheckAccountExists([FromUri] string email)
        {
            var errors = new List<ErrorViewModel>();

            bool exists = false;

            if (!string.IsNullOrEmpty(email))
            {
                exists = _accountService.CheckDuplicateEmail(email, null);
            }

            if (exists)
                return Ok(new
                {
                    success = true,
                    message = "Found account with that email",

                    accountEmail = email
                });

            return Ok(new
            {
                success = false,
                message = "Found no account with that email",
                email = email
            });
        }

        [AllowAnonymous]
        [HttpGet, Route("Query")]
        public HttpResponseMessage QueryAccount([FromUri] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Request.CreateApiErrorResponse("Missing email key", HttpStatusCode.BadRequest,
                    "account.email.missing");
            }

            var account = _accountService.GetByEmail(email);
            if (account == null)
                return Request.CreateApiErrorResponse("Account not found", error: "account.notFound");

            var phone = account.Profile.PhoneNumber;
            var phoneLast4 = phone?.Length > 3 ? phone.Substring(phone.Length - 4, 4) : phone;
            return Request.CreateSuccessResponse(new
            {
                id = account.Id.ToString(),
                accountId = account.AccountId,
                displayName = account.Profile.DisplayName,
                avatar = account.Profile.PhotoUrl,
                country = account.Profile.Country,
                city = account.Profile.City,
                phoneLast4,
                unverified = !account.EmailVerified
            }, "Account exists");
        }

        [AllowAnonymous]
        [HttpPost, Route("ValidateRegistrationInfo")]
        public async Task<IHttpActionResult> ValidateRegistrationInfo(RegistrationInfo model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

            var errors = new List<ErrorViewModel>();

            bool dupEmail = _accountService.CheckDuplicateEmail(model.Email, null);
            if (dupEmail)
            {
                errors.Add(new ErrorViewModel
                {
                    Error = ErrorCode.AUTH_EmailDuplicated,
                    Message = GH.Lang.Regit.Email_Error_Not_Available
                });
            }

            var result = await UserManager.PasswordValidator.ValidateAsync(model.Password);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e =>
                    new ErrorViewModel {Error = ErrorCode.AUTH_PasswordInvalid, Message = e}));
            }

            try
            {
                PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.PhoneNumberCountryCallingCode + model.PhoneNumber);
            }
            catch (CustomException ex)
            {
                errors.AddRange(ex.Errors.Select(e =>
                    new ErrorViewModel {Error = ErrorCode.AUTH_PhoneNumberInvalid, Message = e.Message}));
            }

            if (errors.Any())
            {
                throw new CustomException(errors.ToArray());
            }

            return Ok();
        }


        [AllowAnonymous]
        [HttpPost, Route("ValidateExternalRegistrationInfo")]
        public async Task<IHttpActionResult> ValidateExternalRegistrationInfo(ExternalRegistrationInfo model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

            var errors = new List<ErrorViewModel>();

            try
            {
                PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.PhoneNumberCountryCallingCode + model.PhoneNumber);
            }
            catch (CustomException ex)
            {
                errors.AddRange(ex.Errors.Select(e =>
                    new ErrorViewModel {Error = ErrorCode.AUTH_PhoneNumberInvalid, Message = e.Message}));
            }

            if (errors.Any())
            {
                throw new CustomException(errors.ToArray());
            }

            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new CustomException(new ErrorViewModel
                {
                    Error = ErrorCode.InternalServerError,
                    Message = "Someting went wrong!"
                });
            }

            var exInfo = await GetExternalLoginInfo();

            if (info.Email == null)
            {
                info.Email = exInfo.Email;
            }

            string formatedNumber = null;

            if (!exInfo.Skip)
            {
                if (!ModelState.IsValid)
                {
                    throw new CustomException(GetErrorViewModels(ModelState));
                }

                //validate account info
                await ValidateExternalRegistrationInfo(model.Account);

                //validate security questions id are exist in system
                ISecurityQuestionService _questionService = new SecurityQuestionService();
                var allQuestions = _questionService.GetAll();

                var questionIds = new string[]
                {
                    model.SecurityQuestions.Question1.QuestionId, model.SecurityQuestions.Question2.QuestionId,
                    model.SecurityQuestions.Question3.QuestionId
                };
                if (questionIds.Any(qid => !allQuestions.Any(q => q.Id.ToString() == qid)))
                {
                    throw new CustomException(new ErrorViewModel {Message = "Security question does not exist"});
                }

                formatedNumber =
                    PhoneNumberHelper.GetFormatedPhoneNumber(
                        "+" + model.Account.PhoneNumberCountryCallingCode + model.Account.PhoneNumber);
                /* Disable Nexmo
                //check for submit requestId is for registration phone number
                var searchResponse = Nexmo.Api.NumberVerify.Search(new Nexmo.Api.NumberVerify.SearchRequest { request_id = model.Authentication.RequestId });

                if (!string.IsNullOrEmpty(searchResponse.error_text))
                    throw new CustomException(searchResponse.error_text);

                if (searchResponse.number != formatedNumber.Trim('+'))
                {
                    throw new CustomException("Invalid request");
                }

                if (!searchResponse.checks.Any(c => c.code == model.Authentication.PIN && c.status == "VALID"))
                {
                    throw new CustomException("Invalid authentication");
                }

                if (searchResponse.status != "SUCCESS")
                {
                    throw new CustomException("Invalid authentication");
                }
                */
            }

            //pass all validation
            //start create account process
            var user = new ApplicationUser() {UserName = info.Email, Email = info.Email};
            bool isNew = true;

            if (string.IsNullOrEmpty(info.Email))
            {
                user.UserName = info.Login.LoginProvider + "-" + info.Login.ProviderKey;

                IdentityResult result = await UserManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new CustomException(GetErrorViewModels(result));
                }
            }
            else
            {
                var existUser = await UserManager.FindByEmailAsync(info.Email);
                if (existUser != null)
                {
                    user = existUser;
                    isNew = false;
                }
                else
                {
                    IdentityResult result = await UserManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new CustomException(GetErrorViewModels(result));
                    }
                }
            }

            var addLoginResult = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!addLoginResult.Succeeded)
            {
                throw new CustomException(GetErrorViewModels(addLoginResult));
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            AccountLink accountLink = null;
            if (info.Login.LoginProvider == "Facebook")
            {
                if (!_accountService.HasPersonalAccountLinkWithSocial(SocialType.Facebook, info.Login.ProviderKey))
                {
                    IFacebookCollector _facebookService = new FacebookCollector();
                    var facebookToken = await _facebookService.GetLongLivedToken(externalLogin.ExternalAccessToken);
                    accountLink = new AccountLink
                    {
                        AccessToken = facebookToken,
                        Type = SocialType.Facebook,
                        SocialAccountId = info.Login.ProviderKey
                    };
                }
            }
            else if (info.Login.LoginProvider == "Twitter")
            {
                if (!_accountService.HasAccountLinkWithSocial(SocialType.Twitter, info.Login.ProviderKey))
                {
                    accountLink = new AccountLink
                    {
                        AccessToken = externalLogin.ExternalAccessToken,
                        AccessTokenSecret = externalLogin.ExternalAccessTokenSecret,
                        Type = SocialType.Twitter,
                        SocialAccountId = info.Login.ProviderKey,
                        TwitterName = externalLogin.TwitterName
                    };
                }
            }

            if (isNew)
            {
                var account = new Account();
                if (accountLink != null)
                {
                    account.AccountLinks.Add(accountLink);
                }

                string name = user.Id + "_profile_pic_" + DateTime.Now.Ticks;
                name += Path.GetExtension(model.Account.FileName);
                var isNewProfilePicture = !string.IsNullOrEmpty(model.Account.Avatar) &&
                                          !string.IsNullOrEmpty(model.Account.FileName);
                if (isNewProfilePicture)
                {
                    FileAccessHelper.SaveBase64String(model.Account.Avatar, PROFILE_PICTURE_DIRECTORY, name);
                }

                account.SocialAccountId = info.Login.ProviderKey;
                account.AccountActivityLogSettings = new AccountActivityLogSettings
                {
                    RecordAccess = true,
                    RecordProfile = true,
                    RecordAccount = true,
                    RecordNetwork = true,
                    RecordVault = true,
                    RecordDelegation = true,
                    RecordInteraction = true,
                    RecordSocialActivity = true,

                    RecordProfileBusiness = true,
                    RecordAccountBusiness = true,
                    RecordBusinessSystem = true,
                    RecordCampaign = true,
                    RecordWorkflow = true
                };
                account.AccountId = user.Id;
                account.AccountPrivacies = new AccountPrivacies
                {
                    AutoDeleteMessage = false,
                    FindMe = true,
                    NotFollowBusinessSendMeAds = true,
                    SendMeMessage = SendMeMessagePrivacy.All,
                    ShareMyActivity = true,
                    ViewMyProfile = true
                };
                account.EmailVerified = false;
                account.PhoneNumberVerified = true;
                account.CreatedDate = DateTime.Now;
                account.Profile = new Profile
                {
                    Gender = model.Account.Gender,
                    Email = model.Account.Email,
                    FirstName = model.Account.FirstName,
                    LastName = model.Account.LastName,
                    DisplayName = model.Account.DisplayName,
                    Birthdate = model.Account.Birthday,
                    City = model.Account.City,
                    Country = model.Account.Country,
                    PhotoUrl = isNewProfilePicture
                        ? PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name
                        : model.Account.Avatar,
                    PhoneNumber = formatedNumber,
                    Description = model.Account.AboutMe
                };

                account.SecurityQuesion1 = new SecurityQuestionAnswer
                {
                    Answer = model.SecurityQuestions.Question1.Answer,
                    QuestionId = new MongoDB.Bson.ObjectId(model.SecurityQuestions.Question1.QuestionId)
                };
                account.SecurityQuesion2 = new SecurityQuestionAnswer
                {
                    Answer = model.SecurityQuestions.Question2.Answer,
                    QuestionId = new MongoDB.Bson.ObjectId(model.SecurityQuestions.Question2.QuestionId)
                };
                account.SecurityQuesion3 = new SecurityQuestionAnswer
                {
                    Answer = model.SecurityQuestions.Question3.Answer,
                    QuestionId = new MongoDB.Bson.ObjectId(model.SecurityQuestions.Question3.QuestionId)
                };
                account.NotificationSettings = new AccountNotificationSettings()
                {
                    EventAndReminders = true,
                    Interactions = true,
                    NetworkRequest = true,
                    Workflow = true
                };

                var langCookie = HttpContext.Current.Request.Cookies.Get("regit-language");
                if (langCookie != null)
                {
                    var languages = JArray
                        .Parse(File.ReadAllText(CommonFunctions.MapPath("~/Areas/User/Configs/languages.json")))
                        .ToObject<dynamic[]>();
                    if (languages.Any(l => l.Code == langCookie.Value))
                    {
                        account.Language = langCookie.Value;
                    }
                }

                account = _accountService.Insert(account);
                _informationVault.AddInformationVault(account.AccountId);

                var normalNetwork = new Network
                {
                    NetworkOwnerAccountId = User.Identity.GetUserId(),
                    Code = Network.NORMAL,
                    Name = Network.NORMAL_NETWORK,
                    NetworkOwner = account.Id
                };

                var trustedNetwork = new Network
                {
                    NetworkOwnerAccountId = User.Identity.GetUserId(),
                    Code = Network.TRUSTED,
                    Name = Network.TRUSTED_NETWORK,
                    NetworkOwner = account.Id
                };
                _networkService.InsertNetwork(normalNetwork);
                _networkService.InsertNetwork(trustedNetwork);

                account.EmailVerifyToken = Guid.NewGuid().ToString().Replace("-", "");
                account.EmailVerifyTokenDate = DateTime.Now;
                var url = string.Format(Url.Content("~/") + "User/VerifyEmail?email={0}&token={1}",
                    account.Profile.Email,
                    account.EmailVerifyToken);

                var verify = _accountService.SendVerifyEmail(account, url);
                if (verify.Type != VerifyType.SendMailSuccessfully)
                {
                    return BadRequest(verify.Message);
                }
            }
            else if (accountLink != null)
            {
                var account = _accountService.GetByAccountId(user.Id);

                if (!account.AccountLinks.Any(a => a.Type == accountLink.Type))
                {
                    _accountService.LinkAccount(accountLink, account.Id);
                }
            }

            //WRITE LOG 
            if (user != null)
            {
                var account = _accountService.GetByAccountId(user.Id);
                string type = "user";
                if (account.AccountActivityLogSettings.RecordAccess)
                {
                    string title = "Your account Regit was registered with social account.";
                    if (info.Login.LoginProvider != null)
                    {
                        title = "Your account Regit was registered with " + info.Login.LoginProvider + ".";
                    }

                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(user.Id, title, type);
                }
            }

            return Ok(isNew);
        }

        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("LoginWithExternalBearer")]
        public async Task<string> LoginWithExternalBearer()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                throw new CustomException(new ErrorViewModel
                {
                    Error = ErrorCode.InternalServerError,
                    Message = "Something went wrong!"
                });
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                var isDisabled = _disableUserService.IsDisabled(user.UserName);

                if (isDisabled)
                {
                    var disabledUser = _disableUserService.GetDisabledUserByEmail(user.UserName);
                    var untilDate = "";
                    if (disabledUser.Until != null)
                    {
                        DateTime dt = Convert.ToDateTime(disabledUser.Until);
                        untilDate = dt.ToString("d");
                    }

                    var message = string.IsNullOrEmpty(disabledUser.Reason)
                        ? Regit.The_User_Was_Disabled + Environment.NewLine + Regit.Until_Title +
                          untilDate + Environment.NewLine + Regit.Please_Contact_Admin
                        : Regit.The_User_Was_Disabled + Environment.NewLine + Regit.Until_Title +
                          untilDate + Environment.NewLine +
                          Regit.Reason_Title + disabledUser.Reason + Environment.NewLine + Regit.Please_Contact_Admin;

                    throw new CustomException(new ErrorViewModel
                    {
                        Error = ErrorCode.BadRequest,
                        Message = message
                    });
                }

                var accountInfo = _accountService.GetByAccountId(user.Id);
                if (!accountInfo.EmailVerified)
                {
                    throw new CustomException(new ErrorViewModel
                    {
                        Error = ErrorCode.BadRequest,
                        Message = user.Email
                    });
                }

                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties =
                    ApplicationOAuthProvider.CreateProperties(user.UserName, accountInfo);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);

                var accessToken = GenerateLocalAccessTokenResponse(oAuthIdentity);
                return accessToken.GetValue("access_token").Value<string>();
            }
            else
            {
                throw new CustomException(new ErrorViewModel
                {
                    Error = ErrorCode.BadRequest,
                    Message = "Invalid request!"
                });
            }
        }

        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet, Route("ExternalLoginInfo")]
        public async Task<ExternalRegistrationInfo> GetExternalLoginInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            if (externalLogin == null)
            {
                throw new CustomException("Invalid request");
            }

            ExternalRegistrationInfo model = new ExternalRegistrationInfo();

            bool skip = false;


            if (!string.IsNullOrEmpty(externalLogin.Email))
            {
                skip = _accountService.CheckDuplicateEmail(externalLogin.Email, null);
                model.Skip = skip;
            }

            model.Email = externalLogin.Email;
            model.FirstName = externalLogin.FirstName;
            model.LastName = externalLogin.LastName;
            model.DisplayName = externalLogin.DisplayName;
            model.Avatar = externalLogin.Avatar;

            return model;
        }

        [AllowAnonymous]
        [HttpPost, Route("ForgotPassword")]
        public async Task<string> ForgotPassword(ForgotPasswordModel model)
        {
//            if (!ModelState.IsValid)
//            {
//                throw new CustomException(ModelState);
//            }

            var user = await UserManager.FindByEmailAsync(model.VerifyInfo.Email);
            var account = _accountService.GetByEmail(model.VerifyInfo.Email);
            if (user == null || account == null)
            {
                throw new CustomException("Email does not match with any account");
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var token = verifyTokenService.Generate(VerifyPurpose.RESET_PASSWORD, account.Profile.PhoneNumber,
                account.Profile.Email);
            _accountService.UpdateResetPasswordToken(account.Id, token.Id);

            if (model.Option == ResetPasswordVerifyOption.EMAIL)
            {
                IMailService _mailService = new MailService();
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_EMAIL");
                var email = template.GetEmailContent(new {Code = token.Token});
                email.SendTo = new string[] {user.Email};
                var sendMailTask = _mailService.SendMailAsync(email);
                sendMailTask.Wait();
                if (!sendMailTask.IsCompleted)
                {
                    throw new CustomException("Cannot send email. Please try again.");
                }

                verifyTokenService.MarkVerificationAsSent(token.Id);
            }
            else if (model.Option == ResetPasswordVerifyOption.SMS)
            {
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_SMS");
                var smsContent = template.GetEmailContent(new {Code = token.Token});
                var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
                {
                    from = "REGIT",
                    to = account.Profile.PhoneNumber,
                    text = smsContent.Body
                }, Global.NexmoCredentials).messages.First();

                if (response.status == "0")
                {
                    verifyTokenService.MarkVerificationAsSent(token.Id);
                }
            }

            return token.RequestId;
        }

        [AllowAnonymous]
        [HttpPost, Route("VerifyPhone/Send")]
        public HttpResponseMessage VerifyPhone(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return Request.CreateApiErrorResponse("Missing phone number", HttpStatusCode.BadRequest);

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var token = verifyTokenService.Generate(VerifyPurpose.VerifyPhone, phoneNumber, null);
            var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
            {
                from = "Regit",
                to = phoneNumber,
                text = $"Regit Verify code: {token.Token}. Valid for 5 minutes"
            }, Global.NexmoCredentials).messages.FirstOrDefault();

            if (response == null || response.status != "0")
            {
                return Request.CreateApiErrorResponse("Error sending SMS", error: "sms.send.error");
            }

            verifyTokenService.MarkVerificationAsSent(token.Id);

            return Request.CreateSuccessResponse(new
            {
                requestId = token.RequestId,
                expires = token.ExpiredTime.ToString("o")
            }, "Verification message sent to phone");
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class VerifyPhoneModel
        {
            public string PhoneNumber { get; set; }
            public string RequestId { get; set; }
            public string Token { get; set; }
        }

        [AllowAnonymous]
        [HttpPost, Route("VerifyPhone/Check")]
        public HttpResponseMessage VerifyPhoneCheckToken(VerifyPhoneModel model)
        {
            if (model == null)
            {
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(model.RequestId))
                return Request.CreateApiErrorResponse("Missing OTP request ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Token))
                return Request.CreateApiErrorResponse("Missing verification request token", HttpStatusCode.BadRequest);

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var verifyToken = verifyTokenService.GetByRequestId(model.RequestId);
            if (verifyToken == null)
            {
                return Request.CreateApiErrorResponse("No request found", error: "request.notFound");
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                model.PhoneNumber = model.PhoneNumber.Trim('+');
                if (verifyToken.PhoneNumber.Trim() != model.PhoneNumber)
                {
                    return Request.CreateApiErrorResponse("Invalid request", error: "request.unauthorized");
                }
            }

            if (verifyToken.Status == TokenStatus.VERIFIED)
                return Request.CreateApiErrorResponse($"Token already verified: {model.RequestId}",
                    error: "request.already.verified");

            var result = verifyTokenService.Check(model.RequestId, model.Token);
            switch (result.Status)
            {
                case TokenStatus.INVALID:
                    return Request.CreateApiErrorResponse($"Token not found or invalid: {model.RequestId}",
                        error: "token.invalid");
                case TokenStatus.EXPIRED:
                    return Request.CreateApiErrorResponse($"Token expired: {model.RequestId}",
                        error: "token.expired");

                case TokenStatus.CREATED:
                    return Request.CreateApiErrorResponse($"Token not sent yet: {model.RequestId}",
                        error: "token.notSent");
                case TokenStatus.CANCELED:
                    return Request.CreateApiErrorResponse($"Token cancelled: {model.RequestId}",
                        error: "token.cancelled");
                case TokenStatus.FAILED:
                    return Request.CreateApiErrorResponse($"Token incorrect: {model.RequestId}",
                        error: "token.incorrect");
            }

            return Request.CreateSuccessResponse(new
            {
                requestId = result.Token.RequestId,
                expires = result.Token.ExpiredTime.ToString("o")
            }, "Token verified successfully");
        }

        [AllowAnonymous]
        [HttpPost, Route("RecoverPassword")]
        public async Task<HttpResponseMessage> RecoverPassword(ForgotPasswordModel model)
        {
//            if (!ModelState.IsValid)
//            {
//                throw new CustomException(ModelState);
//            }

            if (string.IsNullOrEmpty(model.VerifyInfo.Email))
                return Request.CreateApiErrorResponse("Missing parameter: email address", HttpStatusCode.BadRequest);

            var user = await UserManager.FindByEmailAsync(model.VerifyInfo.Email);
            var account = _accountService.GetByEmail(model.VerifyInfo.Email);
            if (user == null || account == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var token = verifyTokenService.Generate(VerifyPurpose.RESET_PASSWORD, account.Profile.PhoneNumber,
                account.Profile.Email);
            _accountService.UpdateResetPasswordToken(account.Id, token.Id);

            var method = model.Option;

            if (method == ResetPasswordVerifyOption.SMS)
            {
                var phoneNumber = account.Profile.PhoneNumber;
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_SMS");
                var smsContent = template.GetEmailContent(new {Code = token.Token});
                var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
                {
                    from = "Regit",
                    to = phoneNumber,
                    text = smsContent.Body
                }, Global.NexmoCredentials).messages.FirstOrDefault();

                if (response == null || response.status != "0")
                {
                    return Request.CreateApiErrorResponse("Error sending SMS");
                }

                verifyTokenService.MarkVerificationAsSent(token.Id);
                return Request.CreateSuccessResponse(new
                {
                    requestId = token.RequestId,
                    expires = token.ExpiredTime.ToString("o"),
                    phoneLast4 = phoneNumber.Substring(Math.Max(0, phoneNumber.Length - 4))
                }, "Verification message sent to phone");
            }
            else
            {
                IMailService _mailService = new MailService();
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_EMAIL");
                var email = template.GetEmailContent(new {Code = token.Token});
                email.SendTo = new string[] {user.Email};
                var sendMailTask = _mailService.SendMailAsync(email);
                sendMailTask.Wait();
                if (!sendMailTask.IsCompleted)
                {
                    return Request.CreateApiErrorResponse("Error sending email");
                }

                verifyTokenService.MarkVerificationAsSent(token.Id);
                return Request.CreateSuccessResponse(new
                {
                    requestId = token.RequestId,
                    expires = token.ExpiredTime.ToString("o")
                }, "Verification message sent to email");
            }
        }

        [AllowAnonymous]
        [HttpPost, Route("RecoverPasswordCheck")]
        public async Task<HttpResponseMessage> RecoverPasswordCheckToken(VerifyResetPasswordTokenModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(model.Email))
                return Request.CreateApiErrorResponse("Missing parameter: email address", HttpStatusCode.BadRequest);


            var user = await UserManager.FindByEmailAsync(model.Email);
            var account = _accountService.GetByEmail(model.Email);
            if (user == null || account == null)
            {
                return Request.CreateApiErrorResponse("Account not found", HttpStatusCode.NotFound);
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var verifyToken = verifyTokenService.GetByRequestId(model.RequestId);
            if (verifyToken == null)
            {
                return Request.CreateApiErrorResponse("No request found", HttpStatusCode.OK);
            }

            if (verifyToken.Id != account.ResetPasswordToken)
            {
                return Request.CreateApiErrorResponse("Invalid request", HttpStatusCode.OK);
            }

            if (verifyToken.Status == TokenStatus.VERIFIED)
                return Request.CreateApiErrorResponse($"Token already verified: {model.RequestId}",
                    HttpStatusCode.OK);

            var result = verifyTokenService.Check(model.RequestId, model.Token);
            switch (result.Status)
            {
                case TokenStatus.INVALID:
                    return Request.CreateApiErrorResponse($"Token not found or invalid: {model.RequestId}",
                        HttpStatusCode.OK);
                    break;
                case TokenStatus.EXPIRED:
                    return Request.CreateApiErrorResponse($"Token expired: {model.RequestId}",
                        HttpStatusCode.OK);
                    break;

                case TokenStatus.CREATED:
                    return Request.CreateApiErrorResponse($"Token not sent yet: {model.RequestId}",
                        HttpStatusCode.OK);
                    break;
                case TokenStatus.CANCELED:
                    return Request.CreateApiErrorResponse($"Token cancelled: {model.RequestId}",
                        HttpStatusCode.OK);
                    break;
                case TokenStatus.FAILED:
                    return Request.CreateApiErrorResponse($"Token incorrect: {model.RequestId}",
                        HttpStatusCode.OK);
                    break;
            }

            return Request.CreateSuccessResponse(new
            {
                requestId = result.Token.RequestId,
                expires = result.Token.ExpiredTime.ToString("o")
            }, "Token verified successfully");
        }


        [AllowAnonymous]
        [HttpPost, Route("ResetPassword")]
        public async Task<HttpResponseMessage> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            var account = _accountService.GetByEmail(model.Email);
            if (user == null || account == null)
            {
                return Request.CreateApiErrorResponse("Account not found", HttpStatusCode.NotFound);
            }


            if (model.RequestId != "Question")
            {
                IVerifyTokenService verifyTokenService = new VerifyTokenService();
                var verifyToken = verifyTokenService.GetByRequestId(model.RequestId);

                if (verifyToken == null)
                {
                    return Request.CreateApiErrorResponse("Invalid request", HttpStatusCode.BadRequest);
                }

                if (verifyToken.Status != TokenStatus.VERIFIED)
                {
                    return Request.CreateApiErrorResponse("Unverified request", HttpStatusCode.BadRequest);
                }

                if (verifyToken.Token != model.Token)
                {
                    return Request.CreateApiErrorResponse("Incorrect token", HttpStatusCode.BadRequest);
                }

                if (verifyToken.Id != account.ResetPasswordToken)
                {
                    return Request.CreateApiErrorResponse("Request replaced by new one", HttpStatusCode.BadRequest);
                }
            }


            var resetToken = UserManager.GeneratePasswordResetToken(user.Id);
            var result = await UserManager.ResetPasswordAsync(user.Id, resetToken, model.Password);

            if (!result.Succeeded)
            {
                return Request.CreateApiErrorResponse("Error changing password");
            }

            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "You have reset your password.";
                string type = "user";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(user.Id, title, type);
            }

            _accountService.UpdateResetPasswordToken(account.Id, null);
            return Request.CreateSuccessResponse(new
            {
                accountId = account.AccountId,
            }, "Password changed successfully");
        }

        [AllowAnonymous]
        [HttpPost, Route("ResendResetPasswordToken")]
        public async Task<string> ResendResetPasswordToken(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var user = await UserManager.FindByEmailAsync(model.VerifyInfo.Email);
            var account = _accountService.GetByEmail(model.VerifyInfo.Email);
            if (user == null || account == null)
            {
                throw new CustomException("Email does not match with any account");
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();
            VerifyToken token = null;
            if (account.ResetPasswordToken.HasValue)
            {
                var oldToken = verifyTokenService.GetById(account.ResetPasswordToken.Value);
                if (verifyTokenService.IsPending(oldToken))
                {
                    token = verifyTokenService.RefreshExpiredTime(oldToken.Id);
                }
                else if (oldToken.Status == TokenStatus.EXPIRED || oldToken.Status == TokenStatus.CREATED ||
                         oldToken.Status == TokenStatus.SENT)
                {
                    token = verifyTokenService.Generate(VerifyPurpose.RESET_PASSWORD, account.Profile.PhoneNumber,
                        account.Profile.Email);
                    _accountService.UpdateResetPasswordToken(account.Id, token.Id);
                }
            }
            else
            {
                token = verifyTokenService.Generate(VerifyPurpose.RESET_PASSWORD, account.Profile.PhoneNumber,
                    account.Profile.Email);
                _accountService.UpdateResetPasswordToken(account.Id, token.Id);
            }

            if (model.Option == ResetPasswordVerifyOption.EMAIL)
            {
                IMailService _mailService = new MailService();
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_EMAIL");
                var email = template.GetEmailContent(new {Code = token.Token});
                email.SendTo = new string[] {user.Email};
                var sendMailTask = _mailService.SendMailAsync(email);
                sendMailTask.Wait();
                if (!sendMailTask.IsCompleted)
                {
                    throw new CustomException("Cannot send email. Please try again.");
                }

                verifyTokenService.MarkVerificationAsSent(token.Id);
            }
            else if (model.Option == ResetPasswordVerifyOption.SMS)
            {
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("RESET_PASSWORD_SMS");
                var smsContent = template.GetEmailContent(new {Code = token.Token});
                var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
                {
                    from = "REGIT",
                    to = account.Profile.PhoneNumber,
                    text = smsContent.Body
                }, Global.NexmoCredentials).messages.First();

                if (response.status == "0")
                {
                    verifyTokenService.MarkVerificationAsSent(token.Id);
                }
            }

            return token.RequestId;
        }

        [AllowAnonymous]
        [HttpPost, Route("VerifyResetPasswordToken")]
        public async Task VerifyResetPassworkToken(VerifyResetPasswordTokenModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            var account = _accountService.GetByEmail(model.Email);
            if (user == null || account == null)
            {
                throw new CustomException("Email does not match with any account");
            }

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var verifyToken = verifyTokenService.GetByRequestId(model.RequestId);
            if (verifyToken == null)
            {
                throw new CustomException("Your verification request does not exist");
            }

            if (verifyToken.Id != account.ResetPasswordToken)
            {
                throw new CustomException("Your verification request is not valid");
            }

            verifyTokenService.Verify(model.RequestId, model.Token);
        }


        [HttpPost, Route("Verify/PIN")]
        public async Task<string> VerifyPIN(VerifyPINModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserId);
            var searchResponse =
                Nexmo.Api.NumberVerify.Search(new Nexmo.Api.NumberVerify.SearchRequest {request_id = model.RequestId},
                    Global.NexmoCredentials);
            if (!string.IsNullOrEmpty(searchResponse.error_text))
                throw new CustomException(searchResponse.error_text);

            if (searchResponse.number != account.Profile.PhoneNumber.Trim('+'))
            {
                throw new CustomException("Invalid request");
            }

            var checkResponse =
                Nexmo.Api.NumberVerify.Check(
                    new Nexmo.Api.NumberVerify.CheckRequest {request_id = model.RequestId, code = model.PIN},
                    Global.NexmoCredentials);

            if (!string.IsNullOrEmpty(checkResponse.error_text))
            {
                throw new CustomException(checkResponse.error_text);
            }

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
            }

            return _accountService.GeneratePhoneVerifiedToken(account.AccountId);
        }

        [AllowAnonymous, HttpPost, Route("Verify/PhoneNumber")]
        public async Task<string> VerifyPhoneNumber(VerifyPhoneNumberModel model)
        {
            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                return "123456";
            /* Disable verify Phone */
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var currentUserId = HttpContext.Current.User.Identity.GetUserId();
                    var account = _accountService.GetByAccountId(currentUserId);

                    model = new VerifyPhoneNumberModel();
                    model.PhoneNumber = account.Profile.PhoneNumber;
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            //  Disable Nexmo

            model.PhoneNumber = PhoneNumberHelper.GetFormatedPhoneNumber(model.PhoneNumber);

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var token = verifyTokenService.Generate(VerifyPurpose.AUTHENTICATION, model.PhoneNumber,
                "");
            //_accountService.UpdateResetPasswordToken(account.Id, token.Id);

//            var res = Nexmo.Api.NumberVerify.Verify(new Nexmo.Api.NumberVerify.VerifyRequest
//            {
//                number = model.PhoneNumber,
//                brand = "Regit Verify",
//            }, Global.NexmoCredentials);
//
//            if (!string.IsNullOrEmpty(res.error_text))
//            {
//                return "";
//                //throw new CustomException(res.error_text);
//            }


            var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
            {
                from = "Regit",
                to = model.PhoneNumber,
                text = $"Regit Verify code: {token.Token}. Valid for 5 minutes"
            }, Global.NexmoCredentials).messages.FirstOrDefault();

            if (response == null || response.status != "0")
            {
                return "";
            }

            verifyTokenService.MarkVerificationAsSent(token.Id);

            //return res.request_id;

            return token.RequestId;

            /* Disable verify Phone */
            // return "123456";
        }

        public class NexmoControlResponse
        {
            public string status { get; set; }
            public string command { get; set; }
        }

        [AllowAnonymous, HttpPost, Route("Otp/VerifyPhoneNumber")]
        public async Task<HttpResponseMessage> OtpVerifyPhoneNumber(string phoneNumber = null,
            [FromUri] string cancelRequest = null)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return Request.CreateApiErrorResponse("Missing phone number", HttpStatusCode.OK,
                    "otp_missing_number");


            object rcResponse = null;

            Credentials creds = Global.NexmoCredentials;

            if (!string.IsNullOrEmpty(cancelRequest))
            {
/*                using (var client = new HttpClient())
                {
                    var values = new List<KeyValuePair<string, string>>();
                    values.Add(new KeyValuePair<string, string>("api_key", "8610189e"));
                    values.Add(new KeyValuePair<string, string>("api_secret", "048bce8a9587add6"));
                    values.Add(new KeyValuePair<string, string>("request_id", cancelRequest));
                    values.Add(new KeyValuePair<string, string>("cmd", "cancel"));

                    var content = new FormUrlEncodedContent(values);

                    HttpResponseMessage response = await client.GetAsync($"https://api.nexmo.com/verify/control/json" +
                                                                         $"?api_key=8610189e&api_secret=048bce8a9587add6" +
                                                                         $"&cmd=cancel&request_id={cancelRequest}");
                    var ctrl = await response.Content.ReadAsStreamAsync();
                    string json;
                    using (var sr = new StreamReader(ctrl))
                    {
                        json = sr.ReadToEnd();
                    }

                }*/

                var ctrl = Nexmo.Api.NumberVerify.Control(new Nexmo.Api.NumberVerify.ControlRequest
                {
                    request_id = cancelRequest,
                    cmd = "cancel"
                }, creds);

                if (ctrl.status != "0")
                {
                    switch (ctrl.status)
                    {
                        case "3":
                            rcResponse = new {success = false, message = "Invalid request"};
                            break;
                        case "6":
                            rcResponse = new {success = false, message = $"Request not active: {cancelRequest}"};
                            break;
                        case "19":
                            return Request.CreateApiErrorResponse("Request cannot be cancelled",
                                HttpStatusCode.OK, "otp_uncancellable_request");
                        default:
                            rcResponse = new {success = false, message = "Error cancelling request"};
                            break;
                    }
                }
                else
                {
                    rcResponse = new {success = true, message = $"Request cancelled successfully: {cancelRequest}"};
                }
            }


            phoneNumber = PhoneNumberHelper.GetFormatedPhoneNumber(phoneNumber);
            if (string.IsNullOrEmpty(phoneNumber))
                return Request.CreateApiErrorResponse("Invalid phone number", HttpStatusCode.OK,
                    "otp_invalid_number");

            var res = Nexmo.Api.NumberVerify.Verify(new Nexmo.Api.NumberVerify.VerifyRequest
            {
                number = phoneNumber,
                brand = "Regit Verify",
                sender_id = "Regit",
                //pin_expiry = "60",
//                next_event_wait = "120"
            }, creds);
            if (res.status != "0")
            {
                switch (res.status)
                {
                    case "3":
                        return Request.CreateApiErrorResponse("Invalid phone number", HttpStatusCode.OK,
                            "otp_invalid_number");
                    case "10":
                        return Request.CreateApiErrorResponse($"Pending request for phone number {phoneNumber}",
                            HttpStatusCode.OK, "otp_pending_request");
                }

                return Request.CreateApiErrorResponse("OTP error", HttpStatusCode.OK, "otp_unknown_error");
            }

            var result = rcResponse == null
                ? (object) new {phoneNumber = phoneNumber, requestId = res.request_id}
                : new {phoneNumber = phoneNumber, requestId = res.request_id, requestCancellation = rcResponse};

            return Request.CreateSuccessResponse(result,
                "PIN sent to phone number");
        }


        [HttpPost, Route("SendSMS")]
        public string SendSMS()
        {
            string rs = "";
            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                return rs;
            var PhoneNumber = "";

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var currentUserId = HttpContext.Current.User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(currentUserId);
                PhoneNumber = account.Profile.PhoneNumber;
                PhoneNumber = PhoneNumberHelper.GetFormatedPhoneNumber(PhoneNumber);
                var res = Nexmo.Api.NumberVerify.Verify(new Nexmo.Api.NumberVerify.VerifyRequest
                {
                    number = PhoneNumber,
                    brand = "Regit Business OTP",
                }, Global.NexmoCredentials);
                rs = res.request_id;
                return rs;
            }
            else
            {
                throw new CustomException("Invalid request");
            }
        }

        [AllowAnonymous, HttpPost, Route("Verify/CheckPIN")]
        public async Task CheckPIN(VerifyPINModel model)
        {
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(currentUserAccountId);

                var pinCode = CommonFunctions.CreateSHA512Hash(model.PIN);
                if (account.Profile.PinCode == model.PIN || account.Profile.PinCode == pinCode)
                {
                    _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
                }
                else
                    throw new CustomException(GH.Lang.Regit.PIN_Error_Incorrect);
            }
            else if (User.Identity != null && !string.IsNullOrEmpty(model.StaticPIN))
            {
                HttpContext.Current.Session["PinCode"] = model.StaticPIN.ToString();
                _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
            }
            else
                throw new CustomException(GH.Lang.Regit.PIN_Error_Incorrect);
        }

        [AllowAnonymous, HttpPost, Route("Verify/CheckSetPIN")]
        public bool CheckSetPIN(VerifyPINModel model)
        {
            //Vu Test
            //_accountService.SetSMSAuthenticated(User.Identity.GetUserId());
            //return;
            // end Vu Test
            HttpContext.Current.Session["PinCode"] = "";

            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
            {
                if (!string.IsNullOrEmpty(model.StaticPIN) && !User.Identity.IsAuthenticated)
                {
                    if (model.StaticPIN.Length > 3 && model.StaticPIN.Length < 7)
                    {
                        var pinCode = CommonFunctions.CreateSHA512Hash(model.StaticPIN);
                        HttpContext.Current.Session["PinCode"] = pinCode;
                    }

                    else
                        throw new CustomException(GH.Lang.Regit.PIN_Error_Format_Incorrect);
                }

                return true;
            }

            /* disable verify pin */
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

//            var checkResponse =
//                Nexmo.Api.NumberVerify.Check(
//                    new Nexmo.Api.NumberVerify.CheckRequest {request_id = model.RequestId, code = model.PIN},
//                    Global.NexmoCredentials);
//            try
//            {
//                var dt = DateTime.UtcNow.ToString();
//                log.Debug("Nexmo number phone: " + model.PhoneNumber + " ResquestId: "
//                          + model.RequestId + " Pin:" + model.PIN + " status: " + checkResponse.status + " UTC time: " +
//                          dt);
//            }
//            catch
//            {
//            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var verifyToken = verifyTokenService.GetByRequestId(model.RequestId);
            if (verifyToken == null)
            {
                return false;
            }


            var result = verifyTokenService.Check(model.RequestId, model.PIN);
            switch (result.Status)
            {
                case TokenStatus.INVALID:

                case TokenStatus.EXPIRED:

                case TokenStatus.CREATED:
                case TokenStatus.CANCELED:
                case TokenStatus.FAILED:
                case TokenStatus.FINISHED:
                    return false;
            }

            if (!string.IsNullOrEmpty(model.StaticPIN) && !User.Identity.IsAuthenticated)
            {
                if (model.StaticPIN.Length > 3 && model.StaticPIN.Length < 7)
                {
                    var pinCode = CommonFunctions.CreateSHA512Hash(model.StaticPIN);
                    HttpContext.Current.Session["PinCode"] = pinCode;
                }

                else
                    throw new CustomException(GH.Lang.Regit.PIN_Error_Format_Incorrect);
            }

//            if (!string.IsNullOrEmpty(checkResponse.error_text))
//            {
//                switch (checkResponse.status)
//                {
//                    case "16":
//                        throw new CustomException(GH.Lang.Regit.PIN_Error_Incorrect);
//                    case "17":
//                        throw new CustomException(GH.Lang.Regit.PIN_Error_Wrong_Many_Times);
//                    case "6":
//                        throw new CustomException(GH.Lang.Regit.PIN_Error_Invalid_Request);
//                    default:
//                        throw new CustomException(new ErrorViewModel
//                        {
//                            Error = ErrorCode.InternalServerError,
//                            Message = "Something went wrong!"
//                        });
//                }
//            }
            if (User.Identity.IsAuthenticated)
                _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
            return true;

            /* disable verify pin */
        }

        [AllowAnonymous, HttpPost, Route("Verify/SetStaticPIN")]
        public async Task SetStaticPIN(VerifyPINModel model)
        {
            HttpContext.Current.Session["PinCode"] = "";

            if (!string.IsNullOrEmpty(model.StaticPIN) && !User.Identity.IsAuthenticated)
            {
                if (model.StaticPIN.Length > 3 && model.StaticPIN.Length < 7)
                {
                    var pinCode = CommonFunctions.CreateSHA512Hash(model.StaticPIN);
                    HttpContext.Current.Session["PinCode"] = pinCode;
                }

                else
                    throw new CustomException(GH.Lang.Regit.PIN_Error_Format_Incorrect);
            }
            else

                throw new CustomException("Error can not write static PIN");
        }

        //Verify/IsSMSAuthenticated
        [HttpGet, HttpPost, Route("Verify/IsSMSAuthenticated")]
        public async Task<bool> IsSMSAuthenticated()
        {
            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                return true;


            if (User == null || User.Identity == null) return false;
            return _accountService.IsSMSAuthenticated(User.Identity.GetUserId());
        }

        [HttpGet, HttpPost, Route("IsConfirmSMS")]
        public bool IsConfirmSMS()
        {
            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                return true;
            if (User == null || User.Identity == null) return false;
            return _accountService.IsSMSAuthenticated(User.Identity.GetUserId());
        }

        [HttpGet, HttpPost, Route("IsCheckPinVault")]
        public bool IsCheckPinVault()
        {
            if (!ConfigHelp.GetBoolValue("IsCheckPinVault"))
                return true;

            if (User == null || User.Identity == null) return false;
            if (IsSetPIN())
                return _accountService.IsSMSAuthenticated(User.Identity.GetUserId());
            else
                return false;
        }

        [HttpGet, HttpPost, Route("EnableSMSAuthencation")]
        public bool EnableSMSAuthencation()
        {
            bool rs = true;
            try
            {
                rs = ConfigHelp.GetBoolValue("IsEnableSMSAuthencation");
            }
            catch
            {
            }

            return rs;
        }

        [HttpGet, HttpPost, Route("Verify/IsConfirmSMSBusiness")]
        public async Task<bool> IsConfirmSMSBusiness()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                    return true;
                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(currentUserAccountId);
                var rs = _accountService.IsSMSAuthenticated(User.Identity.GetUserId());
                if (account.AccountType == AccountType.Business)
                    return rs;
                else
                    return true;
            }
            else
                return false;
        }

        [HttpGet, HttpPost, Route("IsBusinessAccount")]
        public async Task<bool> IsBusinessAccount()
        {
            var rs = false;
            if (User.Identity.IsAuthenticated)
            {
                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(currentUserAccountId);

                if (account.AccountType == AccountType.Business)
                    rs = true;
            }

            return rs;
        }

        [HttpGet, HttpPost, Route("IsSetPIN")]
        public bool IsSetPIN()
        {
            var rs = true;
            if (!ConfigHelp.GetBoolValue("IsCheckPinVault"))
                return true;
            try
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (string.IsNullOrEmpty(currentUser.Profile.PinCode))
                {
                    rs = false;
                }
            }
            catch
            {
            }

            return rs;
        }

        //UpdatePIN PinCode
        [HttpPost, Route("SetPinCode")]
        public bool SetPinCode(PinCodeViewModel pinCode)
        {
            var rs = false;

            if (!string.IsNullOrEmpty(pinCode.Pin))
            {
                if (pinCode.Pin.Length > 3 && pinCode.Pin.Length < 7)
                {
                    var pin = CommonFunctions.CreateSHA512Hash(pinCode.Pin);

                    var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                    var updatePin = _accountService.UpdatePIN(pin, account);
                    if (updatePin)
                    {
                        _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
                        rs = true;
                    }
                }

                else
                    throw new CustomException(GH.Lang.Regit.PIN_Error_Format_Incorrect);
            }


            else

                throw new CustomException("Error can not write static PIN");

            return rs;
        }

        [HttpGet, Route("IsLinkedWithBusinessAccount")]
        public bool IsAccountLinkedWithBusinessAccount()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (currentUser.AccountType != AccountType.Business)
            {
                var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id);
                return linkedAccounts.Any(a => a.EmailVerified);
            }

            else
            {
                return true;
            }
        }

        [AllowAnonymous]
        [HttpPost, Route("CheckDuplicatedEmail")]
        public bool CheckDuplicatedEmail(CheckDuplicatedEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            if (User.Identity.IsAuthenticated && (model.IgnoreCurrent || model.IgnoreCurrentBusiness))
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (model.IgnoreCurrentBusiness)
                {
                    if (currentUser.AccountType == AccountType.Business)
                    {
                        return _accountService.CheckDuplicateEmail(model.Email, currentUser.AccountId);
                    }
                    else if (currentUser.BusinessAccountRoles.Any())
                    {
                        return _accountService.CheckDuplicateEmail(model.Email,
                            currentUser.BusinessAccountRoles.First().AccountId);
                    }
                    else
                    {
                        throw new CustomException("Invalid request");
                    }
                }
                else
                {
                    return _accountService.CheckDuplicateEmail(model.Email, currentUser.AccountId);
                }
            }
            else
            {
                return _accountService.CheckDuplicateEmail(model.Email, null);
            }
        }

        [AllowAnonymous]
        [HttpPost, Route("VerifySecurityQuestions")]
        public ResetPasswordAccountInfo VerifySecurityQuestions(VerifySecurityQuestionsModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByEmail(model.Email);
            if (account == null)
            {
                throw new CustomException("Email does not match with any account");
            }

            return new ResetPasswordAccountInfo
            {
                Id = account.Id.ToString(),
                DisplayName = account.Profile.DisplayName,
                Avatar = account.Profile.PhotoUrl,
                Address = (string.IsNullOrEmpty(account.Profile.City) ? "" : account.Profile.City)
                          + ((!string.IsNullOrEmpty(account.Profile.City) &&
                              !string.IsNullOrEmpty(account.Profile.Country))
                              ? ", "
                              : "")
                          + (string.IsNullOrEmpty(account.Profile.Country) ? "" : account.Profile.Country),
                EncodedPhoneNumber = PhoneNumberHelper.EncodePhoneNumber(account.Profile.PhoneNumber)
            };
        }

        [AllowAnonymous]
        [HttpPost, Route("GetQuestionsByEmail")]
        public QuestionAnswerViewModel GetQuestionsByEmail(SendVerifyEmailModel email)
        {
            var rs = new QuestionAnswerViewModel();
            ISecurityQuestionService _questionService = new SecurityQuestionService();
            IList<SecurityQuestion> lstQ = _questionService.GetAll();
            try
            {
                var account = _accountService.GetByEmail(email.Email);
                if (account.AccountType == AccountType.Business)
                    rs.AccountType = "Business";
                if (!string.IsNullOrEmpty(account.AccountId))
                {
                    rs.AccountStatus = "";
                    try
                    {
                        if (account.EmailVerifyTokenDate != null && account.Status == EnumAccount.LockResetPassword)
                        {
                            var dtLock = Convert.ToDateTime(account.EmailVerifyTokenDate);

                            if (dtLock.AddMinutes(30) < DateTime.Now)
                                _accountService.UpdateStatus(account, "");
                            else
                                rs.AccountStatus = account.Status;
                        }
                    }
                    catch
                    {
                    }

                    if (account.SecurityQuesion1 != null)
                        for (int i = 0; i < lstQ.Count; i++)
                        {
                            if (account.SecurityQuesion1.QuestionId.Equals(lstQ[i].Id))
                            {
                                rs.QuestionId1 = account.SecurityQuesion1.QuestionId.ToString();
                                rs.Question1 = lstQ[i].Question;
                                rs.Answer1 = account.SecurityQuesion1.Answer;
                            }

                            if (account.SecurityQuesion2.QuestionId.Equals(lstQ[i].Id))
                            {
                                rs.QuestionId2 = account.SecurityQuesion1.QuestionId.ToString();
                                rs.Question2 = lstQ[i].Question;
                                rs.Answer2 = account.SecurityQuesion2.Answer;
                            }

                            if (account.SecurityQuesion3.QuestionId.Equals(lstQ[i].Id))
                            {
                                rs.QuestionId3 = account.SecurityQuesion1.QuestionId.ToString();
                                rs.Question3 = lstQ[i].Question;
                                rs.Answer3 = account.SecurityQuesion1.Answer;
                            }
                        }

                    rs.email = email.Email;
                }
            }
            catch
            {
            }

            return rs;
        }

        [AllowAnonymous]
        [HttpPost, Route("LockResetPassword")]
        public IHttpActionResult LockResetPassword(SendVerifyEmailModel email)
        {
            try
            {
                var account = _accountService.GetByEmail(email.Email);

                _accountService.UpdateStatus(account, EnumAccount.LockResetPassword);
                SendEmailForLockResetPassword(account);
            }
            catch
            {
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost, Route("InsertTokenDevice")]
        public string InsertTokenDevice(TokenDeviceViewModel tokenDevice)
        {
            var rs = "";
            var token = new ManageTokenDevice();
            try
            {
                var acc = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (!string.IsNullOrEmpty(tokenDevice.AccountId))
                    token.AccountId = tokenDevice.AccountId;
                else
                    token.AccountId = acc.AccountId;
                if (tokenDevice.CreatedDate.HasValue)
                    token.CreatedDate = tokenDevice.CreatedDate.Value;
                else
                    token.CreatedDate = DateTime.UtcNow;

                token.TokenDevice = tokenDevice.TokenDevice;
                token.Status = EnumStatusTokenDevice.Online;
                var _manageTokenDeviceBusinessLogic = new ManageTokenDeviceBusinessLogic();
                rs = _manageTokenDeviceBusinessLogic.Insert(token);
            }
            catch
            {
            }


            return rs;
        }


        #region Outsite

        // 
        [AllowAnonymous]
        [HttpPost, Route("GetOutsiteById")]
        public Outsite GetOutsiteById(OutsiteIdViewModel outsiteId)
        {
            var rs = new Outsite();
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();

            try
            {
                rs = _outsiteBusinessLogic.GetOutsiteById(outsiteId.OutsiteId);
            }
            catch
            {
            }

            return rs;
        }

        [HttpPost, Route("GetOutsiteSyncByCampaignId")]
        public Outsite GetOutsiteSyncByCampaignId(OutsiteIdViewModel outsite)
        {
            var rs = new Outsite();
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();
            if (string.IsNullOrEmpty(outsite.Type))
                outsite.Type = EnumOutSiteType.SyncEmailNoti;
            try
            {
                rs = _outsiteBusinessLogic.GetOutsiteByCompnentId(outsite.CompnentId, outsite.Type);
            }
            catch
            {
            }

            return rs;
        }

        [AllowAnonymous]
        [HttpGet, Route("GetOutsiteByUserId")]
        public OutsiteViewModel GetOutsiteSyncByUserId()
        {
            var id = "";
            var rs = new OutsiteViewModel();

            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();
            try
            {
                id = User.Identity.GetUserId();
                var type = EnumNotificationType.NotifySyncHandshakeToMailOutsite;
                var outsite = new Outsite();
                outsite = _outsiteBusinessLogic.GetOutsiteByUserId(id, type);
                rs.Id = outsite.Id.ToString();
                rs.Email = outsite.Email;
                rs.Option = outsite.Option;

                rs.SendMe = outsite.SendMe;
                rs.FromUserId = outsite.FromUserId;
                rs.ListEmail = outsite.ListEmail;
                rs.FromDisplayName = outsite.FromDisplayName;
                rs.CompnentId = outsite.CompnentId;
                rs.Type = outsite.Type;
                rs.Description = outsite.Description;
                rs.DateCreate = outsite.DateCreate;
                rs.Url = outsite.Url;
                rs.Status = outsite.Status;
            }
            catch
            {
            }

            return rs;
        }

        [AllowAnonymous]
        [HttpPost, Route("GetListHandShakeOutsite")]
        public List<OutsiteViewModel> GetListHandShakeOutsite(OutsiteViewModel osview)
        {
            var id = "";
            var result = new List<OutsiteViewModel>();

            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();
            try
            {
                id = User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(id);
                if (account.AccountType != AccountType.Business)
                {
                    var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

                    var linkedAccountBusiness = _accountService
                        .GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id).FirstOrDefault();
                    if (linkedAccountBusiness != null)
                        id = linkedAccountBusiness.AccountId;
                }

                var type = EnumNotificationType.NotifyInvitedHandshakeOutsite;
                var outsites = new List<Outsite>();
                outsites = _outsiteBusinessLogic.GetListOutsiteByUserId(id, type, osview.CompnentId);
                foreach (var outsite in outsites)
                {
                    var rs = new OutsiteViewModel();
                    try
                    {
                        rs.Id = outsite.Id.ToString();
                        rs.Email = outsite.Email;
                        rs.Option = outsite.Option;
                        rs.SendMe = outsite.SendMe;
                        rs.FromUserId = outsite.FromUserId;
                        rs.ListEmail = outsite.ListEmail;
                        rs.FromDisplayName = outsite.FromDisplayName;
                        rs.CompnentId = outsite.CompnentId;
                        rs.Type = outsite.Type;
                        rs.Description = outsite.Description;
                        rs.DateCreate = outsite.DateCreate;
                        rs.Url = outsite.Url;
                        rs.Status = outsite.Status;
                        if (rs != null && !result.Exists(value => value.Email == rs.Email))
                            result.Add(rs);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        [AllowAnonymous]
        [HttpPost, Route("DeleteOutsiteById")]
        public IHttpActionResult DeleteOutsiteById(OutsiteViewModel osview)
        {
            var rs = false;
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();
            if (!string.IsNullOrEmpty(osview.Id))
            {
                try
                {
                    _outsiteBusinessLogic.DeleteOutsiteById(osview.Id);
                    rs = true;
                }
                catch
                {
                    rs = false;
                }
            }

            return Ok(rs);
        }

        [HttpPost, Route("InsertOutsite")]
        public IHttpActionResult InsertOutsite(Outsite outsite)
        {
            if (outsite == null) return BadRequest();
            var acc = _accountService.GetByAccountId(User.Identity.GetUserId());

            outsite.FromUserId = outsite.FromUserId ?? acc.AccountId;
            outsite.FromDisplayName = outsite.FromDisplayName ?? acc.Profile.DisplayName;
            outsite.Email = outsite.Email ?? acc.Profile.Email;
            outsite.DateCreate = DateTime.Now;

            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();

            var rs = _outsiteBusinessLogic.InsertOutsite(outsite);

            return Ok(rs);
        }


        [HttpPost, Route("UpdateOutsiteEmailSync")]
        public string UpdateOutsiteEmailSync(OutsiteViewModel vm)
        {
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();

            if (string.IsNullOrEmpty(vm.DateCreate.ToString()))
                vm.DateCreate = DateTime.Now;

            var outsite = OutsiteAdapter.OutsiteViewModelToOutsite(vm);

            var rs = _outsiteBusinessLogic.UpdateOutsite(outsite);

            return rs;
        }

        public void InsertInfoFromOutsite(string outsiteId, Account account)
        {
            var rs = new Outsite();
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();

            try
            {
                rs = _outsiteBusinessLogic.GetOutsiteById(outsiteId);
            }
            catch
            {
            }

            if (rs != null)
            {
                try
                {
                    if (rs.Type == EnumNotificationType.InviteEmergency)
                    {
                        var sender = _accountService.GetByAccountId(rs.FromUserId);
                        //   _networkService.InviteTrustEmergency(sender.Id, receiver.Id, model.Relationship, model.IsEmergency, model.Rate);
                        _networkService.InviteTrustEmergency(sender.Id, account.Id, "", true, 1);
                    }

                    if (rs.Type == EnumNotificationType.NotifyInvitedHandshakeOutsite)
                    {
                        new PostHandShakeBusinessLogic().InsertPostHandshake(rs.CompnentId, rs.Description,
                            rs.FromUserId, account.AccountId, new List<FieldinformationVault>());
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
                        notificationMessage.FromAccountId = rs.FromUserId;
                        notificationMessage.FromUserDisplayName = rs.FromDisplayName;
                        notificationMessage.ToAccountId = account.AccountId;
                        notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                        notificationMessage.Content = rs.Description;

                        // notificationMessage.Content = delegationMessage.Message;
                        notificationMessage.PreserveBag = rs.CompnentId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                    }

                    _outsiteBusinessLogic.DeleteOutsiteById(outsiteId);
                }
                catch
                {
                }
            }
        }


        //GetOutsiteByCompnentId

        #endregion Outsite

        #region ProfilePrivacy

        [HttpGet, Route("GetProfilePrivacyByAccountId")]
        public ProfilePrivacy GetProfilePrivacyByAccountId(string accountId)
        {
            string userid = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(accountId))
                userid = accountId;
            var rs = new ProfilePrivacy();
            ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
            try
            {
                rs = _profiePrivacy.GetProfilePrivacyByAccountId(userid);
            }
            catch
            {
            }

            return rs;
        }

        [HttpPost, Route("UpdateProfilePrivacyByAccountId")]
        public string UpdateProfilePrivacyByAccountId(ProfilePrivacy profilePrivacy)
        {
            string userid = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(profilePrivacy.AccountId))
                profilePrivacy.AccountId = userid;
            var proPri = new ProfilePrivacy();
            ProfilePrivacyBusinessLogic _profilePrivacy = new ProfilePrivacyBusinessLogic();

            string rs = "";
            //ProfilePrivacy
            try
            {
                proPri = _profilePrivacy.GetProfilePrivacyByAccountId(profilePrivacy.AccountId);
                if (proPri != null)
                    _profilePrivacy.UpdateProfilePrivacy(profilePrivacy);
                else
                    rs = _profilePrivacy.InsertProfilePrivacy(profilePrivacy);
            }
            catch
            {
            }

            return rs;
        }

        #endregion ProfilePrivacy


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet, Route("GetProfileByAccountId")]
        public HttpResponseMessage GetProfileByAccountId(HttpRequestMessage request)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            string jsonCompanyDetails = "";

            try
            {
                jsonCompanyDetails = _accountService.GetProfileByAccountId(currentUserAccountId);
            }
            catch
            {
            }

            var response = Request.CreateResponse<object>(System.Net.HttpStatusCode.OK,
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsonCompanyDetails));
            return response;
        }

        [HttpPost, Route("UpdateProfileByAccountId")]
        public HttpResponseMessage UpdateProfileByAccountId(HttpRequestMessage request, ProfileViewModel profile)
        {
            try
            {
                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
                _accountService.UpdateProfileByAccountId(currentUserAccountId, profile.BsonString);
            }
            catch
            {
            }

            var response = Request.CreateResponse<object>(System.Net.HttpStatusCode.OK);
            return response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private JObject GenerateLocalAccessTokenResponse(ClaimsIdentity identity)
        {
            var tokenExpiration = TimeSpan.FromDays(1);

            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            var accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);

            JObject tokenResponse = new JObject(
                new JProperty("access_token", accessToken)
            );

            return tokenResponse;
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        // POST api/Account/Logout
        [Route("LogoutBusiness")]
        public IHttpActionResult LogoutBusiness()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            if (account.AccountActivityLogSettings.RecordAccess)
            {
                string title = "You signed out.";
                string type = "user";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            if (User != null || User.Identity != null)
            {
                _accountService.ClearSession(User.Identity.GetUserId());
            }

            this.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private ErrorViewModel[] GetErrorViewModels(ModelStateDictionary modelState)
        {
            return modelState.Values
                .SelectMany(v => v.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage})).ToArray();
        }

        private ErrorViewModel[] GetErrorViewModels(IdentityResult result)
        {
            if (result == null)
            {
                return new ErrorViewModel[]
                    {new ErrorViewModel {Error = ErrorCode.InternalServerError, Message = "Something went wrong!"}};
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    return result.Errors.Select(e => new ErrorViewModel {Message = e}).ToArray();
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return new ErrorViewModel[]
                        {new ErrorViewModel {Error = ErrorCode.BadRequest, Message = "Request invalid!"}};
                }

                return ModelState
                    .SelectMany(m => m.Value.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage}))
                    .ToArray();
            }

            return null;
        }

        public class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string ExternalAccessToken { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
            public string ExternalAccessTokenSecret { get; set; }
            public string TwitterName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                if (ExternalAccessToken != null)
                {
                    claims.Add(new Claim("ExternalAccessToken", ExternalAccessToken, null, LoginProvider));
                }

                if (Email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, Email));
                }

                if (ExternalAccessTokenSecret != null)
                {
                    claims.Add(new Claim("ExternalAccessTokenSecret", ExternalAccessTokenSecret));
                }

                if (TwitterName != null)
                {
                    claims.Add(new Claim("TwitterName", TwitterName));
                }

                if (FirstName != null)
                {
                    claims.Add(new Claim("FirstName", FirstName));
                }

                if (LastName != null)
                {
                    claims.Add(new Claim("LastName", LastName));
                }

                if (DisplayName != null)
                {
                    claims.Add(new Claim("DisplayName", DisplayName));
                }

                if (Avatar != null)
                {
                    claims.Add(new Claim("Avatar", Avatar));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                                             || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name),
                    Email = identity.FindFirstValue(ClaimTypes.Email),
                    FirstName = identity.FindFirstValue("FirstName"),
                    LastName = identity.FindFirstValue("LastName"),
                    DisplayName = identity.FindFirstValue("DisplayName"),
                    Avatar = identity.FindFirstValue("Avatar"),
                    ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken"),
                    ExternalAccessTokenSecret = identity.FindFirstValue("ExternalAccessTokenSecret"),
                    TwitterName = identity.FindFirstValue("TwitterName")
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion

        public void SendEmailForLockResetPassword(Account account)
        {
            //Send Email
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_LockResetPassword.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_LockResetPassword.html");
            }

            string emailContent = string.Empty;
            string email = account.Profile.Email;
            if (File.Exists(emailTemplate) && !string.IsNullOrEmpty(email))
            {
                emailContent = File.ReadAllText(emailTemplate);


                var fullName = account.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);

                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignIn", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);

                var subject = string.Format("Your account in Regit has lock reset password.");
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {email}
                });
            }

            // End send Email
        }

        //GetValueField
        [HttpGet, Route("requesthandshake")]
        public IHttpActionResult GetEnableHandshakeRequest(string userId = null)
        {
            var account = new Account();
            if (userId != null)
            {
                var id = new MongoDB.Bson.ObjectId(userId);
                account = _accountService.GetById(id);
            }
            else
            {
                var accId = HttpContext.Current.User.Identity.GetUserId();
                account = _accountService.GetByAccountId(accId);
            }

            var field = EnumPrivacy.RequestHandshake;
            var pr = new ProfilePrivacyBusinessLogic();
            var req = pr.GetRequestHandshakePrivacy(account.AccountId, field);
            var rs = new PrivacyViewModel();
            rs.AccountId = account.AccountId;
            rs.UserId = account.Id.ToString();
            rs.Email = account.Profile.Email;
            rs.FirstName = account.Profile.FirstName;
            rs.LastName = account.Profile.LastName;
            rs.DisplayName = account.Profile.DisplayName;
            rs.AvatarUrl = account.Profile.PhotoUrl;
            rs.FieldEnable = req.Role == EnumPrivacyRole.On ? true : false;
            rs.FieldName = field;

            return Ok(rs);
        }

        [HttpPost, Route("requesthandshake/updateprivacy")]
        public IHttpActionResult UpdateEnableHandshakeRequest(Privacy privacy)
        {
            if (string.IsNullOrEmpty(privacy.Field))
                privacy.Field = EnumPrivacy.RequestHandshake;

            var accountId = HttpContext.Current.User.Identity.GetUserId();
            var field = EnumPrivacy.RequestHandshake;
            var pr = new ProfilePrivacyBusinessLogic();
            var rs = pr.UpdateRequestHandshakePrivacy(accountId, privacy.Field, privacy.Role);

            return Ok(rs);
        }

        [HttpGet, Route("UpdateNetworkTrustData")]
        public IHttpActionResult UpdateNetworkTrustData()
        {
            var netw = new NetworkService();
            netw.UpdateNetworkTrustData();
            return Ok();
        }

        [Route("devicetoken")]
        [HttpPost]
        public IHttpActionResult InsertDeviceToken(ManageTokenDevice token)
        {
            if (token == null)
                return BadRequest();

            var tokenDeviceLogic = new ManageTokenDeviceBusinessLogic();
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserId);
            var manageTokenDevice = new ManageTokenDevice();
            manageTokenDevice.AccountId = account.AccountId;
            manageTokenDevice.TokenDevice = token.TokenDevice;
            manageTokenDevice.Status = EnumStatusTokenDevice.Online;
            var rs = tokenDeviceLogic.Insert(manageTokenDevice);

            return Ok(rs);
        }

        [Route("shortprofile")]
        [HttpGet]
        public IHttpActionResult GetShortProfile(string userId = null)
        {
            if (userId == null)
                userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);
            if (account == null)
                return NotFound();
            var rs = new ShortProfileViewModel();
            rs.userId = userId;
            rs.accountType = account.AccountType == AccountType.Business ? "Business" : "Normal";
            rs.displayName = account.Profile.DisplayName;
            rs.avatarUrl = account.Profile.PhotoUrl;
            rs.firstName = account.Profile.FirstName;
            rs.lastName = account.Profile.LastName;
            rs.phone = account.Profile.PhoneNumber;
            rs.email = account.Profile.Email;
            return Ok(rs);
        }

        #region otp

        [HttpGet, Route("otp/getcode")]
        public IHttpActionResult GetCodeOtp(string option, string requestId = null)
        {
            if (string.IsNullOrEmpty(option))
            {
                return BadRequest();
            }

            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserId);
            IVerifyTokenService verifyTokenService = new VerifyTokenService();

            var token = verifyTokenService.GenerateOTP(VerifyPurpose.AUTHENTICATION, account.Profile.PhoneNumber,
                account.Profile.Email, requestId);
            _accountService.UpdateResetPasswordToken(account.Id, token.Id);
            var displayName = account.Profile.DisplayName ?? account.Profile.FirstName + " " + account.Profile.LastName;
            if (option == "email")
            {
                IMailService _mailService = new MailService();
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("CODE_AUTHENTICATION_EMAIL");
                var email = template.GetEmailContent(new {Code = token.Token, Name = displayName});
                email.SendTo = new string[] {account.Profile.Email};
                var sendMailTask = _mailService.SendMailAsync(email);
                sendMailTask.Wait();
                if (!sendMailTask.IsCompleted)
                {
                    throw new CustomException("Cannot send email. Please try again.");
                }

                verifyTokenService.MarkVerificationAsSent(token.Id);
            }
            else if (option == "sms")
            {
                var template = ConfigHelper.GetConfig<EmailTemplates>().FindByCode("CODE_AUTHENTICATION_SMS");
                var smsContent = template.GetEmailContent(new {Code = token.Token});
                var response = Nexmo.Api.SMS.Send(new Nexmo.Api.SMS.SMSRequest
                {
                    from = "REGIT",
                    to = account.Profile.PhoneNumber,
                    text = smsContent.Body
                }, Global.NexmoCredentials).messages.First();

                if (response.status == "0")
                {
                    verifyTokenService.MarkVerificationAsSent(token.Id);
                }
            }

            return Ok(token.RequestId);
        }

        [HttpPost, Route("otp/verifycode")]
        public IHttpActionResult VerifyCodeOtp(VerifyOTPViewModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            IVerifyTokenService verifyTokenService = new VerifyTokenService();
            var verify = verifyTokenService.VerifyOTP(model.RequestId, model.Code);
            if (verify.Verify == true)
            {
                _accountService.SetSMSAuthenticated(User.Identity.GetUserId());
            }

            return Ok(verify);
        }

        #endregion otp


        [HttpGet, Route("Token/Query")]
        public HttpResponseMessage GetCurrentAccessTokenInfo()
        {
            string accessToken = null;
            string authHeader = Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.Length > 20)
            {
                accessToken = Regex.Match(authHeader, @"^[Bb]earer (.+)$",
                        RegexOptions.Singleline)
                    .Groups[1].Value;
            }

            if (string.IsNullOrEmpty(accessToken))
                return Request.CreateResponse<ErrorResult>(HttpStatusCode.NotFound,
                    new ErrorResult("Error getting current access token"));

            var token = (new AuthTokensLogic()).GetByToken(accessToken);
            if (token == null)
                return Request.CreateResponse<ErrorResult>(HttpStatusCode.NotFound,
                    new ErrorResult("Access token not found"));

            return Request.CreateResponse<AuthTokenViewModel>(new AuthTokenViewModel(token));
        }

        [AllowAnonymous]
        [HttpPost, Route("Token/Query")]
        public HttpResponseMessage GetAccessTokenInfo([FromBody] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return Request.CreateResponse<ErrorResult>(HttpStatusCode.BadRequest,
                    new ErrorResult("Missing access token"));
            }

            accessToken = accessToken.Replace(" ", String.Empty);
            var token = (new AuthTokensLogic()).GetByToken(accessToken);
            if (token == null)
                return Request.CreateResponse<ErrorResult>(HttpStatusCode.NotFound,
                    new ErrorResult("Access token not found"));

            return Request.CreateResponse<AuthTokenViewModel>(new AuthTokenViewModel(token));
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessProfile
        {
            public string AccountId { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Email { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string PhoneNumber { get; set; }
        }


        [HttpGet, Route("AccessProfile")]
        public HttpResponseMessage GetAccessProfile()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            var accessProfile = new AccessProfile()
            {
                AccountId = account.AccountId,
                Email = account.Profile.Email,
                PhoneNumber = account.Profile.PhoneNumber
            };

            return Request.CreateSuccessResponse(accessProfile, "User access profile");
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessEmailUpdate
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost, Route("AccessProfile/Email")]
        public async Task<HttpResponseMessage> UpdateEmail(AccessEmailUpdate model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.Email))
                return Request.CreateApiErrorResponse("Missing new value of email address", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing required password", HttpStatusCode.BadRequest);

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);
            var identity = ContextPerRequest.Db.Users.Find(userId);

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!(await userManager.CheckPasswordAsync(identity, model.Password)))
                return Request.CreateApiErrorResponse("Incorrect password", error: "password.incorrect");

            account = _accountService.UpdateEmail(model.Email, account.Id, ContextPerRequest.Db);
            if (account == null)
                return Request.CreateApiErrorResponse("Error updating account email", error: "access.update.error");

            return Request.CreateSuccessResponse(new AccessProfile
            {
                AccountId = account.AccountId,
                Email = account.Profile.Email
            }, "Account email address changed successfully");
        }


        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessPhoneUpdate
        {
            public string PhoneNumber { get; set; }
            public string Password { get; set; }
            public bool IsInitial { get; set; }
        }

        [HttpPost, Route("AccessProfile/PhoneNumber")]
        public async Task<HttpResponseMessage> UpdatePhone(AccessPhoneUpdate model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.PhoneNumber))
                return Request.CreateApiErrorResponse("Missing new value of phone number", HttpStatusCode.BadRequest);

            if (!model.IsInitial && string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing required password", HttpStatusCode.BadRequest);

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);
            var identity = ContextPerRequest.Db.Users.Find(userId);

            if (!model.IsInitial)
            {
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

                if (!(await userManager.CheckPasswordAsync(identity, model.Password)))
                    return Request.CreateApiErrorResponse("Incorrect password", error: "password.incorrect");
            }

            model.PhoneNumber = "+" + model.PhoneNumber.Trim('+');

            account = _accountService.UpdatePhoneNumber(model.PhoneNumber, userId);
            if (account == null)
                return Request.CreateApiErrorResponse("Error updating account phone number",
                    error: "access.update.error");

            return Request.CreateSuccessResponse(new AccessProfile
            {
                AccountId = account.AccountId,
                PhoneNumber = account.Profile.PhoneNumber
            }, "Account phone number changed successfully");
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessPasswordUpdate
        {
            public string Password { get; set; }
            public string NewPassword { get; set; }
        }

        [HttpPost, Route("AccessProfile/Password")]
        public async Task<HttpResponseMessage> UpdatePasswordAsync(AccessPasswordUpdate model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.NewPassword))
                return Request.CreateApiErrorResponse("Missing new value of password", HttpStatusCode.BadRequest);

            if (model.NewPassword.Length < 8)
                return Request.CreateApiErrorResponse("Password must be at least 8 characters",
                    error: "access.update.invalid.value");

            if (string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing required current password", HttpStatusCode.BadRequest);

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);
            var identity = ContextPerRequest.Db.Users.Find(userId);

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!(await userManager.CheckPasswordAsync(identity, model.Password)))
                return Request.CreateApiErrorResponse("Incorrect current password", error: "password.incorrect");

            var resetToken = UserManager.GeneratePasswordResetToken(userId);
            var result = await UserManager.ResetPasswordAsync(userId, resetToken, model.NewPassword);

            if (!result.Succeeded)
            {
                var errorMsg = result.Errors.FirstOrDefault() ?? string.Empty;
                return Request.CreateApiErrorResponse($"Error changing password: {errorMsg}",
                    error: "access.update.error");
            }

            return Request.CreateSuccessResponse(new AccessProfile
            {
                AccountId = account.AccountId
            }, "Account password changed successfully");
        }


        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessCloseModel
        {
            public string Password { get; set; }
        }

        [HttpPost, Route("AccessProfile/Close")]
        public async Task<HttpResponseMessage> CloseAccountAsync(AccessCloseModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing required password", HttpStatusCode.BadRequest);

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(userId);

            if (account.Status == "Close")
                return Request.CreateApiErrorResponse("Account already closed", error: "account.already.closed");

            var identity = ContextPerRequest.Db.Users.Find(userId);

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!(await userManager.CheckPasswordAsync(identity, model.Password)))
                return Request.CreateApiErrorResponse("Incorrect password", error: "password.incorrect");

            var result = await _accountService.CloseAccount(account.AccountId);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error closing account", error: "account.close.error");
            try
            {
                var user = new DisabledUserService().DisabledUser(new DisabledUser
                {
                    UserId = account.Id,
                    User = account,
                    Reason = "API call",
                    IsEnabled = false,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now
                });
            }
            catch
            {
                return Request.CreateApiErrorResponse("Error disabling account", error: "account.close.error");
            }

            return Request.CreateSuccessResponse(new
            {
                accountId = account.AccountId,
                status = "closed",
                willTerminateAt = DateTime.UtcNow.AddMonths(1)
            }, "Account closed");
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessReopenModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost, Route("AccessProfile/Reopen")]
        public async Task<HttpResponseMessage> ReopenAccountAsync(AccessReopenModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (string.IsNullOrEmpty(model.Email))
                return Request.CreateApiErrorResponse("Missing email address", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Password))
                return Request.CreateApiErrorResponse("Missing required password", HttpStatusCode.BadRequest);

            var account = _accountService.GetByEmail(model.Email);
            if (account == null)
                return Request.CreateApiErrorResponse("Account not found", error: "account.notFound");

            if (account.Status != "Close")
                return Request.CreateApiErrorResponse("Account not already closed",
                    error: "account.not.already.closed");

            var identity = ContextPerRequest.Db.Users.Find(account.AccountId);

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!await userManager.CheckPasswordAsync(identity, model.Password))
                return Request.CreateApiErrorResponse("Incorrect password", error: "password.incorrect");

            var result = await _accountService.ReopenAccount(account.AccountId);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error reopening account", error: "account.reopen.error");

            return Request.CreateSuccessResponse(new
            {
                accountId = account.AccountId,
                status = "reopened",
            }, "Account temporarily reopened");
        }


        [HttpGet, Route("AccessProfile/SecurityQuestions")]
        public async Task<HttpResponseMessage> GetSecurityQuestionsAsync()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            var answers = new List<AccessSecurityAnswer>();

            var securityQuestions = new List<SecurityQuestionAnswer>
            {
                account.SecurityQuesion1,
                account.SecurityQuesion2,
                account.SecurityQuesion3
            };

            var sqCollection = MongoDBConnection.Database.GetCollection<SecurityQuestion>("SecurityQuestion");
            var builder = Builders<SecurityQuestion>.Filter;

            int count = 0;
            foreach (var securityQuestion in securityQuestions)
            {
                if (securityQuestion == null)
                {
                    answers.Add(new AccessSecurityAnswer
                    {
                        Code = "",
                        Question = "",
                        Answer = ""
                    });
                    count++;
                    continue;
                }

                var filter = builder.Eq("Id", securityQuestion.QuestionId);
                var sq = await sqCollection.Find(filter).FirstOrDefaultAsync();
                if (sq == null)
                    return Request.CreateApiErrorResponse("Error getting security questions", error: "access.error");

                answers.Add(new AccessSecurityAnswer
                {
                    Code = sq.Code,
                    Question = sq.Question,
                    Answer = securityQuestion.Answer
                });
            }

            if (count == 3)
                return Request.CreateSuccessResponse(new object[] { }, "Security answers not defined");
            return Request.CreateSuccessResponse(answers, "List user security questions and answers");
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AccessSecurityAnswerUpdate
        {
            public IList<AccessSecurityAnswer> SecurityQuestions;
        }

        [HttpPost, Route("AccessProfile/SecurityQuestions")]
        public async Task<HttpResponseMessage> SetSecurityQuestionsAsync(AccessSecurityAnswerUpdate model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);

            if (model.SecurityQuestions == null)
                return Request.CreateApiErrorResponse("Missing list of security questions", HttpStatusCode.BadRequest);

            if (model.SecurityQuestions.Count != 3)
                return Request.CreateApiErrorResponse("Invalid list: expects exactly 3 security questions",
                    HttpStatusCode.BadRequest);

            var userId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(userId);

            var sqCollection = MongoDBConnection.Database.GetCollection<SecurityQuestion>("SecurityQuestion");
            var builder = Builders<SecurityQuestion>.Filter;

            var answers = new List<SecurityQuestionAnswer>();

            for (var i = 0; i < 3; i++)
            {
                var securityQuestion = model.SecurityQuestions[i];
                if (string.IsNullOrEmpty(securityQuestion.Code))
                    return Request.CreateApiErrorResponse($"Security question #{i + 1} missing code",
                        error: "access.param.missing");
                if (string.IsNullOrEmpty(securityQuestion.Answer))
                    return Request.CreateApiErrorResponse($"Security question #{i + 1} missing answer",
                        error: "access.param.missing");
                var filter = builder.Eq("Code", securityQuestion.Code);
                var sq = await sqCollection.Find(filter).FirstOrDefaultAsync();
                if (sq == null)
                    return Request.CreateApiErrorResponse(
                        $"Security question #{i + 1} unknown code: {securityQuestion.Code}",
                        error: "access.param.invalid");
                answers.Add(new SecurityQuestionAnswer
                {
                    QuestionId = sq.Id,
                    Answer = securityQuestion.Answer
                });
            }

            var accountCollection = MongoDBConnection.Database.GetCollection<Account>("Account");

            var filterAccount = Builders<Account>.Filter.Eq("AccountId", userId);
            var update = Builders<Account>.Update;
            for (var i = 0; i < 3; i++)
            {
                var answer = answers[i];
                var result =
                    await accountCollection.UpdateOneAsync(filterAccount,
                        update.Set($"SecurityQuesion{i + 1}", answer));
            }

            return Request.CreateSuccessResponse(new {updatedCount = 3},
                "User security questions and answers updated successfully");
        }
    }
}