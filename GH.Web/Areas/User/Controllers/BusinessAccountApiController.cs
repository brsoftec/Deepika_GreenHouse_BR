using GH.Core.Exceptions;
using GH.Web.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using GH.Core.Services;
using GH.Core.Models;
using GH.Core.Helpers;
using MongoDB.Bson;
using GH.Core.Adapters;
using GH.Core.ViewModels;
using Newtonsoft.Json.Linq;
using System.IO;
using GH.Core.Extensions;
using GH.Core.IServices;
using GH.Web.Areas.SocialNetwork.ViewModels;
using GH.Core.BlueCode.BusinessLogic;
using GH.Util;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.InformationVault;
using WizLocal.BL.Core;
using Core.Common;
using GH.Core.BlueCode.Entity.Payment;
using Stripe;
using GH.Core.BlueCode.BusinessLogic.Payment;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.Entity.Request;
using System.Configuration;
using GH.Web.Helpers;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("api/BusinessAccount")]
    public class BusinessAccountApiController : BaseApiController
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IAccountService _accountService;
        private INetworkService _networkService;
        private IJoiningBusinessInvitationService _joinBusService;
        private IRoleService _roleService;
        private ISocialPageService _socialPageService;
        private INotificationService _notifyService;
        private ISocialService _socialService;
        private ITransactionService _transactionService;

        public BusinessAccountApiController()
        {
            _accountService = new AccountService();
            _networkService = new NetworkService();
            _joinBusService = new JoiningBusinessInvitationService();
            _roleService = new RoleService();
            _socialPageService = new SocialPageService();
            _notifyService = new NotificationService();
            _socialService = new SocialService();
            _transactionService = new TransactionService();
        }

        [AllowAnonymous]
        [HttpPost, Route("ValidateRegistration")]
        public async Task ValidateBusinessAccountRegistration(BusinessSignUpAccountInfo model)
        {
            if (model != null)
            {
                if (string.IsNullOrEmpty(model.FirstName))
                {
                    ModelState.AddModelError("FirstName", GH.Lang.Regit.First_Name_Error_Required);
                }

                if (string.IsNullOrEmpty(model.LastName))
                {
                    ModelState.AddModelError("LastName", GH.Lang.Regit.Last_Name_Error_Required);
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    bool duplicated = _accountService.CheckDuplicateEmail(model.Email, null);

                    if (duplicated)
                    {
                        ModelState.AddModelError("Email", GH.Lang.Regit.Email_Error_Not_Available);
                    }
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", GH.Lang.Regit.Password_Error_Required);
                }
                else
                {
                    var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var validateResult = await userManager.PasswordValidator.ValidateAsync(model.Password);
                    if (!validateResult.Succeeded)
                    {
                        ModelState.AddModelError("Password", GH.Lang.Regit.Password_Error_Invalidated);
                    }
                    else if (model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("Password", GH.Lang.Regit.Confirm_password_do_not_match);
                    }
                }

                string formatedNumber = null;

                if (!string.IsNullOrEmpty(model.PhoneNumber) &&
                    !string.IsNullOrEmpty(model.PhoneNumberCountryCallingCode))
                {
                    try
                    {
                        formatedNumber =
                            PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.PhoneNumberCountryCallingCode +
                                                                     model.PhoneNumber);
                    }
                    catch (CustomException ex)
                    {
                        ModelState.AddModelError("PhoneNumber", string.Join(". ", ex.Errors.Select(e => e.Message)));
                    }
                }

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var result = await userManager.PasswordValidator.ValidateAsync(model.Password);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("Password", string.Join(". ", result.Errors.Select(e => e)));
                    }
                }

                if (!ModelState.IsValid)
                {
                    throw new CustomException(ModelState);
                }
            }
            else
            {
                throw new CustomException("Invalid request");
            }
        }

        [HttpGet, Route("GetConnectedFacebookPage")]
        public async Task<SocialPage> GetConnectedFacebookPage()
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            return account.Pages.FirstOrDefault(p => p.SocialType == SocialType.Facebook);
        }

        #region API dataexample

        [AllowAnonymous]
        [Route("RegisterBusinessAccountDataExample")]
        public IHttpActionResult RegisterBusinessAccountDataExample(RegisterBusinessAccountModel model,
            int numbercampaign, int countcampaignkeywords, int countdays)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            /* if (!model.Account.AgreeTermsAndCondition)
             {
                 throw new CustomException("You must agree terms and condition");
             }

             await ValidateBusinessAccountRegistration(model.Account);*/

            //string formatedNumber = null;

            //formatedNumber = PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.Account.PhoneNumberCountryCallingCode + model.Account.PhoneNumber);

            // if (!ModelState.IsValid)
            //{
            // throw new CustomException(ModelState);
            //}

            /* disable verify pin
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

            Account currentUser = null;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var currentAccountId = HttpContext.Current.User.Identity.GetUserId();
                currentUser = _accountService.GetByAccountId(currentAccountId);
            }

            var newIUser = new ApplicationUser {UserName = model.Account.Email, Email = model.Account.Email};
            var createResult = userManager.Create(newIUser, model.Account.Password);

            if (!createResult.Succeeded)
            {
                throw new CustomException(createResult.Errors.Select(e => new ErrorViewModel {Message = e}).ToArray());
            }

            var account = new Account
            {
                AccountId = newIUser.Id,
                AccountType = AccountType.Business,
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
                    FirstName = model.Account.FirstName,
                    LastName = model.Account.LastName,
                    Email = model.Account.Email,
                    PhoneNumber = model.Account.PhoneNumber,
                    DisplayName = model.CompanyDetails.DisplayName,
                    Description = model.CompanyDetails.Description
                },
                CompanyDetails = new CompanyDetails
                {
                    CompanyName = model.CompanyDetails.CompanyName,
                    Industry = model.CompanyDetails.Industry,
                    Website = model.CompanyDetails.Website
                },
                BusinessPrivacies = new BusinessPrivacy()
                {
                    AllowComment = true,
                    Privacy = AccountPrivacy.Public
                },
                BusinessAccountRoles = new List<BusinessAccountRole>(),
                Pages = new List<SocialPage>()
            };

            if (model.AddtionalInfo != null)
            {
                if (!string.IsNullOrEmpty(model.AddtionalInfo.Avatar))
                {
                    try
                    {
                        var name = Guid.NewGuid().ToString().Replace("-", "") + "_profile_pic_" + DateTime.Now.Ticks +
                                   ".jpeg";
                        var fullpath = AccountService.PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name;
                        FileAccessHelper.SaveBase64String(model.AddtionalInfo.Avatar,
                            AccountService.PROFILE_PICTURE_DIRECTORY, name);

                        account.Profile.PhotoUrl = fullpath;
                    }
                    catch (Exception)
                    {
                    }
                }

                account.CompanyDetails.WorkHourFrom = model.AddtionalInfo.WorkHourFrom;
                account.CompanyDetails.WorkHourTo = model.AddtionalInfo.WorkHourTo;
                if (model.AddtionalInfo.Workdays != null &&
                    !model.AddtionalInfo.Workdays.Any(w => !CompanyDetails.AllWorkdays.Contains(w)))
                {
                    account.CompanyDetails.Workdays = model.AddtionalInfo.Workdays;
                }
            }

            if (currentUser != null)
            {
                account.Creator = currentUser.Id;

                if (model.Account.LinkWithPersonal)
                {
                    IRoleService _roleService = new RoleService();
                    var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                    account.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = currentUser.Id,
                        RoleId = adminRole.Id
                    });
                }
            }

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

            var normalNetwork = new Network
            {
                Code = Network.NORMAL,
                Name = Network.NORMAL_NETWORK,
                NetworkOwner = account.Id,
                NetworkOwnerAccountId = User.Identity.GetUserId()
            };

            var trustedNetwork = new Network
            {
                Code = Network.TRUSTED,
                Name = Network.TRUSTED_NETWORK,
                NetworkOwner = account.Id,
                NetworkOwnerAccountId = User.Identity.GetUserId()
            };
            _networkService.InsertNetwork(normalNetwork);
            _networkService.InsertNetwork(trustedNetwork);
            //create campaign

            if (numbercampaign <= 3)
                numbercampaign = 3;
            if (countcampaignkeywords <= 10)
                countcampaignkeywords = 10;
            if (countdays < 30)
                countdays = 30;
            List<FieldinformationVault> listfieldvaults = new List<FieldinformationVault>();
            listfieldvaults.Add(new FieldinformationVault
            {
                displayName = "First Name",
                id = "576187da9e9e822f503fd875",
                jsPath = ".basicInformation.firstName",
                label = "First Name",
                optional = false,
                type = "textbox",
            });
            listfieldvaults.Add(new FieldinformationVault
            {
                displayName = "Title",
                id = "576187da9e9e822f503fd875",
                jsPath = ".basicInformation.title",
                label = "Title",
                optional = false,
                type = "textbox",
            });


            listfieldvaults.Add(new FieldinformationVault
            {
                displayName = "Last Name",
                id = "576187da9e9e822f503fd875",
                jsPath = ".basicInformation.lastName",
                label = "Last Name",
                optional = false,
                type = "textbox",
            });
            var type = "";
            string campaignid = "";


            CampaignBusinessLogic campaignBusinessLogic = new CampaignBusinessLogic();
            InfomationVaultBusinessLogic infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
            PostBusinessLogic postBusinessLogic = new PostBusinessLogic();
            BusinessMemberLogic businessMemberLogic = new BusinessMemberLogic();
            AccountService accountService = new AccountService();
            List<string> memberinVaults = new List<string>();
            for (int i = 0; i < numbercampaign; i++)
            {
                if (i == 0)
                {
                    type = "Advertising";
                }
                else if (i == 1)
                    type = "Registration";
                else if (i == 2)
                    type = "Event";
                else
                    type = ExampleDataHelper.GetRandomCampaignType();

                switch (type)
                {
                    case "Registration":
                        var campaignRegistration = new CampaignRegistration();
                        campaignRegistration.type = type;
                        campaignRegistration.name = model.Account.FirstName + model.Account.LastName + "campaignname " +
                                                    i;
                        campaignRegistration.description = model.Account.FirstName + model.Account.LastName +
                                                           "campaigndescription " + i;
                        var listkeywords = ExampleDataHelper.GetRandomListKeywordsForCampaign(countcampaignkeywords);
                        campaignRegistration.status = "Active";
                        campaignRegistration.qrCode.PublicURL =
                            ConfigHelp.GetStringValue("URLCampaignPublic") + "Registration/" +
                            BsonHelper.GenerateObjectIdString();
                        campaignRegistration.qrCode.AllowCreateQrCode = true;


                        campaignRegistration.criteria.keywords = listkeywords.ToList<object>();
                        campaignRegistration.fields = listfieldvaults.ToList<object>()
                            .Select(x => (FieldinformationVault) x).ToList();
                        campaignid =
                            campaignBusinessLogic.InsertCampaign(account.AccountId,
                                campaignRegistration.ToBsonDocument());

                        memberinVaults = infomationVaultBusinessLogic.Getaccountidsfromkeyworkinvault(listkeywords);
                        foreach (var userid in memberinVaults)
                        {
                            Account accuser = accountService.GetByAccountId(userid);
                            DateTime dateadd = ExampleDataHelper.GetRandomDateFromstartDate(countdays);
                            postBusinessLogic.RegisterCampaign(accuser, account, campaignid, type,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            businessMemberLogic.AddBusinessMember(accuser, account,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            accountService.FollowBusiness(accuser.Id, account.Id, dateadd);
                        }

                        break;
                    case "Event":
                        var campaignEvent = new CampaignEvent();
                        campaignEvent.type = type;
                        campaignEvent.name = model.Account.FirstName + model.Account.LastName + "campaignname " + i;
                        campaignEvent.description = model.Account.FirstName + model.Account.LastName +
                                                    "campaigndescription " + i;
                        campaignEvent.status = "Active";
                        campaignEvent.qrCode.PublicURL = ConfigHelp.GetStringValue("URLCampaignPublic") + "Event/" +
                                                         BsonHelper.GenerateObjectIdString();
                        campaignEvent.qrCode.AllowCreateQrCode = true;
                        var listkeywords1 = ExampleDataHelper.GetRandomListKeywordsForCampaign(countcampaignkeywords);
                        campaignEvent.criteria.keywords = listkeywords1.ToList<object>();
                        campaignEvent.fields = listfieldvaults.ToList<object>().Select(x => (FieldinformationVault) x)
                            .ToList();
                        campaignid =
                            campaignBusinessLogic.InsertCampaign(account.AccountId, campaignEvent.ToBsonDocument());

                        memberinVaults = infomationVaultBusinessLogic.Getaccountidsfromkeyworkinvault(listkeywords1);
                        foreach (var userid in memberinVaults)
                        {
                            Account accuser = accountService.GetByAccountId(userid);
                            DateTime dateadd = ExampleDataHelper.GetRandomDateFromstartDate(countdays);
                            postBusinessLogic.RegisterCampaign(accuser, account, campaignid, type,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            businessMemberLogic.AddBusinessMember(accuser, account,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            accountService.FollowBusiness(accuser.Id, account.Id, dateadd);
                        }

                        break;
                    case "Advertising":
                        var campaignAdvertising = new CampaignAdvertising();
                        campaignAdvertising.type = type;
                        campaignAdvertising.name = model.Account.FirstName + model.Account.LastName + "campaignname " +
                                                   i;
                        campaignAdvertising.description = model.Account.FirstName + model.Account.LastName +
                                                          "campaigndescription " + i;
                        campaignAdvertising.status = "Active";
                        var listkeywords2 = ExampleDataHelper.GetRandomListKeywordsForCampaign(countcampaignkeywords);
                        campaignAdvertising.criteria.keywords = listkeywords2.ToList<object>();
                        campaignid =
                            campaignBusinessLogic.InsertCampaign(account.AccountId,
                                campaignAdvertising.ToBsonDocument());
                        memberinVaults = infomationVaultBusinessLogic.Getaccountidsfromkeyworkinvault(listkeywords2);
                        foreach (var userid in memberinVaults)
                        {
                            Account accuser = accountService.GetByAccountId(userid);
                            DateTime dateadd = ExampleDataHelper.GetRandomDateFromstartDate(countdays);
                            postBusinessLogic.RegisterCampaign(accuser, account, campaignid, type,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            businessMemberLogic.AddBusinessMember(accuser, account,
                                dateadd.ToString("yyyy-MM-dd hh:mm:ss"));
                            accountService.FollowBusiness(accuser.Id, account.Id, dateadd);
                        }

                        break;
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
        [Route("RegistersManyUsersWithExampleData")]
        public IHttpActionResult RegistersManyUsersWithExampleData(RegistersManyUsersWithExampleDataModel model)
        {
            if (model == null)
            {
                model = new RegistersManyUsersWithExampleDataModel();
            }

            if (model.CountUsers == 0)
                model.CountUsers = 1;
            if (model.NmberRandomKeyword == 0)
                model.NmberRandomKeyword = 4;
            if (string.IsNullOrEmpty(model.Firstname))
                model.Firstname = "User";
            if (string.IsNullOrEmpty(model.Lastname))
                model.Lastname = "Bus";

            for (int i = 0; i < model.CountUsers; i++)
            {
                var modeluser = new RegisterBusinessAccountModel
                {
                    Account = new BusinessSignUpAccountInfo
                    {
                        FirstName = model.Firstname + i,
                        LastName = model.Lastname + i,
                        Email = model.Firstname + model.Lastname + i + "@gmail.com",
                        PhoneNumber = ExampleDataHelper.GetRandomPhoneNumber("+84"),
                        Password = "Test@123"
                    },
                    CompanyDetails = new SignUpCompanyDetails
                    {
                        CompanyName = "CompanyName" + model.Firstname + model.Lastname + i,
                        Industry = "Boat",
                        DisplayName = model.Firstname + " " + model.Lastname + i,
                        Website = "http://" + model.Firstname + model.Lastname + i + ".com"
                    }
                };
                var result = RegisterBusinessAccountDataExample(modeluser, model.numbercampaign,
                    model.countcampaignkeywords, model.countdays);
            }

            return Ok();
        }

        #endregion

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> RegisterBusinessAccount(RegisterBusinessAccountModel model)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!model.Account.AgreeTermsAndCondition)
            {
                throw new CustomException("You must agree terms and condition");
            }

            await ValidateBusinessAccountRegistration(model.Account);

            string formatedNumber = null;

            formatedNumber =
                PhoneNumberHelper.GetFormatedPhoneNumber("+" + model.Account.PhoneNumberCountryCallingCode +
                                                         model.Account.PhoneNumber);

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }


            Account currentUser = null;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var currentAccountId = HttpContext.Current.User.Identity.GetUserId();
                currentUser = _accountService.GetByAccountId(currentAccountId);
            }

            var newIUser = new ApplicationUser {UserName = model.Account.Email, Email = model.Account.Email};
            var createResult = await userManager.CreateAsync(newIUser, model.Account.Password);

            if (!createResult.Succeeded)
            {
                throw new CustomException(createResult.Errors.Select(e => new ErrorViewModel {Message = e}).ToArray());
            }

            var account = new Account
            {
                AccountId = newIUser.Id,
                AccountType = AccountType.Business,
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
                    FirstName = model.Account.FirstName,
                    LastName = model.Account.LastName,
                    Email = model.Account.Email.ToLower(),
                    PhoneNumber = formatedNumber,
                    DisplayName = model.CompanyDetails.DisplayName,
                    Description = model.CompanyDetails.Description,
                    Street = model.CompanyDetails.Address,
                    Country = model.CompanyDetails.Country
                },
                CompanyDetails = new CompanyDetails
                {
                    CompanyName = model.CompanyDetails.CompanyName,
                    Industry = model.CompanyDetails.Industry,
                    Website = model.CompanyDetails.Website,
                    Country = model.CompanyDetails.Country,
                    Description = model.CompanyDetails.Description,
                    Street = model.CompanyDetails.Address
                },
                BusinessPrivacies = new BusinessPrivacy()
                {
                    AllowComment = true,
                    Privacy = AccountPrivacy.Public
                },
                BusinessAccountRoles = new List<BusinessAccountRole>(),
                Pages = new List<SocialPage>()
            };

            if (model.AddtionalInfo != null)
            {
                if (!string.IsNullOrEmpty(model.AddtionalInfo.Avatar))
                {
                    try
                    {
                        var name = Guid.NewGuid().ToString().Replace("-", "") + "_profile_pic_" + DateTime.Now.Ticks +
                                   ".jpeg";
                        var fullpath = AccountService.PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name;
                        FileAccessHelper.SaveBase64String(model.AddtionalInfo.Avatar,
                            AccountService.PROFILE_PICTURE_DIRECTORY, name);

                        account.Profile.PhotoUrl = fullpath;
                    }
                    catch (Exception)
                    {
                    }
                }

                account.CompanyDetails.WorkHourFrom = model.AddtionalInfo.WorkHourFrom;
                account.CompanyDetails.WorkHourTo = model.AddtionalInfo.WorkHourTo;
                if (model.AddtionalInfo.Workdays != null &&
                    !model.AddtionalInfo.Workdays.Any(w => !CompanyDetails.AllWorkdays.Contains(w)))
                {
                    account.CompanyDetails.Workdays = model.AddtionalInfo.Workdays;
                }
            }

            if (currentUser != null)
            {
                account.Creator = currentUser.Id;

                if (model.Account.LinkWithPersonal)
                {
                    IRoleService _roleService = new RoleService();
                    var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                    account.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = currentUser.Id,
                        RoleId = adminRole.Id
                    });
                }
            }

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

            var normalNetwork = new Network
            {
                Code = Network.NORMAL,
                Name = Network.NORMAL_NETWORK,
                NetworkOwner = account.Id,
                NetworkOwnerAccountId = User.Identity.GetUserId()
            };

            var trustedNetwork = new Network
            {
                Code = Network.TRUSTED,
                Name = Network.TRUSTED_NETWORK,
                NetworkOwner = account.Id,
                NetworkOwnerAccountId = User.Identity.GetUserId()
            };
            _networkService.InsertNetwork(normalNetwork);
            _networkService.InsertNetwork(trustedNetwork);


            //WRITE LOG
            if (User != null)
            {
                if (account.AccountActivityLogSettings.RecordAccess)
                {
                    string title = "Your business account was registered.";
                    string type = "user";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
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

        [HttpGet, Route("SearchMembersForInvitation")]
        public async Task<List<FriendInNetworkInfo>> SearchMembersForInvitation(string keyword, int? start = null,
            int? length = null)
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (account.AccountType == AccountType.Personal)
            {
                var busAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(account.Id)
                    .FirstOrDefault();

                if (busAccount == null)
                {
                    throw new CustomException("Invalid request");
                }

                account = busAccount;
            }

            var users = _joinBusService.SearchUserForInvitation(keyword, account.Id, start, length);

            return users.Select(u => new FriendInNetworkInfo
            {
                Id = u.Id.ToString(),
                Avatar = u.Profile.PhotoUrl,
                DisplayName = u.Profile.DisplayName
            }).ToList();
        }

        [HttpPost, Route("Invite/Accept")]
        public async Task AcceptInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var accepter = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            //WRITE LOG
            if (accepter != null && model.InvitationId != null)
            {
                var accountInvite = _accountService.GetByAccountId(model.InvitationId);
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordWorkflow)
                {
                    string title = "You accepted invitation from another user.";
                    string type = "user";
                    if (accountInvite.Profile.DisplayName != null)
                    {
                        title = "You accepted invite from " + accountInvite.Profile.DisplayName.ToString() + ".";
                    }

                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }

            _joinBusService.Accept(new MongoDB.Bson.ObjectId(model.InvitationId), accepter.Id);
        }

        [HttpPost, Route("Invite/Deny")]
        public async Task DenyInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var denier = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            //WRITE LOG
            if (denier != null && model.InvitationId != null)
            {
                var accountInvite = _accountService.GetByAccountId(model.InvitationId);
                if (denier.AccountActivityLogSettings.RecordWorkflow)
                {
                    string title = "You denied invitation from another user.";
                    string type = "user";
                    if (accountInvite.Profile.DisplayName != null)
                    {
                        title = "You denied invitation from " + accountInvite.Profile.DisplayName.ToString() + ".";
                    }

                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(denier.AccountId, title, type);
                }
            }

            _joinBusService.Deny(new MongoDB.Bson.ObjectId(model.InvitationId), denier.Id);
        }

        [HttpGet, Route("Roles")]
        public async Task<List<Role>> GetRoles()
        {
            return _roleService.GetAllRoles();
        }

        [HttpGet, Route("Members")]
        public async Task<MembersInBusinessViewModel> GetMemberInBusiness(int? start = null, int? length = null)
        {
            var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (busAccount.AccountType == AccountType.Personal)
            {
                busAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(busAccount.Id).FirstOrDefault();

                if (busAccount == null)
                {
                    throw new CustomException("Invalid request");
                }
            }

            long total = 0;
            var members = _accountService.GetMembersInBusiness(busAccount.Id, start, length, out total);

            return new MembersInBusinessViewModel
            {
                Total = total,
                Members = members.Select(m => new MemberInBusiness
                {
                    AccountId = m.Id,
                    Avatar = m.Profile.PhotoUrl,
                    DisplayName = m.Profile.DisplayName,
                    RoleIds = m.BusinessAccountRoles.Where(b => b.AccountId == busAccount.Id).Select(a => a.RoleId)
                        .ToList(),
                }).ToList()
            };
        }

        [HttpPost, Route("UpdateMembers")]
        public async Task<object> UpdateMembersInBusiness(List<UpdateMemberModel> model)
        {
            var modifier = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            ObjectId businessAccountId;
            if (modifier.AccountType == AccountType.Business)
            {
                businessAccountId = modifier.Id;
            }
            else
            {
                businessAccountId = modifier.BusinessAccountRoles.First().AccountId;
            }

            var roles = _roleService.GetAllRoles();
            var adminRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_ADMIN);
            var reviewerRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_REVIEWER);
            var editorRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_EDITOR);

            List<object> errors = new List<object>();
            foreach (var action in model)
            {
                var userAccount = _accountService.GetById(new ObjectId(action.Member.AccountId));
                try
                {
                    switch (action.Action)
                    {
                        case UpdateMemberAction.UPDATE:
                            List<ObjectId> updateRoles = new List<ObjectId>();
                            if (action.Member.IsAdmin)
                            {
                                updateRoles.Add(adminRole.Id);
                                //WRITE LOG
                                if (userAccount != null)
                                {
                                    //if (userAccount.AccountActivityLogSettings.RecordAccess)
                                    //{
                                    string title = userAccount.Profile.DisplayName + " updated to " + Role.ROLE_ADMIN;
                                    var act = new ActivityLogBusinessLogic();
                                    act.WriteActivityLogFromAcc(userAccount.AccountId, title, "", modifier.AccountId);
                                    //}
                                }
                            }

                            if (action.Member.IsReviewer)
                            {
                                updateRoles.Add(reviewerRole.Id);
                                if (userAccount != null)
                                {
                                    //if (userAccount.AccountActivityLogSettings.RecordAccess)
                                    //{
                                    string title = userAccount.Profile.DisplayName + " updated to " +
                                                   Role.ROLE_REVIEWER;
                                    var act = new ActivityLogBusinessLogic();
                                    act.WriteActivityLogFromAcc(userAccount.AccountId, title, "", modifier.AccountId);
                                    //}
                                }
                            }

                            if (action.Member.IsEditor)
                            {
                                updateRoles.Add(editorRole.Id);
                                //updateRoles.Add(reviewerRole.Id);
                                if (userAccount != null)
                                {
                                    //if (userAccount.AccountActivityLogSettings.RecordAccess)
                                    //{
                                    string title = userAccount.Profile.DisplayName + " updated to " + Role.ROLE_EDITOR;
                                    var act = new ActivityLogBusinessLogic();
                                    act.WriteActivityLogFromAcc(userAccount.AccountId, title, "", modifier.AccountId);
                                    //}
                                }
                            }

                            _accountService.UpdateMemberRoleInBusiness(businessAccountId,
                                new ObjectId(action.Member.AccountId), updateRoles, modifier.Id);

                            break;
                        case UpdateMemberAction.DELETE:
                            _accountService.RemoveMemberFromBusiness(businessAccountId,
                                new ObjectId(action.Member.AccountId), modifier.Id);
                            break;
                        default:
                            break;
                    }
                }
                catch (CustomException ex)
                {
                    errors.Add(new
                    {
                        Action = action.Action.ToString(),
                        Id = action.Member.AccountId,
                        Handled = true,
                        Message = string.Join(", ", ex.Errors.Select(e => e.Message))
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(new {Action = action.Action.ToString(), Id = action.Member.AccountId, Handled = false});
                }
            }

            return new
            {
                Success = !errors.Any(),
                Errors = errors
            };
        }


        [HttpPost, Route("RejectPost")]
        public async Task RejectPost(RejectBusinessPostModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var currentUser = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            var post = _socialPageService.RejectPost(model.Id.ParseToObjectId(), model.Comment, currentUser.Id);

            _notifyService.AddNotifications(new Notification
            {
                CreatedAt = DateTime.Now,
                Creator = currentUser.Id,
                ReceiverId = post.AccountId,
                TargetId = post.Id,
                TargetType = NotificationTargetType.RejectPost
            });
        }

        [HttpPost, Route("ApprovePost")]
        public async Task ApprovePost(ApproveBusinessPostModel model)
        {
            if (!ModelState.IsValid)
                throw new CustomException(ModelState);

            var currentUser = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            var businessPost = _socialPageService.GetBusinessPost(model.Id.ParseToObjectId());

            var businessUser = _accountService.GetById(businessPost.BusinessAccountId);

            _socialPageService.ApprovePost(model.Id.ParseToObjectId(), currentUser.Id);
            _notifyService.AddNotifications(new Notification
            {
                CreatedAt = DateTime.Now,
                Creator = currentUser.Id,
                ReceiverId = businessPost.AccountId,
                TargetId = businessPost.Id,
                TargetType = NotificationTargetType.ApprovePost
            });
            //post after approve business post
            var socialPost = SocialAdapter.BusinessPostToModel(businessPost);
            foreach (var type in businessPost.SocialTypes)
            {
                socialPost.Type = type;
                socialPost.Id = ObjectId.Empty;
                if (type == SocialType.GreenHouse || (businessPost.SocialTypes.Count > 1 &&
                                                      string.IsNullOrEmpty(socialPost.GroupId)))
                {
                    socialPost.Id = ObjectId.Empty;
                    var savedPost =
                        _socialService.PostNewPost(
                            new PostSocialNetworkViewModel {Post = socialPost, Type = SocialType.GreenHouse}, null,
                            null, null);
                    if (string.IsNullOrEmpty(socialPost.GroupId))
                    {
                        socialPost.GroupId = savedPost.Id.ToString();
                    }
                }

                if (type == SocialType.Facebook)
                {
                    if (businessUser.Pages != null && businessUser.Pages.Count > 0)
                    {
                        socialPost.Id = ObjectId.Empty;
                        string fbPageAccessToken = businessUser.Pages[0].AccessToken;
                        var savedPost =
                            _socialService.PostNewPost(
                                new PostSocialNetworkViewModel
                                {
                                    Post = socialPost,
                                    Type = SocialType.Facebook,
                                    PageId = businessUser.Pages[0].Id,
                                    IsPostToFacebookPage = true
                                }, fbPageAccessToken, null, businessUser.Pages[0].Id);
                        if (string.IsNullOrEmpty(socialPost.GroupId))
                        {
                            socialPost.GroupId = savedPost.Id.ToString();
                        }
                    }
                }

                if (type == SocialType.Twitter)
                {
                    string twAccessToken = "", twSecrect = "";
                    var twLink = businessUser.AccountLinks.FirstOrDefault(t => t.Type == SocialType.Twitter);
                    if (twLink != null)
                    {
                        socialPost.Id = ObjectId.Empty;
                        twAccessToken = twLink != null ? twLink.AccessToken : "";
                        twSecrect = twLink != null ? twLink.AccessTokenSecret : "";
                        var savedPost =
                            _socialService.PostNewPost(
                                new PostSocialNetworkViewModel {Post = socialPost, Type = SocialType.Twitter},
                                twAccessToken, twSecrect, twLink.SocialAccountId);
                        if (string.IsNullOrEmpty(socialPost.GroupId))
                        {
                            socialPost.GroupId = savedPost.Id.ToString();
                        }
                    }
                }
            }
        }


        [HttpGet, Route("PictureAlbum")]
        public async Task<List<string>> GetBusinessPictureAlbum(string businessId = null)
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (string.IsNullOrEmpty(businessId))
            {
                if (busUser.AccountType == AccountType.Personal)
                {
                    var busId = busUser.BusinessAccountRoles.FirstOrDefault();
                    if (busId != null)
                    {
                        busUser = _accountService.GetById(busId.AccountId);
                    }
                    else
                    {
                        throw new CustomException("Invalid request");
                    }
                }
            }
            else
            {
                busUser = _accountService.GetById(new ObjectId(businessId));
            }

            if (busUser == null)
            {
                throw new CustomException("Invalid request");
            }

            return busUser.PictureAlbum;
        }

        [HttpPost, Route("PictureAlbum")]
        public async Task<List<string>> UpdateBusinessPictureAlbum()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            // Read the form data.
            await Request.Content.ReadAsMultipartAsync(provider);

            try
            {
                var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

                if (busUser.AccountType == AccountType.Personal)
                {
                    var busId = busUser.BusinessAccountRoles.FirstOrDefault();
                    if (busId != null)
                    {
                        busUser = _accountService.GetById(busId.AccountId);
                    }
                    else
                    {
                        throw new CustomException("Invalid request");
                    }
                }

                List<MultipartFileData> album = provider.FileData.ToList();

                var deletePhotos = new List<string>();
                if (!string.IsNullOrEmpty(provider.FormData["DeletePhotos"]))
                {
                    deletePhotos = provider.FormData["DeletePhotos"].Split(',').ToList();
                }

                var exist = _accountService.UpdateBusinessAccountPictureAlbum(busUser.Id, album, deletePhotos);
                return exist.PictureAlbum;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                var deleteFiles = provider.FileData.ToList();
                foreach (var file in deleteFiles)
                {
                    FileAccessHelper.DeleteFileWithAbsoluteFilePath(file.LocalFileName.Trim('"'));
                }
            }
        }

        [HttpGet, Route("WorkTime")]
        public async Task<WorkTimeViewModel> GetBusinessWorktime()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            return new WorkTimeViewModel
            {
                WorkHourFrom = busUser.CompanyDetails.WorkHourFrom,
                WorkHourTo = busUser.CompanyDetails.WorkHourTo,
                Workdays = busUser.CompanyDetails.Workdays
            };
        }

        [HttpPost, Route("WorkTime")]
        public async Task UpdateBusinessWorktime(WorkTimeViewModel model)
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            _accountService.UpdateWorkTime(busUser.Id, model.WorkHourFrom, model.WorkHourTo, model.Workdays);
        }


        [HttpGet, Route("GetPushVaultBus")]
        public async Task<BusinessProfileViewModel> GetPushVaultBus()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            string busid = "";

            if (currentUser.AccountType == AccountType.Business)
            {
                busid = currentUser.AccountId.ToString();
            }
            else
            {
                busid = currentUser.AccountId.ToString();
            }

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

            return new BusinessProfileViewModel
            {
                Id = busid,
                DisplayName = currentUser.Profile.DisplayName,
                ListPushToVault = newFeedList
            };
        }

        [HttpGet, Route("GetPushVaultBusiness")]
        public HttpResponseMessage GetPushVaultBusiness()
        {
            var rs = new BusinessPushVaulViewModel();
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            var camp = new CampaignBusinessLogic();
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, rs);
            try
            {
                var userAccount = BusinessAccount;
                rs.Id = userAccount.AccountId;
                rs.DisplayName = userAccount.Profile.DisplayName;
                rs.ListPushToVault = camp.GetAllPushVaultByUser(userAccount.AccountId);
            }
            catch
            {
                response = Request.CreateResponse<object>(HttpStatusCode.BadRequest, rs);
            }

            response = Request.CreateResponse<object>(HttpStatusCode.OK, rs);
            return response;
        }


        [HttpGet, Route("PublicProfile")]
        public async Task<BusinessProfileViewModel> GetBusinessPublicProfile(string businessId = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());


            if (string.IsNullOrEmpty(businessId))
            {
                if (currentUser.AccountType == AccountType.Business)
                {
                    businessId = currentUser.Id.ToString();
                }
                else
                {
                    throw new CustomException("Business profile does not exist");
                }
            }

            ObjectId busObjId = new ObjectId();
            if (!ObjectId.TryParse(businessId, out busObjId))
            {
                throw new CustomException("Business profile does not exist");
            }

            var businessAccount = _accountService.GetById(busObjId);

            if (businessAccount == null)
            {
                throw new CustomException("Business profile does not exist");
            }

            if (businessAccount.AccountType == AccountType.Personal)
            {
                throw new CustomException("Business profile does not exist");
            }

            var newFeedList = new List<NewFeedsViewModel>();
            var strbusinessList = new List<string>();
            var strcampaignIdList = new List<string>();
            var strBusinessCampaignIdList = new List<string>();
            var campaignListSRFI = new List<CampaignItemForHomeFeed>();
            bool followed = false;
            AccountType accounttype = AccountType.Personal;
            if (currentUser.AccountType != AccountType.Business)
            {
                followed = currentUser.Followees.Any(f => f.AccountId == businessAccount.Id);

                var campaignList = new CampaignBusinessLogic()
                    .GetActiveCampaignsForUser(currentUser.AccountId, "", "All", false)
                    .Where(x => x.BusinessUserId == businessAccount.AccountId).ToList();

                campaignListSRFI = campaignList.Where(x => x.CampaignType == "SRFI").ToList();
                var campaignListwithoutSRFI = campaignList.Where(x => x.CampaignType != "SRFI").ToList();
                var businessCampaignList = new PostBusinessLogic().GetFollowedBusinessByUserId(currentUser.AccountId)
                    .DataOfCurrentPage;
                var businessList = new BusinessMemberLogic().GetFollowingBusinesses(currentUser.AccountId).ToList();
                //var postList = PostBusinessLogic.GetPostList();
                var postList = new PostBusinessLogic().GetPostListbyUserId(currentUser.AccountId);


                foreach (var item in campaignListwithoutSRFI.OrderByDescending(x => x.Id))
                {
                    var newfeed = new NewFeedsViewModel();
                    newfeed.UserId = currentUser.AccountId;
                    newfeed.AllowCreateQrCode = item.AllowCreateQrCode;
                    newfeed.PublicURL = item.PublicURL;

                    newfeed.CampaignId = item.Id;
                    newfeed.CampaignType = item.CampaignType;
                    newfeed.Name = item.CampaignName;
                    newfeed.Description = item.Description;
                    newfeed.termsAndConditionsFile = item.termsAndConditionsFile;
                    //Event
                    newfeed.starttime = item.starttime;
                    newfeed.startdate = item.startdate;
                    newfeed.endtime = item.endtime;
                    newfeed.enddate = item.enddate;
                    newfeed.location = item.location;
                    newfeed.theme = item.theme;
                    newfeed.usercodecurrentcy = item.usercodecurrentcy;
                    newfeed.usercodetype = item.usercodetype;
                    newfeed.usercode = item.usercode;
                    newfeed.BusinessUserId = item.BusinessUserId;
                    var businessItem = _accountService.GetByAccountId(item.BusinessUserId);
                    newfeed.BusinessUserobjectId = businessItem.Id.ToString();
                    if (businessItem != null)
                    {
                        newfeed.BusinessName = businessItem.Profile.DisplayName;
                        newfeed.UserName = businessItem.Profile.DisplayName;
                        newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                        var membersOfCampaignList = new PostBusinessLogic().GetFollowersList(item.Id);
                        if (membersOfCampaignList.DataOfCurrentPage != null)
                        {
                            newfeed.MembersOfBusiness = membersOfCampaignList.DataOfCurrentPage.ToList();
                            newfeed.MembersOfBusinessNbr = newfeed.MembersOfBusiness.Count;
                        }

                        newfeed.MembersOfBusinessNbr = membersOfCampaignList.TotalItems;
                    }

                    newfeed.MaxAge = item.MaxAge;
                    newfeed.MinAge = item.MinAge;
                    newfeed.Gender = item.Gender;

                    newfeed.LocationType = item.LocationType;
                    newfeed.CountryName = item.CountryName;
                    newfeed.CityName = item.CityName;

                    newfeed.TargetNetwork = item.TargetNetwork;

                    newfeed.SpendMoney = item.SpendMoney;
                    newfeed.SpendEffectDate = item.SpendEffectDate;
                    newfeed.SpendEndDate = item.SpendEndDate;

                    newfeed.ResidenceStatus = item.ResidenceStatus;
                    newfeed.EstimatedReach = item.EstimatedReach;
                    newfeed.Image = item.Image;
                    newfeed.TargetLink = item.TargetLink;
                    newfeed.Image = item.Image;

                    newfeed.FollowerIds = item.FollowerIds;

                    newfeed.BusinessList2 = businessList;
                    newFeedList.Add(newfeed);
                }

                strbusinessList = businessList.Select(item => item.Id).ToList();
                strcampaignIdList = postList.Select(item => item.CampaignId).ToList();
                strBusinessCampaignIdList = businessCampaignList.Select(item => item.UserId).ToList();
            }
            else
            {
                accounttype = AccountType.Business;
            }

            return new BusinessProfileViewModel
            {
                AccountType = accounttype,
                Id = businessId,
                BusId = businessAccount.AccountId,
                Avatar = businessAccount.Profile.PhotoUrl,
                CanFollow = currentUser.Id != businessAccount.Id,
                Followed = followed,
                NumberOfFollowers = businessAccount.Followers.Count,
                Street = businessAccount.Profile.Street,
                City = businessAccount.Profile.City,
                Region = businessAccount.Profile.Region,
                Country = businessAccount.Profile.Country,
                ZipPostalCode = businessAccount.Profile.ZipPostalCode,
                Description = businessAccount.Profile.Description,
                Email = businessAccount.Profile.Email,
                PhoneNumber = businessAccount.Profile.PhoneNumber,
                PictureAlbum = businessAccount.PictureAlbum,
                Website = businessAccount.CompanyDetails.Website,
                BusinessPrivacies = businessAccount.BusinessPrivacies,
                DisplayName = businessAccount.Profile.DisplayName,
                ListCampaignSRFI = campaignListSRFI,
                ListCampaignWithoutSRFI = newFeedList,
                BusinessIdList = strbusinessList,
                CampaignIdInPostList = strcampaignIdList,
                BusinessCampaignIdList = strBusinessCampaignIdList
            };
        }

        [HttpGet, Route("GetBusinessFollowers")]
        public async Task<List<MemberInBusiness>> GetBusinessFollowers(string businessId = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (!string.IsNullOrEmpty(businessId))
                currentUser = _accountService.GetByAccountId(businessId);
            var lists = new List<MemberInBusiness>();
            foreach (var follow in currentUser.Followers)
            {
                try
                {
                    var userfollow = _accountService.GetById(follow.AccountId);
                    if (userfollow != null)
                    {
                        if (!lists.Any(x => x.DisplayName == userfollow.Profile.DisplayName))
                        {
                            lists.Add(new MemberInBusiness
                            {
                                AccountId = userfollow.Id,
                                Avatar = userfollow.Profile.PhotoUrl,
                                DisplayName = userfollow.Profile.DisplayName,
                                UserId = userfollow.AccountId
                            });
                        }
                    }
                }
                catch { }
            }

            return lists;
        }


        [HttpGet, Route("PublicProfileFull")]
        public async Task<BusinessProfileFullViewModel> GetBusinessPublicProfileFull(string businessId = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (string.IsNullOrEmpty(businessId))
            {
                if (currentUser.AccountType == AccountType.Business)
                {
                    businessId = currentUser.Id.ToString();
                }
                else
                {
                    throw new CustomException("Business profile does not exist");
                }
            }

            ObjectId busObjId = new ObjectId();
            if (!ObjectId.TryParse(businessId, out busObjId))
            {
                throw new CustomException("Business profile does not exist");
            }

            var businessAccount = _accountService.GetById(busObjId);

            if (businessAccount == null)
            {
                throw new CustomException("Business profile does not exist");
            }

            if (businessAccount.AccountType == AccountType.Personal)
            {
                throw new CustomException("Business profile does not exist");
            }


            bool followed = false;
            AccountType accounttype = AccountType.Personal;
            if (currentUser.AccountType != AccountType.Business)
            {
                followed = currentUser.Followees.Any(f => f.AccountId == businessAccount.Id);
            }
            else
                accounttype = AccountType.Business;

            string street = "";
            string country = "";
            string city = "";
            string phone = "";
            string email = "";

            if (!string.IsNullOrEmpty(businessAccount.CompanyDetails.Street))
                street = businessAccount.CompanyDetails.Street;
            if (!string.IsNullOrEmpty(businessAccount.CompanyDetails.Country))
                country = businessAccount.CompanyDetails.Country;
            if (!string.IsNullOrEmpty(businessAccount.CompanyDetails.City))
                city = businessAccount.CompanyDetails.City;


            if (!string.IsNullOrEmpty(businessAccount.CompanyDetails.Phone))
                phone = businessAccount.CompanyDetails.Phone;

            if (!string.IsNullOrEmpty(businessAccount.CompanyDetails.Phone))
                email = businessAccount.CompanyDetails.Email;

            return new BusinessProfileFullViewModel
            {
                AccountType = accounttype,
                Id = businessId,
                BusId = businessAccount.AccountId,
                Avatar = businessAccount.Profile.PhotoUrl,
                CanFollow = currentUser.Id != businessAccount.Id,
                Followed = followed,
                NumberOfFollowers = businessAccount.Followers.Count,
                Street = businessAccount.Profile.Street,
                City = businessAccount.Profile.City,
                Region = businessAccount.Profile.Region,
                Country = businessAccount.Profile.Country,
                ZipPostalCode = businessAccount.Profile.ZipPostalCode,
                Description = businessAccount.Profile.Description,
                Email = businessAccount.Profile.Email,
                PhoneNumber = businessAccount.Profile.PhoneNumber,
                PictureAlbum = businessAccount.PictureAlbum,

                BusinessPrivacies = businessAccount.BusinessPrivacies,
                DisplayName = businessAccount.Profile.DisplayName,
                Website = businessAccount.CompanyDetails.Website,
                WorkHourFrom = businessAccount.CompanyDetails.WorkHourFrom,
                WorkHourTo = businessAccount.CompanyDetails.WorkHourTo,
                Workdays = businessAccount.CompanyDetails.Workdays,

                StreetCompany = street,
                CityCompany = city,
                CountryCompany = country,
                EmailCompany = email,
                PhoneCompany = phone
            };
        }


        [HttpPost, Route("Follow")]
        public async Task<FollowersSummary> FollowBusinesss(FollowModel model)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (model == null || string.IsNullOrEmpty(model.Id))
            {
                throw new CustomException("Business profile does not exist");
            }

            ObjectId busObjId = new ObjectId();
            if (!ObjectId.TryParse(model.Id, out busObjId))
            {
                throw new CustomException("Business profile does not exist");
            }

            var businessAccount = _accountService.FollowBusiness(currentUser.Id, busObjId);
            new BusinessMemberLogic().AddBusinessMember(currentUser, businessAccount);
            _transactionService.InsertTransaction(new FollowTransactionParamter
            {
                FromUser = currentUser.Id,
                ToUser = busObjId,
                User = currentUser,
                Date = DateTime.Now.Date,
                Type = FollowType.Follow
            });

            return new FollowersSummary
            {
                Followed = true,
                NumberOfFollowers = businessAccount.Followers.Count
            };
        }

        [HttpPost, Route("Unfollow")]
        public async Task<FollowersSummary> UnfollowBusinesss(FollowModel model)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (model == null || string.IsNullOrEmpty(model.Id))
            {
                throw new CustomException("Business profile does not exist");
            }

            ObjectId busObjId = new ObjectId();
            if (!ObjectId.TryParse(model.Id, out busObjId))
            {
                throw new CustomException("Business profile does not exist");
            }

            var businessAccount = _accountService.UnfollowBusiness(currentUser.Id, busObjId);

            new BusinessMemberLogic().RemoveBusinessMember(currentUser, businessAccount);
            _transactionService.InsertTransaction(new FollowTransactionParamter
            {
                FromUser = currentUser.Id,
                ToUser = busObjId,
                User = currentUser,
                Date = DateTime.Now.Date,
                Type = FollowType.UnFollow
            });

            return new FollowersSummary
            {
                Followed = false,
                NumberOfFollowers = businessAccount.Followers.Count
            };
        }

        [HttpGet, Route("BusinessAccountProfile")]
        public async Task<dynamic> GetBusinessAccountProfile()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (busUser.AccountType == AccountType.Personal)
            {
                try
                {
                    return AccountAdapter.ConvertToViewModel(_accountService.GetById(busUser.BusinessAccountRoles
                        .FirstOrDefault().AccountId));
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return AccountAdapter.ConvertToViewModel(busUser);
            }
        }

        [HttpGet, Route("SummarizeNumberOfFollowersByTime")]
        public async Task<FollowersByTimeStatisticResult> SummarizeNumberOfFollowersByTime(
            [FromUri] GetFollowersByTimeCriteria criteria)
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            if (criteria == null)
            {
                criteria = new GetFollowersByTimeCriteria();
            }

            IBusinessAccountStatisticsService statService = new BusinessAccountStatisticsService();

            var virtualData = new FollowersByTimeStatisticResult();
            var now = DateTime.Now;


            return statService.SummarizeFollowersByTime(busUser.Id, criteria.FromDate, criteria.ToDate);
        }

        [HttpGet, Route("SummarizeNumberOfFollowersByGenders")]
        public async Task<List<FollowersByGenderResult>> SummarizeNumberOfFollowersByGenders()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            IBusinessAccountStatisticsService statService = new BusinessAccountStatisticsService();
            var ageRangeConfigPath = CommonFunctions.MapPath("~/Areas/User/Configs/AgeRangeForStats.json");
            var ageRanges = JArray.Parse(File.ReadAllText(ageRangeConfigPath)).ToObject<List<AgeRange>>();

            var virtualData = new List<FollowersByGenderResult>();
            var now = DateTime.Now;

            return statService.SummarizeFollowersByGender(busUser.Id, ageRanges);
        }

        [HttpGet, Route("SummarizeNumberOfFollowersByCountries")]
        public async Task<List<FollowersByCountryResult>> SummarizeNumberOfFollowersByCountries()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            IBusinessAccountStatisticsService statService = new BusinessAccountStatisticsService();

            return statService.SummarizeFollowersByCountry(busUser.Id);
        }

        [HttpGet, Route("SummarizeNumberOfFollowersByCities")]
        public async Task<List<FollowersByCityResult>> SummarizeNumberOfFollowersByCities()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            IBusinessAccountStatisticsService statService = new BusinessAccountStatisticsService();


            return statService.SummarizeFollowersByCity(busUser.Id);
        }

        [HttpGet, Route("RolesOfCurrentUser")]
        public async Task<List<string>> GetRolesOfCurrentUser()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (currentUser.AccountType == AccountType.Business)
            {
                return _roleService.GetAllRoles().Select(r => r.Name).ToList();
            }
            else
            {
                if (currentUser.BusinessAccountRoles.Any())
                {
                    return _roleService.GetRolesOfAccount(currentUser,
                        currentUser.BusinessAccountRoles.FirstOrDefault().AccountId).Select(r => r.Name).ToList();
                }
                else
                {
                    throw new CustomException("User does not belong to any business");
                }
            }
        }

        [HttpGet, Route("WebsiteUrl")]
        public async Task<string> GetWebsiteUrl()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var busId = busUser.BusinessAccountRoles.FirstOrDefault();
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            return busUser.CompanyDetails.Website;
        }


        [HttpPost, Route("WebsiteUrl")]
        public async Task UpdateWebsiteUrl(UpdateBusinessAccountWebsiteModel model)
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            _accountService.UpdateWebsite(model.Website, busUser.Id);
        }


        [HttpGet, Route("PhoneCompany")]
        public async Task<string> PhoneCompany()
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var busId = busUser.BusinessAccountRoles.FirstOrDefault();
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            return busUser.CompanyDetails.Website;
        }


        [HttpPost, Route("PhoneCompany")]
        public async Task PhoneCompany(UpdateBusinessAccountWebsiteModel model)
        {
            var busUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            if (busUser.AccountType == AccountType.Personal)
            {
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                var busId = busUser.BusinessAccountRoles.FirstOrDefault(r => r.RoleId == adminRole.Id);
                if (busId != null)
                {
                    busUser = _accountService.GetById(busId.AccountId);
                }
                else
                {
                    throw new CustomException("Invalid request");
                }
            }

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            _accountService.UpdateWebsite(model.Website, busUser.Id);
        }

        //
        [HttpGet, Route("CheckSocialNetworks")]
        public async Task<CheckSocialNetworkViewModel> CheckSocialNetworks()
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (account.AccountType != AccountType.Business)
            {
                if (!account.BusinessAccountRoles.Any())
                {
                    throw new CustomException("Permission denied");
                }

                account = _accountService.GetById(account.BusinessAccountRoles.First().AccountId);
            }

            return new CheckSocialNetworkViewModel
            {
                Facebook = account.AccountLinks.Any(l => l.Type == SocialType.Facebook),
                Twitter = account.AccountLinks.Any(l => l.Type == SocialType.Twitter),
                FacebookPage = account.Pages != null && account.Pages.Any(l => l.SocialType == SocialType.Facebook)
            };
        }


        [HttpGet, Route("GetJsonCompanyDetails")]
        public HttpResponseMessage GetJsonCompanyDetails(HttpRequestMessage request, string businessId = null)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(businessId))
            {
                currentUserAccountId = businessId;
            }

            string jsonCompanyDetails = "";

            try
            {
                jsonCompanyDetails = _accountService.GetCompanyDetailsByAccountId(currentUserAccountId);
            }
            catch
            {
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK,
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsonCompanyDetails));
            return response;
        }

        [HttpPost, Route("SaveJsonCompanyDetails")]
        public HttpResponseMessage SaveJsonCompanyDetails(HttpRequestMessage request, CompanyViewModel companyDetails)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(companyDetails.BusinessId))
                currentUserAccountId = companyDetails.BusinessId;
            try
            {
                _accountService.UpdateCompanyDetailsByAccountId(currentUserAccountId, companyDetails.BsonString);
            }
            catch
            {
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, companyDetails);
            return response;
        }

        #region Subcription 

        [HttpPost, Route("UpgradePlan")]
        public HttpResponseMessage UpgradePlan(HttpRequestMessage request, SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var billingLogic = new BillingLogic();
                var subcriptionLogic = new SubcriptionLogic();
                var paymentCardLogic = new PaymentCardLogic();

                if (subcriptionViewModel.PaymentPlanName == "free")
                {
                    subcriptionLogic.SetFreeSubcriptionByAccountId(account.AccountId);
                    goto Result;
                }

                var billing = billingLogic.GetCurrentBilling(account.AccountId);

                //
                if (!string.IsNullOrEmpty(subcriptionViewModel.PromoCode))
                {
                    PromoCode promoCode = new PromoCodeLogic().GetPromoCodeByCode(subcriptionViewModel.PromoCode);
                    if (promoCode != null &&
                        (promoCode.NumberReUse == 0 ||
                         billingLogic.CountReUsePromoCode(promoCode.Code) < promoCode.NumberReUse)
                        && !billingLogic.isUsePromocodeBus(promoCode.Code, account.AccountId)
                    )
                    {
                        var plan = subcriptionLogic.GetPlanFromName(promoCode.Type);
                        var planName = promoCode.Type.First().ToString().ToUpper() + promoCode.Type.Substring(1);
                        var strdescriptionplan = planName + " Plan Promotion Discount Subscription";
                        var userid = account.AccountId;
                        var subcription = subcriptionLogic.GetSubcriptionByUserId(userid);
                        if (billing != null)
                            billingLogic.UpdateBillingCurrent(billing.Id);
                        if (string.IsNullOrEmpty(subcriptionViewModel.NumberMonthExpired))
                            subcriptionViewModel.NumberMonthExpired = "1";
                        var isPending = string.IsNullOrEmpty(billing?.promocode);
                        billingLogic.InsertBilling(userid, subcription["_id"].AsString, "new_sub_promo_" + userid, strdescriptionplan,
                            "Subscribed", isPending ? (billing?.dateend ?? DateTime.Now) : DateTime.Now,
                            "0", "PromoCode",
                            promoCode.Code, promoCode.NumberMonthExpired);
                        new PromoCodeLogic().UpdatePromocode(promoCode.Code);
                        subcriptionLogic.UpgradeSubcriptionWithPromoCodebyUserId(account.AccountId, promoCode.Type);
                    }
                    else
                    {
                        subcriptionViewModel.ReturnStatus = false;
                        subcriptionViewModel.ReturnMessage = new string[] {"Invalid promo code"}.ToList();
                        goto Result;
                    }
                }
                else
                {
                    try
                    {
                        var planname = subcriptionViewModel.PaymentPlanName;
                        var subcription = subcriptionLogic.GetSubcriptionByUserId(account.AccountId);
                        var subscriptionid = subcription["_id"].AsString;
                        var plan = subcriptionLogic.GetPlanFromName(planname);
                        var userid = account.AccountId;
                        var planName = plan.PlanName.First().ToString().ToUpper() + plan.PlanName.Substring(1);
                        var strdescriptionplan = planName + " Plan Monthly Subscription";
                        var paymentCard = paymentCardLogic.GetPaymentCardDefaultByUserId(userid);
                        if (paymentCard != null || planname== "enterprise")
                        {
                            var toCharge = plan.PlanName != "enterprise" && (billing == null
                                           || billing.productname.Contains("Free") || !string.IsNullOrEmpty(billing.promocode));

                            if (toCharge)
                            {
                                StripeCharge stripeCharge =
                                    new StripePaymentLogic().ProcessPaymentCreditCard("usd", plan.Price,
                                        strdescriptionplan, paymentCard);
                                if (stripeCharge != null && stripeCharge.Status == "succeeded")
                                {
                                    var method = paymentCard.cardtype + "**" +
                                                 paymentCard.cardnumber.Substring(
                                                     Math.Max(0, paymentCard.cardnumber.Length - 4));

                                    var transactionid = stripeCharge.Id;
                                    subcriptionLogic.UpgradeSubcriptionbyUserId(account.AccountId,
                                        subcriptionViewModel.PaymentPlanName);
                                    new SubcriptionLogic().UpdateSubcriptionFirstTime(subscriptionid);
                                    if (billing != null)
                                        billingLogic.UpdateBillingCurrent(billing.Id);
                                    new BillingLogic().InsertBilling(account.AccountId, subscriptionid,
                                        transactionid,
                                        strdescriptionplan, "Paid", billing?.dateend ?? DateTime.Now,
                                        plan.Price.ToString(), method);


                                    var notificationMessage = new NotificationMessage();
                                    notificationMessage.Id = ObjectId.GenerateNewId();
                                    notificationMessage.Type = EnumNotificationType.NotifyBillingFirst;
                                    notificationMessage.ToAccountId = account.AccountId;
                                    notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                                    var notificationBus = new NotificationBusinessLogic();
                                    notificationBus.SendNotification(notificationMessage);
                                    
                                    var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                                    var callbackLink = $"{baseUrl}/BusinessAccount/Billing";
                                    var subject = $"Regit payment: Charge successful for new subscription plan";

                                    var emailModel = new PaymentEmailViewModel {
                                        subject = subject,
                                        toName = account.Profile.DisplayName,
                                        backLink = callbackLink,
                                        productName = strdescriptionplan,
                                        amount = "USD " + plan.Price.ToString(),
                                        method = method,
                                        transactionId = transactionid,
                                        nextCharge = (billing?.dateend ?? DateTime.Now).AddMonths(1).ToString("dd MMMM yyyy")
                                    };

                                    string emailContent =
                                        ViewUtils.RenderPartialViewToString("_EmailTemplate_Payment", emailModel);

                                    IMailService mailService = new MailService();
                                    mailService.SendMailAsync(new NotificationContent
                                    {
                                        Title = subject,
                                        Body = string.Format(emailContent, ""),
                                        SendTo = new[] {account.Profile.Email}
                                    });
                                }
                                else
                                {
                                    var notificationMessage = new NotificationMessage();
                                    notificationMessage.Id = ObjectId.GenerateNewId();
                                    notificationMessage.Type = EnumNotificationType.NotifyBillingFailed;
                                    notificationMessage.ToAccountId = account.AccountId;
                                    notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                                    var notificationBus = new NotificationBusinessLogic();
                                    notificationBus.SendNotification(notificationMessage);
                                }
                                
                                
                            }
                            else
                            {
                                subcriptionLogic.UpgradeSubcriptionbyUserId(account.AccountId,
                                    subcriptionViewModel.PaymentPlanName);
                                new SubcriptionLogic().UpdateSubcriptionFirstTime(subscriptionid);
                                if (billing != null)
                                    billingLogic.UpdateBillingCurrent(billing.Id);
                                new BillingLogic().InsertBilling(account.AccountId, subscriptionid,
                                    "new_sub_" + subscriptionid,
                                    strdescriptionplan, "New", billing?.dateend ?? DateTime.Now,
                                    plan.Price.ToString(), "");
                            }
                        }
                        else
                        {
                            subcriptionViewModel.ReturnStatus = false;
                            subcriptionViewModel.ReturnMessage =
                                new string[] {"Please add a payment method"}.ToList();
                            goto Result;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex.StackTrace);
                    }
                }


                subcriptionViewModel.ReturnStatus = true;
                subcriptionViewModel.ReturnMessage = new string[] {"Get successfully"}.ToList();
            }
            catch (Exception ex)
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {ex.ToString() + ex.StackTrace}.ToList();
            }

            Result:
            var response = Request.CreateResponse<SubcriptionViewModel>(HttpStatusCode.OK, subcriptionViewModel);
            return response;
        }


        [HttpPost, Route("GetSubcriptionByUser")]
        public HttpResponseMessage GetSubcriptionByUserId(HttpRequestMessage request,
            SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                subcriptionViewModel.isAdmin = this.User.IsInRole("Admin");
                subcriptionViewModel.AccountId = account.AccountId.ToString();
                subcriptionViewModel.AccountName = account.Profile.DisplayName;
                subcriptionViewModel.AccountAddress = account.Profile.Street;
                BsonDocument result = new SubcriptionLogic().GetSubcriptionByUserId(account.AccountId);
                var strresult = result.ToJson();
                subcriptionViewModel.Subcription =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                result = new SubcriptionLogic().GetSubcriptionPlanTemplate();
                strresult = result.ToJson();
                subcriptionViewModel.SubcriptionPlans =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                var list = new PaymentCardLogic().GetListPaymentCard(account.AccountId);

                subcriptionViewModel.ListPaymentCard = list;

                subcriptionViewModel.ListBilling =
                    new BillingLogic().GetListBillingFromUserId(account.AccountId.ToString());
                subcriptionViewModel.ReturnStatus = true;
                subcriptionViewModel.ReturnMessage = new string[] {"Get successfully"}.ToList();

                //subcriptionViewModel.
            }
            catch
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {"fail"}.ToList();
            }

            var response = Request.CreateResponse<SubcriptionViewModel>(HttpStatusCode.OK, subcriptionViewModel);
            return response;
        }

        [HttpPost, Route("InsertPromoCode")]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage InsertPromoCode(HttpRequestMessage request,
            SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                new PromoCodeLogic().InsertPromoCode(subcriptionViewModel.PromoCode, subcriptionViewModel.PromoCodeType,
                    subcriptionViewModel.NumberReUse, subcriptionViewModel.NumberMonthExpired);
                //subcriptionViewModel.
            }
            catch
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {"fail"}.ToList();
            }

            var response = Request.CreateResponse<SubcriptionViewModel>(HttpStatusCode.OK, subcriptionViewModel);
            return response;
        }

        [HttpPost, Route("GetPaymentDetailByUserId")]
        public HttpResponseMessage GetPaymentDetailByUserId(HttpRequestMessage request,
            PaymentViewModel paymentViewModel)
        {
            if (paymentViewModel == null)
                paymentViewModel = new PaymentViewModel();
            try
            {
                SubcriptionLogic subcriptionLogic = new SubcriptionLogic();
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

                var planname = "Free Tier";
                var currentbilling = new BillingLogic().GetCurrentBilling(account.AccountId);
                if (currentbilling != null)
                {
                    if (currentbilling.dateend <= DateTime.Now)
                    {
                        var subscriptioncurrent = subcriptionLogic.GetSubcriptionByUserId(account.AccountId);
                        if (subscriptioncurrent["subcription"]["CurrentPlan"].ToString() == "free")
                        {
                            planname = "Free Tier";
                        }
                        else
                        {
                            paymentViewModel.ReturnStatus = false;
                            paymentViewModel.ReturnMessage = new string[] {"-2"}.ToList();
                            goto Results;
                        }
                    }
                    else
                        planname = currentbilling.productname;
                }

                PaymentPlanDetail plan = null;
                paymentViewModel.PaymentPlanDetailInteraction.currentnumber =
                    new CampaignBusinessLogic().CountInteractionsUserId(account.AccountId.ToString());
                paymentViewModel.PaymentPlanDetailSyncForm.currentnumber =
                    new PostHandShakeBusinessLogic().CountUserInvitedHandshake(account.AccountId.ToString());
                paymentViewModel.PaymentPlanDetailWorkFlow.currentnumber =
                    _accountService.CountMembersInBusiness(account.Id);
                var planName = "Free";
                var productName = planname.ToLower();
                switch (planname)
                {
                    case "Free Tier":
                        plan = subcriptionLogic.GetPlanFromName("free");
                        paymentViewModel.PaymentPlanName = "free";
                        paymentViewModel.PaymentPlanDesc = "Free Tier";
                        break;
                    default:
                        if (productName.Contains("lite"))
                            planName = "Lite";
                        if (productName.Contains("medium"))
                            planName = "Medium";
                        if (productName.Contains("heavy"))
                            planName = "Heavy";
                        if (productName.Contains("enterprise"))
                            planName = "Enterprise";
                        plan = subcriptionLogic.GetPlanFromName(planName.ToLower());
                        paymentViewModel.PaymentPlanName = planName.ToLower();
                        paymentViewModel.PaymentPlanDesc = planName + " Tier";
                        break;
                }

                int pending = 0;
                try
                {
                    var _joinBusService = new JoiningBusinessInvitationService();
                    var lst = _joinBusService.GetAllWorkflowInvitationsFromBusiness(account.Id);
                    pending = lst.Count();
                }
                catch { }

                paymentViewModel.PaymentPlanDetailWorkFlow.currentmaxnumber = plan.BusinessUsers=="unlimited" ? int.MaxValue :
                    ConvertHelper.ConvertToLong(plan.BusinessUsers) + pending;
                paymentViewModel.PaymentPlanDetailSyncForm.currentmaxnumber = plan.SyncRelationships=="unlimited" ? int.MaxValue :
                    ConvertHelper.ConvertToLong(plan.SyncRelationships);
                paymentViewModel.PaymentPlanDetailInteraction.currentmaxnumber =plan.InteractionActives=="unlimited" ? int.MaxValue :
                    ConvertHelper.ConvertToLong(plan.InteractionActives);
            }
            catch (Exception ex)
            {
                paymentViewModel.ReturnStatus = false;
                paymentViewModel.ReturnMessage = new string[] {"-1"}.ToList();
            }

            Results:
            var response = Request.CreateResponse<PaymentViewModel>(HttpStatusCode.OK, paymentViewModel);
            return response;
        }

        #endregion

        #region PaymentCard 

        [HttpPost, Route("GetPaymentCardsByUserId")]
        public IHttpActionResult GetPaymentCardsByUserId(SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                var list = new PaymentCardLogic().GetListPaymentCard(subcriptionViewModel.UserId);

                subcriptionViewModel.ListPaymentCard = list;

                subcriptionViewModel.ReturnStatus = true;
                subcriptionViewModel.ReturnMessage = new string[] {"Get successfully"}.ToList();
            }
            catch
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {"fail"}.ToList();
            }

            return Ok(subcriptionViewModel);
        }


        [HttpPost, Route("DeletePaymentCard")]
        public IHttpActionResult DeletePaymentCard(SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                new PaymentCardLogic().DeletePaymentCard(subcriptionViewModel.PaymentCardId);
                subcriptionViewModel.ReturnStatus = true;
                subcriptionViewModel.ReturnMessage = new string[] {"Get successfully"}.ToList();
            }
            catch (Exception ex)
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {ex.Message}.ToList();
            }

            return Ok(subcriptionViewModel);
        }


        [HttpPost, Route("SavePaymentCard")]
        public IHttpActionResult SavePaymentCard(SubcriptionViewModel subcriptionViewModel)
        {
            if (subcriptionViewModel == null)
                subcriptionViewModel = new SubcriptionViewModel();
            try
            {
                //verify card from paypal
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var ccService = new CreditCardLogic(PayPalConfig.GetAPIContext());
                var validatedCard = ccService.CreateCreditCard(subcriptionViewModel.CardType,
                    subcriptionViewModel.CardNumber,
                    subcriptionViewModel.CardSecurity,
                    ConvertHelper.ConvertToInt(subcriptionViewModel.CardExpiredMonth),
                    ConvertHelper.ConvertToInt(subcriptionViewModel.CardExpiredYear));

                if (validatedCard == null || string.IsNullOrEmpty(validatedCard.id))
                {
                    subcriptionViewModel.ReturnStatus = false;
                    subcriptionViewModel.ReturnMessage = new string[] {"Invalid Card"}.ToList();
                }

                if (!string.IsNullOrEmpty(subcriptionViewModel.PaymentCardId))
                {
                    new PaymentCardLogic().DeletePaymentCard(subcriptionViewModel.PaymentCardId);
                }

                var card = new PaymentCardLogic().InsertPaymentCard(account.AccountId, subcriptionViewModel.CardName,
                    subcriptionViewModel.CardType, subcriptionViewModel.CardNumber,
                    subcriptionViewModel.CardExpiredMonth,
                    subcriptionViewModel.CardExpiredYear, subcriptionViewModel.IsDefault,
                    subcriptionViewModel.CardSecurity, validatedCard.id
                );
                subcriptionViewModel.ReturnStatus = true;
                subcriptionViewModel.ReturnMessage = new string[] {"Get successfully"}.ToList();
            }
            catch (Exception ex)
            {
                subcriptionViewModel.ReturnStatus = false;
                subcriptionViewModel.ReturnMessage = new string[] {ex.Message}.ToList();
            }

            return Ok(subcriptionViewModel);
        }

        #endregion

        #region HandshakeRequest

        [HttpPost, Route("requesthandshake/update")]
        public IHttpActionResult UpdateRequestHandshake(HttpRequestMessage request, RequestViewModel requestHandShake)
        {
            if (requestHandShake == null)
                return BadRequest();

            if (string.IsNullOrEmpty(requestHandShake.Type))
                requestHandShake.Type = EnumRequest.RequestHandshake;
            var rq = new Request();
            rq = RequestAdapter.RequestViewModelToRequest(requestHandShake);
            var requestBus = new RequestBusinessLogic();
            requestBus.Update(rq);

            return Ok(rq);
        }

        [HttpPost, Route("requesthandshake/send")]
        public IHttpActionResult SendRequestHandshake(RequestViewModel userRequest)
        {
            var sendEmailAdmin = ConfigurationManager.AppSettings["IsSendEmailTemporaryBusiness"];

            var fromUser = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userRequest.FromUserId))
                userRequest.FromUserId = fromUser;
            userRequest.CreatedDate = DateTime.UtcNow;

            var rq = new Request();
            rq = RequestAdapter.RequestViewModelToRequest(userRequest);
            var requestBus = new RequestBusinessLogic();
            var requestId = requestBus.Insert(rq);

            var postHandShake = new PostHandShakeBusinessLogic();
            var fromAccount = _accountService.GetByAccountId(userRequest.FromUserId);
            if (userRequest.Type != EnumRequest.UserCreatedBusinessType)
            {
                userRequest.Type = EnumRequest.RequestHandshake;

                var toAccount = _accountService.GetByAccountId(userRequest.ToUserId);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyRequestdHandshake;
                notificationMessage.FromAccountId = userRequest.FromUserId;
                notificationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = toAccount.AccountId;
                notificationMessage.ToUserDisplayName = toAccount.Profile.DisplayName;
                notificationMessage.Content = userRequest.Message;
                notificationMessage.PreserveBag = requestId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);

                postHandShake.ExportToSendMailHandShakeRequest(requestId);
            }
            else
            {
                //SyncListMailHandShakeRequest EmailAdmin
                var emailAdmin = "";
                var toEmail = "";
                if (sendEmailAdmin == "true")
                    emailAdmin = ConfigurationManager.AppSettings["EmailAdmin"];

                if (!string.IsNullOrEmpty(rq.Email))
                {
                    toEmail = userRequest.ToEmail;
                }

                postHandShake.SyncListMailHandShakeRequestUcb(emailAdmin, toEmail, fromAccount.Profile.DisplayName,
                    userRequest.ToDisplayName);
            }


            return Ok(userRequest);
        }

        [HttpGet, Route("requesthandshake/business")]
        public IHttpActionResult GetHandshakeRequestByBusiness(string toUserId = null)
        {
            if (string.IsNullOrEmpty(toUserId))
            {
                toUserId = HttpContext.Current.User.Identity.GetUserId();
            }

            var rs = new List<Request>();
            var requestBus = new RequestBusinessLogic();
            rs = requestBus.GetListByToUserId(toUserId);
            return Ok(rs);
        }

        [HttpGet, Route("requesthandshakevalid/business")]
        public IHttpActionResult GetHandshakeRequestValidByBusiness(string toUserId = null)
        {
            if (string.IsNullOrEmpty(toUserId))
            {
                toUserId = HttpContext.Current.User.Identity.GetUserId();
            }

            var rs = new List<Request>();
            var requestBus = new RequestBusinessLogic();
            rs = requestBus.GetListByToUserIdStatus(toUserId);
            return Ok(rs);
        }

        [AllowAnonymous]
        [HttpGet, Route("requesthandshake/emailnotification")]
        public IHttpActionResult GetListHandShakeRequestOutsite(string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
                userId = HttpContext.Current.User.Identity.GetUserId();

            var result = new OutsiteViewModel();
            OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();

            var type = EnumNotificationType.NotifyEmailHandshakeRequest;
            var outsite = _outsiteBusinessLogic.GetOutsiteByUserId(userId, type);

            if (outsite != null)
                result = OutsiteAdapter.OutsiteToOutsiteViewModel(outsite);
            else
            {
                var acc = _accountService.GetByAccountId(userId);
                result.Email = acc.Profile.Email;
            }

            return Ok(result);
        }

        #endregion HandshakeRequest

        [Route("getotp")]
        [HttpGet]
        public IHttpActionResult getOtp(string userId = null)
        {
            var otp = new OtpViewModel();
            if (userId == null)
                userId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(userId);
            otp.UserId = userId;
            otp.Value = account.PhoneVerifiedToken;
            if (string.IsNullOrEmpty(otp.Value))
                otp.Value = EnumAccount.otpDisable;

            return Ok(otp);
        }

        [Route("setotp")]
        [HttpPost]
        public IHttpActionResult setOtp(OtpViewModel otp)
        {
            if (otp.UserId == null)
                otp.UserId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(otp.UserId);
            _accountService.UpdateOtp(otp.Value, account.Id);
            var rs = account.PhoneVerifiedToken;
            if (string.IsNullOrEmpty(rs))
                rs = EnumAccount.otpDisable;
            return Ok(rs);
        }
    }
}