using GH.Core.Exceptions;
using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.Services;
using GH.Core.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GH.Core.Adapters;
using GH.Web.Areas.User.ViewModels;
using System.Text;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.Extensions;
using GH.Core.IServices;
using GH.Web.Extensions;
using MongoDB.Bson;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using Newtonsoft.Json.Linq;
using System.IO;
using GH.Lang;
using GH.Core.BlueCode.Entity.FeedBack;
using System.Collections.Generic;
using System.Web.Hosting;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [ApiAuthorize]
    [RoutePrefix("Api/AccountSettings")]
    public class AccountSettingsApiController : BaseApiController
    {
        IAccountService _accountService;
        ITransactionService _transactionService;
        public AccountSettingsApiController()
        {
            _accountService = new AccountService();
            _transactionService = new TransactionService();
        }

        [HttpPost, Route("Profile")]
        public async Task<AccountViewModel> UpdateProfile()
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
                string displayName = provider.FormData["DisplayName"];
                string status = provider.FormData["Status"];
                string city = provider.FormData["City"];
                string country = provider.FormData["Country"];
                string gender = provider.FormData["Gender"];
                string birthDate = provider.FormData["Birthdate"];
                string region = provider.FormData["Region"];
                string street = provider.FormData["Street"];
                string zipPostalCode = provider.FormData["ZipPostalCode"];
                string description = provider.FormData["Description"];
                string forAccount = provider.FormData["ForAccount"];

                string[] updateFields;
                string[] allowFields;
                DateTime birthdate = new DateTime();
                if (string.IsNullOrEmpty(provider.FormData["UpdateFields"]))
                {
                    throw new CustomException(new ErrorViewModel {Message = "Missing Update Fields"});
                }
                else
                {
                    updateFields = provider.FormData["UpdateFields"].Split(',');
                    allowFields = new string[]
                    {
                        "DisplayName", "Status", "City", "Country", "PhotoUrl", "Gender", "Birthdate", "Region",
                        "Street", "ZipPostalCode", "Description"
                    };
                    if (updateFields.Any(f => !allowFields.Contains(f, StringComparer.CurrentCultureIgnoreCase)))
                    {
                        throw new CustomException(new ErrorViewModel {Message = Regit.Invalid_Request_Message});
                    }

                    if (updateFields.Contains("DisplayName"))
                    {
                        if (string.IsNullOrEmpty(displayName))
                        {
                            throw new CustomException(new ErrorViewModel {Message = Regit.Display_Name_Error_Required});
                        }
                        else if (displayName.Length > 128)
                        {
                            throw new CustomException(
                                new ErrorViewModel {Message = Regit.EditProfileSettings_DisplayName_MaxLength});
                        }
                    }

                    if (updateFields.Contains("Status") && !string.IsNullOrEmpty(status) && status.Length > 128)
                    {
                        throw new CustomException(
                            new ErrorViewModel {Message = Regit.EditProfileSettings_Status_MaxLength});
                    }

                    if (updateFields.Contains("Gender") && gender != "Male" && gender != "Female")
                    {
                        throw new CustomException(new ErrorViewModel {Message = Regit.Invalid_Gender_Message});
                    }

                    if (updateFields.Contains("Birthdate"))
                    {
                        try
                        {

                            birthdate = DateTime.Parse(birthDate);
                        }
                        catch (Exception)
                        {
                            throw new CustomException(new ErrorViewModel {Message = Regit.Birthdate_Error_Invalid});
                        }
                    }
                }

                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

                MultipartFileData profilePicture = provider.FileData.FirstOrDefault();

                var existAccount = _accountService.GetByAccountId(currentUserAccountId);

                if (forAccount == "Business")
                {
                    if (existAccount.AccountType == AccountType.Personal)
                    {
                        IRoleService _roleService = new RoleService();
                        var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                        var busAcc = existAccount.BusinessAccountRoles.FirstOrDefault(b => b.RoleId == adminRole.Id);
                        if (busAcc == null)
                        {
                            throw new CustomException(Regit.Permission_Denied_Message);
                        }
                        existAccount = _accountService.GetById(busAcc.AccountId);
                    }
                }

                if (updateFields.Contains("DisplayName"))
                {
                    existAccount.Profile.DisplayName = displayName;
                }

                if (updateFields.Contains("Status"))
                {
                    existAccount.Profile.Status = status;
                }

                if (updateFields.Contains("City"))
                {
                    existAccount.Profile.City = city;
                }

                if (updateFields.Contains("Country"))
                {
                    existAccount.Profile.Country = country;
                }

                if (updateFields.Contains("Gender"))
                {
                    existAccount.Profile.Gender = gender;
                }

                if (updateFields.Contains("Birthdate"))
                {
                    existAccount.Profile.Birthdate = birthdate;
                }

                if (updateFields.Contains("Region"))
                {
                    existAccount.Profile.Region = region;
                }

                if (updateFields.Contains("Street"))
                {
                    existAccount.Profile.Street = street;
                }

                if (updateFields.Contains("ZipPostalCode"))
                {
                    existAccount.Profile.ZipPostalCode = zipPostalCode;
                }

                if (updateFields.Contains("Description"))
                {
                    existAccount.Profile.Description = description;
                }

                existAccount = _accountService.UpdateProfile(existAccount, profilePicture);

                //WRITE LOG
                var account = _accountService.GetByAccountId(currentUserAccountId);
                if (account.AccountActivityLogSettings.RecordProfile)
                {
                    string title = "You updated your profile.";
                    string type = "settings";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
                }
                return AccountAdapter.ConvertToViewModel(existAccount);
            }
            catch (Exception ex)
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


        [HttpGet, Route("Profile")]
        public HttpResponseMessage GetProfile()
        {
            // HttpContext.Current.User.IsInRole("Admin");
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            var accountmodel = AccountAdapter.ConvertToViewModel(account);
            accountmodel.IsAdmin = HttpContext.Current.User.IsInRole("Admin");
            return Request.CreateSuccessResponse(accountmodel,"Current user full profile");;
        }

        [HttpGet, Route("ProfileBusinessFull")]
        public async Task<BusinessAccountViewModel> ProfileBusinessFull()
        {
            var rs = new BusinessAccountViewModel();
            try
            {
                var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
                var account = _accountService.GetByAccountId(currentUserAccountId);

                if (account.AccountType == AccountType.Business)
                    rs.AccountType = "Business";

                rs.Id = account.Id.ToString();
                rs.AccountId = account.AccountId;
                rs.DisplayName = account.Profile.DisplayName;
                rs.FirstName = account.Profile.FirstName;
                rs.LastName = account.Profile.LastName;
                rs.Email = account.Profile.Email;
                rs.Phone = account.Profile.PhoneNumber;
                rs.PhotoUrl = account.Profile.PhotoUrl;
                rs.Status = account.Profile.PhotoUrl;
                rs.WebsiteURL = account.CompanyDetails.Website;
                rs.Country = account.Profile.Country;
                rs.City = account.Profile.City;
                rs.Street = account.Profile.Street;
                rs.Region = account.Profile.Region;
                rs.ZipPostalCode = account.Profile.ZipPostalCode;
                rs.Description = account.Profile.Description;

                if (!string.IsNullOrEmpty(account.CompanyDetails.Street))
                    rs.StreetCompany = account.CompanyDetails.Street;
                if (!string.IsNullOrEmpty(account.CompanyDetails.Country))
                    rs.CountryCompany = account.CompanyDetails.Country;
                if (!string.IsNullOrEmpty(account.CompanyDetails.City))
                    rs.CityCompany = account.CompanyDetails.City;

                if (!string.IsNullOrEmpty(account.CompanyDetails.Phone))
                    rs.PhoneCompany = account.CompanyDetails.Phone;

                if (!string.IsNullOrEmpty(account.CompanyDetails.Phone))
                    rs.EmailCompany = account.CompanyDetails.Email;
            }
            catch
            {
            }

            return rs;
        }


        [HttpGet, Route("ProfileByAccountId")]
        public async Task<AccountViewModel> GetProfileByAccountId(FeedBackEntity feedback)
        {
            // var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(feedback.UserId);
            return AccountAdapter.ConvertToViewModel(account);
        }
        
        [HttpPost, Route("ProfileProperty")]
        public async Task SetProfileProperty(ProfilePropertyViewModel prop)
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(accountId);
            _accountService.UpdateProfileProperty(account, prop.PropName, prop.PropValue);
        }               
        [HttpPost, Route("ViewPreference")]
        public async Task SetViewPreference(ViewPreferenceViewModel pref)
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(accountId);
            _accountService.UpdateViewPreference(account, pref.PrefName, pref.PrefValue);
        }        
        
        [HttpPost, Route("ViewPreferences")]
        public async Task SetViewPreferences(AccountViewPreferences prefs)
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(accountId);
            _accountService.UpdateViewPreferences(account, prefs);
        }


        [HttpGet, Route("Privacies")]
        public async Task<AccountPrivacies> GetPrivacies()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            return account.AccountPrivacies;
        }

        [HttpGet, Route("GetAccountId")]
        public async Task<string> GetAccountId()
        {
            return User.Identity.GetUserId();
        }

        [HttpPost, Route("Privacies")]
        public async Task<AccountPrivacies> UpdatePrivacies(AccountPrivacies model)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.UpdatePrivacies(model, currentUserAccountId);

            //WRITE LOG
            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "You updated privacy.";
                string type = "settings";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return account.AccountPrivacies;
        }

        [HttpGet, Route("ActivityLog")]
        public async Task<AccountActivityLogSettings> GetActivityLogSettings()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            return account.AccountActivityLogSettings;
        }

        [HttpPost, Route("ActivityLog")]
        public async Task<AccountActivityLogSettings> UpdateActivityLogSettings(AccountActivityLogSettings model)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.UpdateAccountActivityLogSettings(model, currentUserAccountId);
            return account.AccountActivityLogSettings;
        }

        [HttpGet, Route("SecurityQuestions")]
        public async Task<AnswerSecurityQuestionModel> GetSecurityQuestionsAnswers()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            return new AnswerSecurityQuestionModel
            {
                Question1 = new AnswerSecurityQuestionViewModel
                {
                    QuestionId = account.SecurityQuesion1 == null
                        ? null
                        : account.SecurityQuesion1.QuestionId.ToString(),
                    Answer = account.SecurityQuesion1 == null ? null : account.SecurityQuesion1.Answer
                },
                Question2 = new AnswerSecurityQuestionViewModel
                {
                    QuestionId = account.SecurityQuesion2 == null
                        ? null
                        : account.SecurityQuesion2.QuestionId.ToString(),
                    Answer = account.SecurityQuesion2 == null ? null : account.SecurityQuesion2.Answer
                },
                Question3 = new AnswerSecurityQuestionViewModel
                {
                    QuestionId = account.SecurityQuesion3 == null
                        ? null
                        : account.SecurityQuesion3.QuestionId.ToString(),
                    Answer = account.SecurityQuesion3 == null ? null : account.SecurityQuesion3.Answer
                }
            };
        }

        [HttpPost, Route("SecurityQuestions")]
        public async Task UpdateSecurityQuestions(AnswerSecurityQuestionModel questions)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.UpdateSecurityQuestions(questions, currentUserAccountId);
            
            

            //WRITE LOG
            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "You updated security questions.";
                string type = "Keep a record of your account setting changes";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }
        }

        [HttpGet, Route("EncodedPhoneNumber")]
        public async Task<string> GetEncodedPhoneNumber()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            if (string.IsNullOrEmpty(account.Profile.PhoneNumber))
            {
                return null;
            }
            else
            {
                return PhoneNumberHelper.EncodePhoneNumber(account.Profile.PhoneNumber);
            }
        }

        [HttpPost, Route("PhoneNumber")]
        public async Task<string> UpdatePhoneNumber(ChangePhoneNumberModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage})).ToArray());
            }
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            var formated = PhoneNumberHelper.GetFormatedPhoneNumber(model.NewPhoneNumber);

            //WRITE LOG 
            account = _accountService.UpdatePhoneNumber(model.NewPhoneNumber, currentUserAccountId);
            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "Changing your phone number was " + account.Profile.PhoneNumber;
                string type = "Keep a record of your account setting changes";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return PhoneNumberHelper.EncodePhoneNumber(account.Profile.PhoneNumber);
        }

        [HttpPost, Route("CheckPhoneNumber")]
        public async Task<string> CheckPhoneNumber(VerifyPhoneNumberModel model)
        {
            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                throw new CustomException("Invalid request");
            }
            else
                // var check = PhoneNumberHelper.CheckPhoneNumber(model.PhoneNumber);
                return PhoneNumberHelper.CheckPhoneNumber(model.PhoneNumber);
        }

        [HttpPost, Route("ValidPhoneNumber")]
        public ValidPhoneViewModel ValidPhoneNumber(VerifyPhoneNumberModel model)
        {
            var rs = new ValidPhoneViewModel();
            rs.ValidPhone = true;
            rs.PhoneNumber = "";
            if (string.IsNullOrEmpty(model.PhoneNumber.ToString()))
                rs.ValidPhone = false;
            else
                rs = PhoneNumberHelper.ValidPhoneNumber(model.PhoneNumber.ToString());

            return rs;
        }

        [HttpPost, Route("GetPhoneCode")]
        public async Task<PhoneViewModel> GetPhoneCode(VerifyPhoneNumberModel model)
        {
            var rs = new PhoneViewModel();
            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                throw new CustomException("Invalid request");
            }
            else
                rs = PhoneNumberHelper.GetPhoneCode(model.PhoneNumber);

            return rs;
        }

        /* PIN Code    public PhoneViewModel GetPhoneCode(string phoneNumber)*/
        [HttpGet, Route("EncodedPinCode")]
        public async Task<string> GetEncodedPinCode()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            if (string.IsNullOrEmpty(account.Profile.PinCode))
            {
                return null;
            }
            else
            {
                return PhoneNumberHelper.EncodePinCode(account.Profile.PinCode);
            }
        }

        [HttpPost, Route("PinCode")]
        public async Task<string> UpdatePinCode(ChangePinCodeModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage})).ToArray());
            }
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            //WRITE PIN CODE
            if (model.NewPinCode.Length > 3 && model.NewPinCode.Length < 7)
            {
                var pinCode = CommonFunctions.CreateSHA512Hash(model.NewPinCode);
                account = _accountService.UpdatePinCode(pinCode, currentUserAccountId);
            }

            else
                throw new CustomException(GH.Lang.Regit.PIN_Error_Format_Incorrect);


            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "Update your PIN code was " + account.Profile.PhoneNumber;
                string type = "Keep a record of your account setting changes";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return PhoneNumberHelper.EncodePinCode(account.Profile.PinCode);
        }

        /* End PIN Code */
        [HttpPost, Route("Business/PhoneNumber")]
        public async Task<string> UpdateBusinessAccountPhoneNumber(ChangePhoneNumberModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage})).ToArray());
            }
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            var formated = PhoneNumberHelper.GetFormatedPhoneNumber(model.NewPhoneNumber);

            if (account.AccountType == AccountType.Personal)
            {
                IRoleService _roleService = new RoleService();
                var adminRole = _roleService.GetRoleByName(Role.ROLE_ADMIN);
                if (!account.BusinessAccountRoles.Any(a => a.RoleId == adminRole.Id))
                {
                    throw new CustomException("Permission deny");
                }

                account = _accountService.GetById(account.BusinessAccountRoles.First().AccountId);
            }

            account = _accountService.UpdatePhoneNumber(model.NewPhoneNumber, currentUserAccountId);

            //WRITE LOG 
            if (account.AccountActivityLogSettings.RecordAccountBusiness)
            {
                string title = "Changing your business phone number was " + account.Profile.PhoneNumber;
                string type = "Keep a record of business account management changes";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return PhoneNumberHelper.EncodePhoneNumber(account.Profile.PhoneNumber);
        }

        [HttpGet, Route("EncodedEmail")]
        public async Task<string> GetEncodedEmail()
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            if (string.IsNullOrEmpty(account.Profile.Email))
            {
                return null;
            }
            else
            {
                return EncodeEmail(account.Profile.Email);
            }
        }

        private string EncodeEmail(string email)
        {
            StringBuilder builder = new StringBuilder();
            var fragments = email.Split('@');
            var fragments2 = fragments[1].Split('.');
            builder.Append(fragments[0].Take(3).ToArray());
            builder.Append(fragments[0].Skip(3).Select(f => '*').ToArray());
            builder.Append('*');
            builder.Append(fragments2[0].Select(f => '*').ToArray());
            builder.Append("." + string.Join(".", fragments2.Skip(1)));
            return builder.ToString();
        }

        [HttpPost, Route("Email")]
        public async Task<string> UpdateEmail(ChangeEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => new ErrorViewModel {Message = e.ErrorMessage})).ToArray());
            }
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);

            var identity = ContextPerRequest.Db.Users.Find(currentUserAccountId);

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            if (string.IsNullOrEmpty(identity.PasswordHash))
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    throw new CustomException(Regit.Incorrect_Password_Message);
                }
            }
            else if (!userManager.CheckPassword(identity, model.Password))
            {
                throw new CustomException(Regit.Incorrect_Password_Message);
            }
            account = _accountService.UpdateEmail(model.Email, account.Id, ContextPerRequest.Db);

            //WRITE LOG 
            if (account.AccountActivityLogSettings.RecordAccount)
            {
                string title = "Changing your email was " + account.Profile.Email;
                string type = "Keep a record of your account setting changes";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(currentUserAccountId, title, type);
            }

            return EncodeEmail(account.Profile.Email);
        }


        [HttpGet, Route("GetShortProfile")]
        public async Task<AccountViewModel> GetShortProfile(string id)
        {
            var account = _accountService.GetById(id.ParseToObjectId());

            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var currentAccount = _accountService.GetByAccountId(currentUserAccountId);
            if (account != null)
            {
                var acc = AccountAdapter.ConvertToViewModel(account);
                string statusFriend = new NetworkService().GetStatusFriend(currentAccount, account);
                if (acc.IsShowProfile || id.Equals(currentAccount.Id.ToString()))
                {
                    acc.StatusFriend = statusFriend;
                    acc.AccountType = currentAccount.AccountType.ToString();
                    return acc;
                }
                else
                {
                    acc.StatusFriend = statusFriend;
                    acc.AccountType = currentAccount.AccountType.ToString();
                    return new AccountViewModel()
                    {
                        DisplayName = acc.DisplayName,
                        IsShowProfile = acc.IsShowProfile,
                        Country = acc.Country
                    };
                }
            }
            else
            {
                throw new CustomException(Regit.Account_Not_Found_Message);
            }
        }

        [HttpGet, Route("GetUserProfile/{userId}")]
        public async Task<AccountViewModel> GetUserProfile(string userId)
        {
            var user = _accountService.GetById(new ObjectId(userId));
            return AccountAdapter.ConvertToViewModel(user);
           
        }        
        [HttpGet, Route("GetAccountProfile/{accountId}")]
        public async Task<AccountViewModel> GetAccountProfile(string accountId)
        {
            var user = _accountService.GetByAccountId(accountId);
            return AccountAdapter.ConvertToViewModel(user);
           
        }

        [HttpGet, Route("Followee")]
        public IHttpActionResult GetListFolloweeOfCurrentUser([FromUri] FollowRequest request)
        {
            var result = new FolloweeResponse();
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.GetByAccountId(currentUserAccountId);
            request = request ?? new FollowRequest();
            var followResult = _accountService.GetFollowByListUserId(new FolloweeParameter
            {
                Account = account,
                Start = request.Start ?? 0,
                Length = request.Length ?? ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"].ParseInt()
            });
            result.Total = followResult.Total;
            result.Data = AccountAdapter.ConvertToFolloweeViewModel(followResult.Followee, account);
            return Ok(result);
        }

        [HttpGet, Route("GetFollowTransactions")]
        public IHttpActionResult GetFollowTransactions([FromUri] TransactionRequest request)
        {
            ObjectId toUser;
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            var fromUser = currentUser.Id;
            var isValid = ObjectId.TryParse(request.ToUser, out toUser);
            if (!string.IsNullOrEmpty(request.FromUser))
            {
                isValid = ObjectId.TryParse(request.FromUser, out fromUser);
            }

            if (!isValid) return BadRequest("Data is invalid. Please check again.");

            var result = _transactionService.GetFollowTransactions(new TransactionParamter
            {
                TransactionType = TransactionType.FollowTransaction,
                FollowTransactionParamter = new FollowTransactionParamter
                {
                    FromUser = fromUser,
                    ToUser = toUser,
                    User = currentUser,
                    Date = DateTime.Now.Date,
                    Type = FollowType.Follow
                },
                Length = request.Length
            });
            var response = new TransactionResponse()
            {
                Transactions = TransactionAdapter.Convert(result.Transactions),
                Total = result.Total
            };
            return Ok(response);
        }

        [HttpPost, Route("NotificationSettings")]
        public dynamic UpdateAccountNotificationSetting(AccountNotificationSettings model)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            var account = _accountService.UpdateNotificationSettings(model, currentUserAccountId);
            return account.NotificationSettings;
        }

        [HttpPost, Route("Business/Privacy")]
        public dynamic UpdateBusinessPrivacy(UpdateBusinessPrivacyViewModel model)
        {
            var privacy = new BusinessPrivacy()
            {
                Privacy = model.Privacy,
                AllowComment = model.AllowComment
            };
            var account = _accountService.UpdateBusinessPrivacy(privacy, model.BAId);
            return account.BusinessPrivacies;
        }

        [AllowAnonymous]
        [HttpPost, Route("Language")]
        public void UpdateLanguage([FromBody] string code)
        {
            if (User.Identity.IsAuthenticated)
            {
                var languages = JArray
                    .Parse(File.ReadAllText(CommonFunctions.MapPath("~/Areas/User/Configs/languages.json")))
                    .ToObject<dynamic[]>();
                if (!languages.Any(l => l.Code == code))
                {
                    throw new CustomException("Language is not supported");
                }

                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                _accountService.UpdateLanguage(account.Id, code);
            }
        }


        [Route("UpdateProfilePicture")]
        [HttpPost]
        public HttpResponseMessage UpdateProfilePicture()
        {
            string userid = HttpContext.Current.User.Identity.GetUserId();
            var FileName = userid + "_profile_pic_" + DateTime.Now.Ticks + ".png";
            var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/ProfilePictures"));

            if (!directory.Exists)
            {
                directory.Create();
            }

            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {

                    string extension = Path.GetExtension(httpRequest.Files[file].FileName);
                    if (extension == ".jpg" || extension == ".bmp" || extension == ".png" || extension == ".gif" ||
                        extension == ".tiff" || extension == ".jpeg")
                    {
                        var postedFile = httpRequest.Files[file];

                        string filePath =
                            HostingEnvironment.MapPath(Path.Combine("~/Content/ProfilePictures/", FileName));
                        postedFile.SaveAs(filePath);
                        docfiles.Add(filePath);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                    }
                }
                string savePath = string.Format("/Content/ProfilePictures/" + FileName);
                result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                try
                {
                    var resultAccount = _accountService.UpdatePictureProfileByAccountId(userid, savePath);
                }
                catch
                {
                }
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }

        [Route("UpdateProfilePictureFileName")]
        [HttpPost]
        public object UpdateProfilePictureFileName()
        {
            string userid = HttpContext.Current.User.Identity.GetUserId();
            var FileName = userid + "_profile_pic_" + DateTime.Now.Ticks + ".png";
            var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/ProfilePictures"));
            string filePath = "";
            if (!directory.Exists)
            {
                directory.Create();
            }

            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    //.tiff .bmp .jpg .gif .png .jpeg


                    string extension = Path.GetExtension(httpRequest.Files[file].FileName);
                    if (extension == ".jpg" || extension == ".bmp" || extension == ".png" || extension == ".gif" ||
                        extension == ".tiff" || extension == ".jpeg")
                    {
                        var postedFile = httpRequest.Files[file];

                        filePath = HostingEnvironment.MapPath(Path.Combine("~/Content/ProfilePictures/", FileName));
                        postedFile.SaveAs(filePath);
                        docfiles.Add(filePath);
                    }
                    else
                    {
                        //throw new CustomException("Invalid request");
                        // return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                    }
                }
                string savePath = string.Format("/Content/ProfilePictures/" + FileName);
                result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                try
                {
                    var resultAccount = _accountService.UpdatePictureProfileByAccountId(userid, savePath);
                }
                catch
                {
                }
                return new {photoUrl = savePath};
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
                return new { };
            }
        }
    }
}