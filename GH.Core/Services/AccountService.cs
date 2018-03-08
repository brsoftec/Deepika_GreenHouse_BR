using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.Extensions;
using MongoDB.Bson;
using GH.Core.Exceptions;
using System.Net.Http;
using GH.Core.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using GH.Core.ViewModels;
using libphonenumber;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Delegation;
using System.Linq.Expressions;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using System.Reflection;
using System.Web;
using System.Configuration;
using System.Threading.Tasks;
using GH.Lang;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ManageTokenDevice;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.IServices;
using GH.Util;

namespace GH.Core.Services
{
    public class AccountService : IAccountService
    {
        public const string PROFILE_PICTURE_DIRECTORY = "~/Content/ProfilePictures";
        public const string BUSINESS_ALBUM_PICTURE_DIRECTORY = "~/Content/BusinessAlbumPictures";

        private IMongoCollection<Account> _accountCollection;
        private IMongoCollection<SecurityQuestion> _securityQuestionsCollection;
        private IMongoCollection<Role> _roleCollection;
        private Logger log = LogManager.GetCurrentClassLogger();
        private ITransactionService _transactionService;

        public AccountService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _securityQuestionsCollection = db.SecurityQuestions;
            _roleCollection = db.Roles;
            _transactionService = new TransactionService();
        }

        public List<Account> GetListALlUserEmailVerified()
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>("Account");
            var listueridregisaccount = accountCollection.Find<Account>(x => x.EmailVerified == true)
                .Project<Account>(
                    Builders<Account>.Projection
                        .Include(a => a.AccountId)
                        .Include(a => a.AccountType)
                        .Include(a => a.Profile.FirstName)
                        .Include(a => a.Profile.LastName)
                        .Include(a => a.Profile.DisplayName)
                        .Include(a => a.Profile.Email)
                ).ToList();
            return listueridregisaccount;
        }

        public List<Account> GetListUser(int start = 0, int take = 10, DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");

            var filter = new List<FilterDefinition<BsonDocument>>
            {
                Builders<BsonDocument>.Filter.Eq("AccountType", 1),
                Builders<BsonDocument>.Filter.Eq("AccountType", 0)
            };


            var listAccount = accountCollection.Find(Builders<BsonDocument>.Filter.Or(filter));

            //if (endDate.HasValue && startDate.HasValue)
            //{
            //    listAccount.Filter = listAccount.Filter & Builders<BsonDocument>.Filter.Gte<DateTime>("CreatedDate", startDate.Value.AddDays(-1));
            //    listAccount.Filter = listAccount.Filter & Builders<BsonDocument>.Filter.Lte<DateTime>("CreatedDate", endDate.Value);
            //}

            var result = listAccount.Skip((start * take)).Limit(take).ToList();

            var dateTime = Convert.ToDateTime(result[0].GetValue("CreatedDate").ToString());

            var newResult = result.Select(x => new Account
            {
                AccountId = x.GetValue("AccountId").ToString(),
                CreatedDate = ConvertStringToDateTime(x.GetValue("CreatedDate").ToString())
            }).ToList();

            if (startDate.HasValue && endDate.HasValue)
            {
                return newResult
                    .Where(x => x.CreatedDate >= startDate.Value && x.CreatedDate <= endDate.Value.AddDays(+1))
                    .ToList();
            }

            return newResult.ToList();
        }

        private DateTime ConvertStringToDateTime(string val)
        {
            DateTime date = DateTime.Now;

            DateTime.TryParse(val, out date);

            return date;
        }

        //  return _accountCollection.Find(t => t.Id == id).FirstOrDefault();

        public SearchUserResult Search(SearchUserCriteria criteria)
        {
            List<System.Linq.Expressions.Expression<Func<Account, bool>>> exps =
                new List<System.Linq.Expressions.Expression<Func<Account, bool>>>();
            System.Linq.Expressions.Expression<Func<Account, bool>> expression = x => true;//x.EmailVerified;

            if (criteria == null)
            {
                criteria = new SearchUserCriteria();
            }

            if (criteria.AccountType.HasValue)
            {
                var value = (int) criteria.AccountType.Value;
                exps.Add(x => (int) x.AccountType == value);
            }

            var now = DateTime.Now;
            if (criteria.FromAge.HasValue)
            {
                //find people who born before x years ago
                var fromDate = now.AddYears(-criteria.FromAge.Value);
                exps.Add(x => x.Profile.Birthdate.HasValue && x.Profile.Birthdate.Value <= fromDate);
            }

            if (criteria.ToAge.HasValue)
            {
                //find people who born after x years ago
                var toDate = now.AddYears(-criteria.ToAge.Value);
                exps.Add(x => x.Profile.Birthdate.HasValue && x.Profile.Birthdate.Value >= toDate);
            }

            if (!string.IsNullOrEmpty(criteria.Country))
            {
                exps.Add(x => x.Profile.Country == criteria.Country);
            }

            if (!string.IsNullOrEmpty(criteria.City))
            {
                exps.Add(x => x.Profile.City == criteria.City);
            }

            if (!string.IsNullOrEmpty(criteria.Gender))
            {
                exps.Add(x => x.Profile.Gender == criteria.Gender);
            }

            foreach (var exp in exps)
            {
                var bin = System.Linq.Expressions.Expression.AndAlso(expression.Body, exp.Body);
                expression =
                    System.Linq.Expressions.Expression.Lambda<Func<Account, bool>>(bin, expression.Parameters[0]);
            }


            var filter = Builders<Account>.Filter.Where(expression);

            var accounts = _accountCollection.Find(filter);

            return new SearchUserResult
            {
                Total = accounts.Count(),
                Accounts = accounts.Skip(criteria.Start).Limit(criteria.Length).ToList()
            };
        }

        public IQueryable<Account> Search(Expression<Func<Account, bool>> expression)
        {
            return _accountCollection.AsQueryable().Where(expression);
        }

        public Account Insert(Account account, string invitedDelegationId = null)
        {
            account.ModifiedDate = account.CreatedDate;
            _accountCollection.InsertOne(account);
            if (account.BusinessAccountRoles.Any())
            {
                var linked = account.BusinessAccountRoles.FirstOrDefault();
                _accountCollection.UpdateOne(a => a.Id == linked.AccountId,
                    Builders<Account>.Update.AddToSet(x => x.BusinessAccountRoles,
                        new BusinessAccountRole {AccountId = account.Id, RoleId = linked.RoleId}));
            }

            //Update delegation if invited
            if (!string.IsNullOrEmpty(invitedDelegationId))
            {
                //Update delegation for delegatOR
                var fromAccount = _accountCollection.Find(a =>
                        a.Delegations != null && a.Delegations.Any(d => d.DelegationId.Equals(invitedDelegationId)))
                    .SingleOrDefault();
                DelegationItemTemplate delegation = null;
                if (fromAccount != null)
                {
                    delegation =
                        fromAccount.Delegations.FirstOrDefault(d => d.DelegationId.Equals(invitedDelegationId));
                    delegation.ToAccountId = account.AccountId;
                    delegation.ToUserDisplayName = account.Profile.DisplayName;
                    this.UpdateDelegation(fromAccount.Delegations.ToList(), fromAccount.AccountId);

                    //Add delegation for delegatEE
                    List<DelegationItemTemplate> delegations = null;
                    if (account.Delegations != null)
                    {
                        delegations = account.Delegations.ToList();
                    }
                    else
                    {
                        delegations = new List<DelegationItemTemplate>();
                    }

                    delegation.Direction = EnumDelegationDirection.DelegationIn;
                    delegations.Add(delegation);
                    this.UpdateDelegation(delegations, account.AccountId);
                }

                //Send notification to delegatEE
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.DelegationRequest;
                notificationMessage.FromAccountId = fromAccount.AccountId;
                notificationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = account.AccountId;
                notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                notificationMessage.Content = delegation.Message;
                notificationMessage.PreserveBag = delegation.DelegationId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }


            return account;
        }

        public List<Account> GetAllAccountLinkedSocialNetwork()
        {
            return _accountCollection.Find(t => t.AccountLinks.Any()).ToList();
        }

        public Account GetById(ObjectId id)
        {
            return _accountCollection.Find(t => t.Id == id).FirstOrDefault();
        }

        public Account GetById(string id)
        {
            if (!ObjectId.TryParse(id, out var objId)) return null;
            return _accountCollection.Find(t => t.Id == objId).FirstOrDefault();
        }

        public Account GetByEmail(string email)
        {
            return _accountCollection.Find(a => a.Profile.Email.ToLower() == email.ToLower()).FirstOrDefault();
        }

        public Account GetByAccountId(string accountId)
        {
            // log.Debug($"GetByAccountId: {_accountCollection} AccountId: {accountId}");

            return _accountCollection.Find(t => t.AccountId == accountId).FirstOrDefault();
        }

        public List<Account> GetByListId(List<ObjectId> ids)
        {
            var accounts = new List<Account>();
            for (var i = 0; i < ids.Count; i += 2000)
            {
                var idTemps = ids.Skip(i).Take(2000);
                accounts.AddRange(_accountCollection.Find(t => ids.Contains(t.Id)).ToList());
            }

            return accounts;
        }

        public List<Account> GetByListSocialId(List<string> ids)
        {
            return _accountCollection.Find(t => ids.Contains(t.SocialAccountId)).ToList();
        }

        public List<Account> GetBusinessAccountsLinkWithPersonalAccount(ObjectId personalAccountId)
        {
            var personalAcc = _accountCollection
                .Find(a => a.Id == personalAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();

            if (personalAcc == null)
            {
                throw new CustomException("The account is not type of personal");
            }

            if (personalAcc.BusinessAccountRoles == null)
            {
                return new List<Account>();
            }

            var businessAccountIds = personalAcc.BusinessAccountRoles.Select(a => a.AccountId).ToList();

            return _accountCollection.Find(a => businessAccountIds.Contains(a.Id)).ToList();
        }

        public void UpdateStatus(Account account, string status)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);
            var builder = Builders<Account>.Filter;
            var filter = builder.Eq(c => c.Id, account.Id);
            var update = Builders<Account>.Update
                .Set(f => f.Status, status);
            if (status == EnumAccount.LockResetPassword)
            {
                var dt = DateTime.Now;
                update = Builders<Account>.Update
                    .Set(f => f.Status, status)
                    .Set(a => a.EmailVerifyTokenDate, dt);
            }

            accountCollection.UpdateOne(filter, update);

            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UpdateStatus");
        }

        public void UpdateProfileProperty(Account account, string name, dynamic value)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);

            try
            {
                var builder = Builders<Account>.Filter;
                var filter = builder.Eq(c => c.Id, account.Id);
                var update = Builders<Account>.Update
                    .Set("Profile." + name, value);

                accountCollection.UpdateOne(filter, update);
                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdateProfile");
            }
            catch
            {
            }
        }

        private static readonly Dictionary<string, string> ProfileProps = new Dictionary<string, string>
        {
            ["displayname"] = "DisplayName",
            ["firstname"] = "FirstName",
            ["middlename"] = "MiddleName",
            ["lastname"] = "LastName",
            ["gender"] = "Gender",
            ["dob"] = "Birthdate",
            ["description"] = "Status",
            ["country"] = "Country",
            ["city"] = "City",
            ["address"] = "Street",
            ["zipcode"] = "ZipPostalCode",
            ["picture"] = "PhotoUrl"
        };

        public async Task<FuncResult> SetProfile(string name, object value, string accountId)
        {
            if (!ProfileProps.TryGetValue(name.ToLower(), out var key))
                return new ErrResult("profile.key.invalid");

            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);

            var builder = Builders<Account>.Filter;
            var filter = builder.Eq(c => c.AccountId, accountId);

            var update = Builders<Account>.Update.Set("Profile." + key, value);

            var result = await accountCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("profile.update.not", key);

            if (key == "Status")
            {
                update = Builders<Account>.Update.Set("Profile.Description", value);
                await accountCollection.UpdateOneAsync(filter, update);
            }

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(accountId, "UpdateProfile");

            return new OkResult("profile.set.ok", key);
        }       
        public async Task<FuncResult> SetAccountStatusAsync(string key, object value, string accountId)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);

            var builder = Builders<Account>.Filter;
            var filter = builder.Eq(c => c.AccountId, accountId);

            var update = Builders<Account>.Update.Set("AccountStatus." + key, value);

            var result = await accountCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("profile.update.not", key);

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(accountId, "UpdateAccountStatus");

            return new OkResult("account.status.set.ok", key);
        }
        public async Task<FuncResult> CloseAccount(string accountId)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);

            var builder = Builders<Account>.Filter;
            var filter = builder.Eq(c => c.AccountId, accountId);

            var update = Builders<Account>.Update.Set("Status", "Close");

            var result = await accountCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("account.close.not");

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(accountId, "CloseAccount");

            return new OkResult("account.close.ok");
        }      
        public async Task<FuncResult> ReopenAccount(string accountId)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);

            var builder = Builders<Account>.Filter;
            var filter = builder.Eq(c => c.AccountId, accountId);

            var update = Builders<Account>.Update.Set("Status", "");

            var result = await accountCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("account.reopen.not");

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(accountId, "ReopenAccount");

            return new OkResult("account.reopen.ok");
        }

        public async Task<FuncResult> SetSecurityQuestionsAsync(IList<AccessSecurityAnswer> sqa, Account account)
        {
            var sqCollection = MongoDBConnection.Database.GetCollection<SecurityQuestion>("SecurityQuestion");
            var builder = Builders<SecurityQuestion>.Filter;

            var answers = new List<SecurityQuestionAnswer>();

            for (var i = 0; i < 3; i++)
            {
                var securityQuestion = sqa[i];
                if (string.IsNullOrEmpty(securityQuestion.Code))
                    return new ErrResult("sq.code.missing", message: $"Security question #{i + 1} missing code");
                if (string.IsNullOrEmpty(securityQuestion.Answer))
                    return new ErrResult("sq.answer.missing", message: $"Security question #{i + 1} missing answer");
                var filter = builder.Eq("Code", securityQuestion.Code);
                var sq = await sqCollection.Find(filter).FirstOrDefaultAsync();
                if (sq == null)
                {
                       return new ErrResult("sq.code.unknown",
                            message: $"Security question #{i + 1} unknown code: {securityQuestion.Code}");
                }

                answers.Add(new SecurityQuestionAnswer
                {
                    QuestionId = sq.Id,
                    Answer = securityQuestion.Answer
                });
            }

            var accountCollection = MongoDBConnection.Database.GetCollection<Account>("Account");

            var filterAccount = Builders<Account>.Filter.Eq("AccountId", account.AccountId);
            var update = Builders<Account>.Update;
            for (var i = 0; i < 3; i++)
            {
                var answer = answers[i];
                var result =
                    await accountCollection.UpdateOneAsync(filterAccount,
                        update.Set($"SecurityQuesion{i + 1}", answer));
            }

            return new OkResult("sq.set.ok");
        }


        public void UpdateViewPreference(Account account, string name, dynamic value)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);
            //var account = _accountCollection.Find(t => t.AccountId == account.Id).FirstOrDefault();


            try
            {
                var builder = Builders<Account>.Filter;
                var filter = builder.Eq(c => c.Id, account.Id);

                var acc = accountCollection.Find(filter).FirstOrDefault();


                var options = new UpdateOptions
                {
                    IsUpsert = true
                };


                if (acc.ViewPreferences == null)
                {
                    var prefs = new AccountViewPreferences { };
                    var create = Builders<Account>.Update
                        .Set(f => f.ViewPreferences, prefs);
                    accountCollection.UpdateOne(filter, create);
                }

                var update = Builders<Account>.Update
                    .Set("ViewPreferences." + name, value);

                accountCollection.UpdateOne(filter, update, options);
                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdateViewPreference");
            }
            catch
            {
            }
        }

        public void UpdateViewPreferences(Account account, AccountViewPreferences prefs)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>(RegitTable.Account);
            //var account = _accountCollection.Find(t => t.AccountId == account.Id).FirstOrDefault();

            Logger log = LogManager.GetCurrentClassLogger();
            try
            {
                var builder = Builders<Account>.Filter;
                var filter = builder.Eq(c => c.Id, account.Id);

                var update = Builders<Account>.Update
                    .Set(f => f.ViewPreferences, prefs);

                accountCollection.UpdateOne(filter, update);

                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdateViewPreferences");
            }
            catch
            {
            }
        }

        public Account UpdateProfile(Account account, MultipartFileData profilePicture)
        {
            var exist = _accountCollection.Find(t => t.Id == account.Id && t.AccountId == account.AccountId)
                .FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            exist.ModifiedDate = DateTime.Now;
            exist.Profile.City = account.Profile.City;
            exist.Profile.Country = account.Profile.Country;
            exist.Profile.DisplayName = account.Profile.DisplayName;
            exist.Profile.Status = account.Profile.Status;
            exist.Profile.Birthdate = account.Profile.Birthdate;
            exist.Profile.Gender = account.Profile.Gender;
            exist.Profile.Description = account.Profile.Description;
            exist.Profile.Street = account.Profile.Street;
            exist.Profile.Region = account.Profile.Region;
            exist.Profile.ZipPostalCode = account.Profile.ZipPostalCode;

            string name = exist.Id + "_profile_pic_" + DateTime.Now.Ticks;

            string oldPic = null;

            if (profilePicture != null)
            {
                oldPic = exist.Profile.PhotoUrl;
                var localFileName = profilePicture.Headers.ContentDisposition.FileName;
                if (localFileName.StartsWith("\"") && localFileName.EndsWith("\""))
                {
                    localFileName = localFileName.Trim('"');
                }

                name += Path.GetExtension(localFileName);

                FileAccessHelper.SaveMultipartFileData(profilePicture, PROFILE_PICTURE_DIRECTORY, name);

                exist.Profile.PhotoUrl = PROFILE_PICTURE_DIRECTORY.Trim('~') + "/" + name;
            }

            _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id), exist);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UpdateProfile");

            if (!string.IsNullOrEmpty(oldPic))
            {
                try
                {
                    FileAccessHelper.DeleteFile("~" + oldPic);
                }
                catch (Exception ex)
                {
                }
            }

            return exist;
        }

        public bool UpdatePIN(string pin, Account account)

        {
            var rs = false;
            var exist = _accountCollection.Find(t => t.Id == account.Id && t.AccountId == account.AccountId)
                .FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            exist.ModifiedDate = DateTime.Now;
            exist.Profile.PinCode = pin;
            try
            {
                _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id), exist);
                rs = true;
                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdatePIN");
            }
            catch
            {
                rs = false;
            }

            return rs;
        }

        public bool CloseAccount(Account account)

        {
            var rs = false;
            var exist = _accountCollection.Find(t => t.Id == account.Id && t.AccountId == account.AccountId)
                .FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            exist.ModifiedDate = DateTime.Now;
            exist.Status = "Close";
            try
            {
                _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id), exist);
                rs = true;
                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdatePIN");
            }
            catch
            {
                rs = false;
            }

            return rs;
        }

        public bool UnCloseAccount(Account account)

        {
            var rs = false;
            var exist = _accountCollection.Find(t => t.Id == account.Id && t.AccountId == account.AccountId)
                .FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            exist.ModifiedDate = DateTime.Now;
            exist.Status = "";
            try
            {
                _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id), exist);
                rs = true;
                // Sync to Admin
                var adminS = new AdminService();
                adminS.syncaccount(account.AccountId, "UpdatePIN");
            }
            catch
            {
                rs = false;
            }

            return rs;
        }

        //
        public Account UpdatePictureProfileByAccountId(string accountId, string profilePicture)
        {
            var exist = _accountCollection.Find(t => t.AccountId == accountId).FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            var oldPicture = exist.Profile.PhotoUrl;
            exist.Profile.PhotoUrl = profilePicture;

            _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id), exist);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(exist.AccountId, "UpdatePictureProfileByAccountId");
            if (!string.IsNullOrEmpty(oldPicture))
            {
                try
                {
                    FileAccessHelper.DeleteFile("~" + oldPicture);
                }
                catch (Exception ex)
                {
                }
            }

            return exist;
        }

        public Account UpdatePrivacies(AccountPrivacies privacies, string accountId)
        {
            var account = _accountCollection.Find(t => t.AccountId == accountId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            account.ModifiedDate = DateTime.Now;
            account.AccountPrivacies = privacies;
            _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(a => a.Id, account.Id), account);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UpdatePrivacies");
            return account;
        }

        public Account UpdateDelegation(List<DelegationItemTemplate> delegations, string accountId)
        {
            var account = _accountCollection.Find(t => t.AccountId == accountId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            account.ModifiedDate = DateTime.Now;
            account.Delegations = delegations;
            _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(a => a.Id, account.Id), account);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UpdatePrivacies");
            //  log.Debug("UpdateDelegation:  " + accountId);
            return account;
        }

        public Account UpdateLastestNotification(LatestNotification latestNotification, string accountId)
        {
            var account = _accountCollection.Find(t => t.AccountId.Equals(accountId)).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            var update = Builders<Account>.Update.Set(a => a.LastestNotification, latestNotification);
            _accountCollection.UpdateOne(filter, update);
            // Sync to Admin
            //var adminS = new AdminService();
            //adminS.syncaccount(account.AccountId, "UpdateLastestNotification");
            return account;
        }

        public Account UpdateAccountActivityLogSettings(AccountActivityLogSettings actLogSettings, string accountId)
        {
            var account = _accountCollection.Find(t => t.AccountId == accountId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            account.ModifiedDate = DateTime.Now;
            account.AccountActivityLogSettings = actLogSettings;
            _accountCollection.ReplaceOne(Builders<Account>.Filter.Eq(a => a.Id, account.Id), account);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UpdateLastestNotification");
            return account;
        }

        public Account UpdateSecurityQuestions(AnswerSecurityQuestionModel questions, string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.AccountId == accountId).FirstOrDefault();

            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

/*            if (!existAccount.PhoneNumberVerified)
            {
                throw new CustomException("Invalid request");
            }*/

            /*
            if ((questions.VerifiedToken != existAccount.PhoneVerifiedToken || !existAccount.PhoneVerifiedTokenExpired.HasValue || existAccount.PhoneVerifiedTokenExpired < DateTime.Now)
                && existAccount.SecurityQuesion1 != null && existAccount.SecurityQuesion2 != null && existAccount.SecurityQuesion3 != null)
            {
                throw new CustomException(new ErrorViewModel { Error = ErrorCode.AUTH_VerifiedPhoneTokenExpired, Message = "Your phone authentication has been expired. Please re-authenticate." });
            }
            */

            if (questions.Question1 == null || questions.Question2 == null || questions.Question3 == null ||
                string.IsNullOrEmpty(questions.Question1.Answer) || string.IsNullOrEmpty(questions.Question2.Answer) ||
                string.IsNullOrEmpty(questions.Question3.Answer))
            {
                throw new CustomException(new ErrorViewModel {Message = "Please answer for all questions"});
            }

            var question1 = _securityQuestionsCollection.Find(x => x.Id == new ObjectId(questions.Question1.QuestionId))
                .FirstOrDefault();
            var question2 = _securityQuestionsCollection.Find(x => x.Id == new ObjectId(questions.Question2.QuestionId))
                .FirstOrDefault();
            var question3 = _securityQuestionsCollection.Find(x => x.Id == new ObjectId(questions.Question3.QuestionId))
                .FirstOrDefault();

            if (question1 == null || question2 == null || question3 == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Question does not exist"});
            }

            existAccount.SecurityQuesion1 = new SecurityQuestionAnswer
            {
                QuestionId = question1.Id,
                Answer = questions.Question1.Answer
            };

            existAccount.SecurityQuesion2 = new SecurityQuestionAnswer
            {
                QuestionId = question2.Id,
                Answer = questions.Question2.Answer
            };

            existAccount.SecurityQuesion3 = new SecurityQuestionAnswer
            {
                QuestionId = question3.Id,
                Answer = questions.Question3.Answer
            };

            //            existAccount.AccountStatus.NoSecurityQuestions = false;

            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.SecurityQuesion1, existAccount.SecurityQuesion1)
                .Set(a => a.SecurityQuesion2, existAccount.SecurityQuesion2)
                .Set(a => a.SecurityQuesion3, existAccount.SecurityQuesion3)
                .Set(a => a.AccountStatus.NoSecurityQuestions, false)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(existAccount.AccountId, "UpdateLastestNotification");
            return existAccount;
        }

        public Account UpdatePhoneNumber(string phoneNumber, string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.AccountId == accountId).FirstOrDefault();

            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.Profile.PhoneNumber = PhoneNumberHelper.GetFormatedPhoneNumber(phoneNumber);
            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.Profile.PhoneNumber, existAccount.Profile.PhoneNumber)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);

            return existAccount;
        }

        /* PIN Code */
        public Account UpdatePinCode(string pinCode, string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.AccountId == accountId).FirstOrDefault();

            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.Profile.PinCode = pinCode;
            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.Profile.PinCode, existAccount.Profile.PinCode)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(existAccount.AccountId, "UpdatePinCode");
            return existAccount;
        }
        /* End PIN Code */

        public string GeneratePhoneVerifiedToken(string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.AccountId == accountId).FirstOrDefault();

            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.PhoneVerifiedToken = Guid.NewGuid().ToString();
            existAccount.PhoneVerifiedTokenExpired = DateTime.Now.AddMinutes(5);
            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.PhoneVerifiedToken, existAccount.PhoneVerifiedToken)
                .Set(a => a.PhoneVerifiedTokenExpired, existAccount.PhoneVerifiedTokenExpired)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(existAccount.AccountId, "GeneratePhoneVerifiedToken");

            return existAccount.PhoneVerifiedToken;
        }

        public bool CheckDuplicateEmail(string email, string ignoreAccountId)
        {
            var accounts = _accountCollection.Find(a => a.Profile.Email.ToLower() == email.ToLower()).ToList();
            if (string.IsNullOrEmpty(ignoreAccountId))
            {
                return accounts.Any();
            }
            else
            {
                return accounts.Any(a => a.AccountId != ignoreAccountId);
            }
        }

        public bool CheckDuplicateEmail(string email, ObjectId ignoreAccountId)
        {
            var accounts = _accountCollection.Find(a => a.Profile.Email.ToLower() == email.ToLower()).ToList();
            if (ignoreAccountId == null)
            {
                return accounts.Any();
            }
            else
            {
                return accounts.Any(a => a.Id != ignoreAccountId);
            }
        }

        public bool HasAccountLinkWithSocial(SocialType social, string socialId)
        {
            return _accountCollection
                .Find(a => a.AccountLinks.Any(l => l.Type == social && l.SocialAccountId == socialId)).Any();
        }

        public bool HasPersonalAccountLinkWithSocial(SocialType social, string socialId)
        {
            return _accountCollection.Find(a =>
                a.AccountType == AccountType.Personal &&
                a.AccountLinks.Any(l => l.Type == social && l.SocialAccountId == socialId)).Any();
        }

        public void LinkAccount(AccountLink accountLink, ObjectId accountId)
        {
            var existAccount = _accountCollection.Find(a => a.Id == accountId).FirstOrDefault();
            if (existAccount == null)
            {
                throw new CustomException("Account does not exist");
            }

            var linked = _accountCollection.Count(a =>
                ((accountLink.Type == SocialType.Facebook && a.AccountType == existAccount.AccountType) ||
                 accountLink.Type == SocialType.Twitter) && a.AccountLinks.Any(l =>
                    l.SocialAccountId == accountLink.SocialAccountId && l.Type == accountLink.Type));
            if (linked > 0)
                throw new CustomException("Social account has already linked with an account");

            if (existAccount.AccountLinks == null)
            {
                existAccount.AccountLinks = new List<AccountLink>();
            }

            if (existAccount.AccountLinks.Any(l => l.Type == accountLink.Type))
            {
                throw new CustomException("Your account has already linked with a " + accountLink.Type.ToString() +
                                          " account");
            }

            existAccount.AccountLinks.Add(accountLink);
            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.AccountLinks, existAccount.AccountLinks)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(existAccount.AccountId, "LinkAccount");
        }

        public void UnlinkAccount(ObjectId accountId, SocialType network)
        {
            if (network == SocialType.GreenHouse)
            {
                throw new CustomException("Cannot unlink Regit");
            }

            var account = _accountCollection.Find(a => a.Id == accountId).FirstOrDefault();

            if (account == null)
            {
                throw new CustomException("Account not found");
            }

            var linked = account.AccountLinks.FirstOrDefault(l => l.Type == network);
            if (linked == null)
            {
                throw new CustomException("Linked network not found");
            }

            account.ModifiedDate = DateTime.Now;

            _accountCollection.UpdateOne(a => a.Id == account.Id,
                Builders<Account>.Update
                    .PullFilter(a => a.AccountLinks, a => a.SocialAccountId == linked.SocialAccountId)
                    .Set(a => a.ModifiedDate, account.ModifiedDate));

            if (account.AccountType == AccountType.Business && network == SocialType.Facebook)
            {
                var connectedPage = account.Pages.FirstOrDefault(p => p.SocialType == SocialType.Facebook);
                if (connectedPage != null)
                {
                    DisconnectSocialPage(account.Id, network);
                }
            }

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "UnlinkAccount");
        }

        public void ConnectSocialPage(SocialPage page, ObjectId accountId)
        {
            var account = _accountCollection.Find(a => a.Id == accountId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException("Account not found");
            }

            if (account.Pages.Any(p => p.SocialType == page.SocialType))
            {
                throw new CustomException("Your account has already connected with a " + page.SocialType.ToString() +
                                          " page");
            }

            account.Pages.Add(page);

            var update = Builders<Account>.Update.Set(a => a.Pages, account.Pages)
                .Set(a => a.ModifiedDate, DateTime.Now);
            _accountCollection.UpdateOne(a => a.Id == account.Id, update);
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "ConnectSocialPage");
        }

        public void DisconnectSocialPage(ObjectId accountId, SocialType network)
        {
            var account = _accountCollection.Find(a => a.Id == accountId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException("Account not found");
            }

            var connected = account.Pages.FirstOrDefault(p => p.SocialType == network);
            if (connected == null)
            {
                throw new CustomException("Page not found on your account");
            }

            _accountCollection.UpdateOne(a => a.Id == account.Id,
                Builders<Account>.Update.PullFilter(a => a.Pages, a => a.Id == connected.Id)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(account.AccountId, "DisconnectSocialPage");
        }

        public List<Account> GetMembersInBusiness(ObjectId businessAccountId, int? start, int? length, out long total)
        {
            var query = _accountCollection.Find(a =>
                a.AccountType == AccountType.Personal &&
                a.BusinessAccountRoles.Any(b => b.AccountId == businessAccountId));
            total = query.Count();
            return query.Skip(start).Limit(length).ToList();
        }

        public long CountMembersInBusiness(ObjectId businessAccountId)
        {
            var query = _accountCollection.Find(a =>
                a.AccountType == AccountType.Personal &&
                a.BusinessAccountRoles.Any(b => b.AccountId == businessAccountId));
            return query.Count();
        }

        public void UpdateMemberRoleInBusiness(ObjectId businessAccountId, ObjectId memberAccountId,
            List<ObjectId> roleIds, ObjectId modifierAccountId)
        {
            var businessAccount = _accountCollection
                .Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            }

            var personalAccount = _accountCollection
                .Find(a => a.Id == memberAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("Personal account does not exist");
            }

            if (modifierAccountId != businessAccountId)
            {
                var modifierAccount = _accountCollection
                    .Find(a => a.Id == modifierAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
                if (modifierAccount == null)
                {
                    throw new CustomException("Modifier account does not exist");
                }

                var adminRole = _roleCollection.Find(r => r.Name == Role.ROLE_ADMIN).FirstOrDefault();

                if (!modifierAccount.BusinessAccountRoles.Any(r =>
                    r.AccountId == businessAccountId && r.RoleId == adminRole.Id))
                {
                    throw new CustomException("Modifier does not have access to modify business member access");
                }
            }

            if (!businessAccount.BusinessAccountRoles.Any(a => a.AccountId == memberAccountId))
            {
                throw new CustomException("Personal account is not member of business");
            }

            personalAccount.BusinessAccountRoles.RemoveAll(b => b.AccountId == businessAccountId);
            businessAccount.BusinessAccountRoles.RemoveAll(b => b.AccountId == memberAccountId);
            if (roleIds == null || roleIds.Count == 0)
            {
                businessAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                {
                    AccountId = memberAccountId,
                    RoleId = null
                });
                personalAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                {
                    AccountId = businessAccountId,
                    RoleId = null
                });
            }
            else
            {
                foreach (var roleId in roleIds)
                {
                    businessAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = memberAccountId,
                        RoleId = roleId
                    });
                    personalAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = businessAccountId,
                        RoleId = roleId
                    });
                }
            }

            _accountCollection.UpdateOne(a => a.Id == businessAccountId,
                Builders<Account>.Update.Set(a => a.BusinessAccountRoles, businessAccount.BusinessAccountRoles)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            _accountCollection.UpdateOne(a => a.Id == memberAccountId,
                Builders<Account>.Update.Set(a => a.BusinessAccountRoles, personalAccount.BusinessAccountRoles)
                    .Set(a => a.ModifiedDate, DateTime.Now));

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(businessAccount.AccountId, "UpdateMemberRoleInBusiness");
            adminS.syncaccount(personalAccount.AccountId, "UpdateMemberRoleInBusiness");
        }

        public void UpdateMember(ObjectId businessAccountId, ObjectId memberAccountId, List<string> roles)
        {
            var businessAccount = _accountCollection
                .Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            }

            var personalAccount = _accountCollection
                .Find(a => a.Id == memberAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("Personal account does not exist");
            }

            //            if (modifierAccountId != businessAccountId)
            //            {
            //                var modifierAccount = _accountCollection.Find(a => a.Id == modifierAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
            //                if (modifierAccount == null)
            //                {
            //                    throw new CustomException("Modifier account does not exist");
            //                }
            //
            //                var adminRole = _roleCollection.Find(r => r.Name == Role.ROLE_ADMIN).FirstOrDefault();
            //
            //                if (!modifierAccount.BusinessAccountRoles.Any(r => r.AccountId == businessAccountId && r.RoleId == adminRole.Id))
            //                {
            //                    throw new CustomException("Modifier does not have access to modify business member access");
            //                }
            //            }

            if (!businessAccount.BusinessAccountRoles.Any(a => a.AccountId == memberAccountId))
            {
                throw new CustomException("Personal account is not member of business");
            }

            personalAccount.BusinessAccountRoles.RemoveAll(b => b.AccountId == businessAccountId);
            businessAccount.BusinessAccountRoles.RemoveAll(b => b.AccountId == memberAccountId);

            if (roles == null || roles.Count == 0)
            {
                businessAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                {
                    AccountId = memberAccountId,
                    RoleId = null
                });
                personalAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                {
                    AccountId = businessAccountId,
                    RoleId = null
                });
            }
            else
            {
                var editorRole = _roleCollection.Find(r => r.Name == Role.ROLE_EDITOR).FirstOrDefault();
                var adminRole = _roleCollection.Find(r => r.Name == Role.ROLE_ADMIN).FirstOrDefault();
                var approverRole = _roleCollection.Find(r => r.Name == Role.ROLE_REVIEWER).FirstOrDefault();

                if (editorRole == null || adminRole == null || approverRole == null)
                {
                    throw new CustomException("Role does not exist");
                }

                foreach (var roleName in roles)
                {
                    Role role = null;
                    if (roleName == "admin")
                    {
                        role = adminRole;
                    }
                    else if (roleName == "editor")
                    {
                        role = editorRole;
                    }
                    else if (roleName == "approver")
                    {
                        role = approverRole;
                    }

                    if (role == null) continue;
                    businessAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = memberAccountId,
                        RoleId = role.Id
                    });
                    personalAccount.BusinessAccountRoles.Add(new BusinessAccountRole
                    {
                        AccountId = businessAccountId,
                        RoleId = role.Id
                    });
                }
            }

            _accountCollection.UpdateOne(a => a.Id == businessAccountId,
                Builders<Account>.Update.Set(a => a.BusinessAccountRoles, businessAccount.BusinessAccountRoles)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            _accountCollection.UpdateOne(a => a.Id == memberAccountId,
                Builders<Account>.Update.Set(a => a.BusinessAccountRoles, personalAccount.BusinessAccountRoles)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(businessAccount.AccountId, "UpdateMember");
            adminS.syncaccount(personalAccount.AccountId, "UpdateMember");
            string title = "You updated roles of workflow member " + personalAccount.Profile.DisplayName;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.WorkflowUpdateMember;
            notificationMessage.FromAccountId = businessAccount.AccountId;
            notificationMessage.FromUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = personalAccount.AccountId;
            notificationMessage.ToUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = "";

            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void RemoveMemberFromBusiness(ObjectId businessAccountId, ObjectId memberAccountId,
            ObjectId removerAccountId)
        {
            var businessAccount = _accountCollection
                .Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            }

            var personalAccount = _accountCollection
                .Find(a => a.Id == memberAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("Personal account does not exist");
            }

            if (removerAccountId != businessAccountId)
            {
                var modifierAccount = _accountCollection
                    .Find(a => a.Id == removerAccountId && a.AccountType == AccountType.Personal).FirstOrDefault();
                if (modifierAccount == null)
                {
                    throw new CustomException("Remover account does not exist");
                }

                var adminRole = _roleCollection.Find(r => r.Name == Role.ROLE_ADMIN).FirstOrDefault();

                if (!modifierAccount.BusinessAccountRoles.Any(r =>
                    r.AccountId == businessAccountId && r.RoleId == adminRole.Id))
                {
                    throw new CustomException("Remover does not have access to remove business member");
                }
            }

            if (!businessAccount.BusinessAccountRoles.Any(a => a.AccountId == memberAccountId))
            {
                throw new CustomException("Personal account is not member of business");
            }

            _accountCollection.UpdateMany(a => a.Id == businessAccountId || a.Id == memberAccountId, Builders<Account>
                .Update.PullFilter(a => a.BusinessAccountRoles,
                    a => a.AccountId == businessAccountId || a.AccountId == memberAccountId)
                .Set(a => a.ModifiedDate, DateTime.Now));
            var adminS = new AdminService();
            adminS.syncaccount(businessAccount.AccountId, "RemoveMemberFromBusiness");
            adminS.syncaccount(personalAccount.AccountId, "RemoveMemberFromBusiness");

            string title = "You removed " + personalAccount.Profile.DisplayName + " from your business workflow.";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.WorkflowRemoveMember;
            notificationMessage.FromAccountId = businessAccount.AccountId;
            notificationMessage.FromUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = personalAccount.AccountId;
            notificationMessage.ToUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = "";
            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public List<Account> GetUsersModifiedAfter(DateTime fromTime)
        {
            return _accountCollection.Find(a => a.ModifiedDate >= fromTime).ToList();
        }

        public Dictionary<ObjectId, List<ObjectId>> GetAccountIdsRelatedToUsers(params ObjectId[] userId)
        {
            var follows = _accountCollection.Find(u => userId.Contains(u.Id) && u.AccountType == AccountType.Personal)
                .Project(a => new {Id = a.Id, Follows = a.Followees.Select(f => f.AccountId)}).ToList();
            var busFollows = _accountCollection
                .Find(u => userId.Contains(u.Id) && u.AccountType == AccountType.Business)
                .Project(a => new {Id = a.Id, Follows = a.Followers.Select(f => f.AccountId)}).ToList();

            follows = follows.Concat(busFollows).ToList();

            IMongoCollection<Network> _networkCollection = MongoContext.Db.Networks;
            var friends = _networkCollection.Aggregate().Group(a => a.NetworkOwner,
                a => new {NetworkOwner = a.Key, Friends = a.Select(f => f.Friends)}).ToList();

            return userId.ToDictionary(u => u,
                u => follows.FirstOrDefault(f => f.Id == u).Follows.Concat(friends
                    .FirstOrDefault(t => t.NetworkOwner == u).Friends.SelectMany(t => t.Select(f => f.Id))).ToList());
        }

        public Account UpdateBusinessAccountPictureAlbum(ObjectId businessAccountId, List<MultipartFileData> newPhotos,
            List<string> deletingPhotos)
        {
            var exist = _accountCollection.Find(t => t.Id == businessAccountId && t.AccountType == AccountType.Business)
                .FirstOrDefault();
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            var deleteCount = exist.PictureAlbum.Count(p => deletingPhotos.Contains(p));
            var addNewCount = newPhotos.Count;

            if (exist.PictureAlbum.Count - deleteCount + addNewCount > 6)
            {
                throw new CustomException("You can upload only 6 photos");
            }

            exist.PictureAlbum.RemoveAll(p => deletingPhotos.Contains(p));

            var count = 1;
            foreach (var photo in newPhotos)
            {
                string name = exist.Id + "_picture_album_" + DateTime.Now.Ticks + "_" + count;
                var localFileName = photo.Headers.ContentDisposition.FileName;
                if (localFileName.StartsWith("\"") && localFileName.EndsWith("\""))
                {
                    localFileName = localFileName.Trim('"');
                }

                name += Path.GetExtension(localFileName);
                FileAccessHelper.SaveMultipartFileData(photo, BUSINESS_ALBUM_PICTURE_DIRECTORY, name);
                exist.PictureAlbum.Add(BUSINESS_ALBUM_PICTURE_DIRECTORY.Trim('~') + "/" + name);
                count++;
            }

            exist.ModifiedDate = DateTime.Now;

            _accountCollection.UpdateOne(Builders<Account>.Filter.Eq(t => t.Id, exist.Id),
                Builders<Account>.Update.Set(a => a.PictureAlbum, exist.PictureAlbum)
                    .Set(a => a.ModifiedDate, exist.ModifiedDate));

            foreach (var deletePhoto in deletingPhotos)
            {
                try
                {
                    FileAccessHelper.DeleteFile("~" + deletePhoto);
                }
                catch (Exception ex)
                {
                    //Logger logger = LogManager.GetCurrentClassLogger();
                    //logger.Error(ex, "Unhandle exception when try to delete old album picture ~" + deletePhoto);
                }
            }

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(exist.AccountId, "UpdateBusinessAccountPictureAlbum");

            return exist;
        }

        public void UpdateWorkTime(ObjectId businessAccountId, DateTime? workHourFrom, DateTime? workHourTo,
            List<string> workdays)
        {
            var busUser = _accountCollection
                .Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (busUser == null)
            {
                throw new CustomException(GH.Lang.Regit.Business_account_does_not_exist);
            }

            if (!workHourFrom.HasValue || !workHourTo.HasValue)
            {
                throw new CustomException(GH.Lang.Regit.Work_hours_is_required);
            }
            else if (workdays == null || workdays.Count == 0)
            {
                throw new CustomException(GH.Lang.Regit.Work_hours_is_required);
            }
            else if (workdays.Any(w => !CompanyDetails.AllWorkdays.Contains(w)))
            {
                throw new CustomException(GH.Lang.Regit.Invalid_work_days);
            }

            _accountCollection.UpdateOne(a => a.Id == busUser.Id, Builders<Account>.Update
                .Set(a => a.CompanyDetails.WorkHourFrom, workHourFrom)
                .Set(a => a.CompanyDetails.WorkHourTo, workHourTo)
                .Set(a => a.CompanyDetails.Workdays, workdays).Set(a => a.ModifiedDate, DateTime.Now));

            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(busUser.AccountId, "UpdateWorkTime");
        }

        public Account FollowBusiness(ObjectId followerId, ObjectId businessId, DateTime? datefollow = null)
        {
            var follower = _accountCollection.Find(f => f.Id == followerId).FirstOrDefault();
            if (follower == null)
            {
                throw new CustomException("Follower does not exist");
            }

            var business = _accountCollection.Find(f => f.Id == businessId && f.AccountType == AccountType.Business)
                .FirstOrDefault();
            if (business == null)
            {
                throw new CustomException("Business does not exist");
            }

            if (follower.Id == business.Id)
            {
                throw new CustomException("You cannot follow yourself");
            }

            var followTime = DateTime.Now;
            var followerRecord = new Follow {AccountId = followerId, Time = followTime};
            var followeeRecord = new Follow {AccountId = businessId, Time = followTime};
            business.Followers.Add(followerRecord);
            follower.Followees.Add(followeeRecord);

            business.ModifiedDate = DateTime.Now;

            _accountCollection.UpdateOne(f => f.Id == business.Id,
                Builders<Account>.Update.AddToSet(f => f.Followers, followerRecord)
                    .Set(a => a.ModifiedDate, business.ModifiedDate));
            _accountCollection.UpdateOne(f => f.Id == follower.Id,
                Builders<Account>.Update.AddToSet(f => f.Followees, followeeRecord).Set(a => a.ModifiedDate,
                    datefollow.HasValue ? datefollow.Value : DateTime.Now));
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(business.AccountId, "FollowBusiness");
            adminS.syncaccount(follower.AccountId, "FollowBusiness");
            return business;
        }

        public Account UnfollowBusiness(ObjectId unfollowerId, ObjectId businessId)
        {
            var unfollower = _accountCollection.Find(f => f.Id == unfollowerId).FirstOrDefault();
            if (unfollower == null)
            {
                throw new CustomException("Follower does not exist");
            }

            var business = _accountCollection.Find(f => f.Id == businessId && f.AccountType == AccountType.Business)
                .FirstOrDefault();
            if (business == null)
            {
                throw new CustomException("Business does not exist");
            }

            if (unfollower.Id == business.Id)
            {
                throw new CustomException("You cannot unfollow yourself");
            }

            business.ModifiedDate = DateTime.Now;
            business.Followers.RemoveAll(f => f.AccountId == unfollower.Id);
            unfollower.Followees.RemoveAll(f => f.AccountId == business.Id);

            _accountCollection.UpdateOne(f => f.Id == business.Id,
                Builders<Account>.Update.PullFilter(f => f.Followers, f => f.AccountId == unfollower.Id)
                    .Set(a => a.ModifiedDate, business.ModifiedDate));
            _accountCollection.UpdateOne(f => f.Id == unfollower.Id,
                Builders<Account>.Update.PullFilter(f => f.Followees, f => f.AccountId == business.Id)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(business.AccountId, "FollowBusiness");

            return business;
        }


        public void SetSMSAuthenticated(string userId)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Session[userId] = true;
            }
        }

        public bool IsSMSAuthenticated(string userId)
        {
            if (HttpContext.Current != null)
            {
                var value = HttpContext.Current.Session[userId];
                return value == null ? false : Convert.ToBoolean(value);
            }

            return false;
        }

        public void ClearSession(string userId)
        {
            if (HttpContext.Current.Session[userId] != null)
            {
                HttpContext.Current.Session.Remove(userId);
            }
        }


        public Account UpdateEmail(string email, ObjectId account, GreenHouseDbContext sqlDb)
        {
            var exist = _accountCollection.Find(a => a.Id == account).FirstOrDefault();
            if (exist == null)
            {
                return null;
            }

            if (_accountCollection.Find(a => a.Profile.Email.ToLower() == email.ToLower() && a.Id != exist.Id).Any())
            {
                return null;
            }

            var identityAcc = sqlDb.Users.Find(exist.AccountId);
            identityAcc.Email = email;
            identityAcc.UserName = email;
            sqlDb.SaveChanges();

            exist.Profile.Email = email;
            exist.ModifiedDate = DateTime.Now;
            _accountCollection.UpdateOne(a => a.Id == exist.Id,
                Builders<Account>.Update.Set(a => a.Profile.Email, email).Set(a => a.ModifiedDate, exist.ModifiedDate));
            // Sync to Admin
            var adminS = new AdminService();
            adminS.syncaccount(exist.AccountId, "UpdateEmail");
            return exist;
        }

        public void UpdateOtp(string value, ObjectId account)
        {
            _accountCollection.UpdateOne(a => a.Id == account,
                Builders<Account>.Update.Set(a => a.PhoneVerifiedToken, value)
                    .Set(a => a.ModifiedDate, DateTime.Now));
        }

        public void UpdateWebsite(string website, ObjectId account)
        {
            _accountCollection.UpdateOne(a => a.Id == account,
                Builders<Account>.Update.Set(a => a.CompanyDetails.Website, website)
                    .Set(a => a.ModifiedDate, DateTime.Now));
            // Sync to Admin
            //var adminS = new AdminService();
            //adminS.syncaccount(exist.AccountId, "UpdateWebsite");
        }

        public void UpdateResetPasswordToken(ObjectId account, ObjectId? verifyTokenId)
        {
            _accountCollection.UpdateOne(a => a.Id == account,
                Builders<Account>.Update.Set(a => a.ResetPasswordToken, verifyTokenId)
                    .Set(a => a.ModifiedDate, DateTime.Now));
        }

        public SearchUserResult SearchUsers(SearchUserParameter searchModel)
        {
            ObjectId objectId;
            var builders = Builders<Account>.Filter;

            var filter =
                builders.Regex("Profile.FirstName",
                    BsonRegularExpression.Create(new Regex(searchModel.SearchText, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.LastName",
                    BsonRegularExpression.Create(new Regex(searchModel.SearchText, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.DisplayName",
                    BsonRegularExpression.Create(new Regex(searchModel.SearchText, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.Email",
                    BsonRegularExpression.Create(new Regex(searchModel.SearchText, RegexOptions.IgnoreCase))) |
                builders.Regex("CompanyDetails.CompanyName",
                    BsonRegularExpression.Create(new Regex(searchModel.SearchText, RegexOptions.IgnoreCase)));

            if (ObjectId.TryParse(searchModel.SearchText, out objectId))
            {
                filter = filter | builders.Eq("_id", objectId);
            }

            filter = filter & builders.Eq("AccountType", searchModel.AccountType);
            if (searchModel.AccountType == AccountType.Business && searchModel.FilterType == FilterType.UnVerify)
            {
                filter = filter & builders.Eq("BusinessAccountVerified", false);
            }

            var accounts = _accountCollection.Find(filter);
            return new SearchUserResult
            {
                Total = accounts.Count(),
                Accounts = accounts.Skip(searchModel.Start * searchModel.Length).Limit(searchModel.Length).ToList()
            };
        }

        public FolloweeResult GetFollowByListUserId(FolloweeParameter parameter)
        {
            var builders = Builders<Account>.Filter;
            var skip = parameter.Start * parameter.Length;
            var followees = parameter.Account.Followees.Select(x => x.AccountId).Skip(skip).Take(parameter.Length);
            var filter = builders.In(x => x.Id, followees);

            var result = new FolloweeResult
            {
                Followee = _accountCollection.Find(filter).ToList(),
                Total = parameter.Account.Followees.Count
            };
            return result;
        }

        public Account UpdateNotificationSettings(AccountNotificationSettings settings, string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.AccountId == accountId).FirstOrDefault();

            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.NotificationSettings = settings;
            existAccount.ModifiedDate = DateTime.Now;

            var update = Builders<Account>.Update.Set(a => a.NotificationSettings, existAccount.NotificationSettings)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);

            return existAccount;
        }

        public Account UpdateBusinessPrivacy(BusinessPrivacy privacy, string accountId)
        {
            var existAccount = _accountCollection.Find(a => a.Id == accountId.ParseToObjectId()).FirstOrDefault();
            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.BusinessPrivacies = privacy;
            existAccount.ModifiedDate = DateTime.Now;
            var update = Builders<Account>.Update.Set(a => a.BusinessPrivacies, existAccount.BusinessPrivacies)
                .Set(a => a.ModifiedDate, existAccount.ModifiedDate);
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id, update);

            return existAccount;
        }

        public void UpdateLanguage(ObjectId accountId, string languageCode)
        {
            var existAccount = _accountCollection.Find(a => a.Id == accountId).FirstOrDefault();
            if (existAccount == null)
            {
                throw new CustomException(new ErrorViewModel {Message = "Account does not exist"});
            }

            existAccount.Language = languageCode;
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id,
                Builders<Account>.Update.Set(a => a.Language, languageCode));
        }

        public VerifyEmailViewModel SendVerifyEmail(Account account, string link)
        {
            if (account.EmailVerified)
            {
                return new VerifyEmailViewModel
                {
                    Email = account.Profile.Email,
                    Type = VerifyType.AccountIsVerified,
                    Message = Regit.Account_Is_Verified
                };
            }

            _accountCollection.UpdateOne(a => a.Id == account.Id,
                Builders<Account>.Update.Set(a => a.EmailVerifyToken, account.EmailVerifyToken)
                    .Set(a => a.EmailVerifyTokenDate, account.EmailVerifyTokenDate));

            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_ActivateAccount.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_ActivateAccount.html");
            }

            string emailContent = string.Empty;

            try
            {
                if (File.Exists(emailTemplate))
                {
                    emailContent = File.ReadAllText(emailTemplate);

                    emailContent = emailContent.Replace("{{Url}}", link);

                    IMailService mailService = new MailService();
                    mailService.SendMailAsync(new NotificationContent
                    {
                        Title = "Regit new account email verification",
                        Body = string.Format(emailContent, ""),
                        SendTo = new[] {account.Profile.Email}
                    });
                }
            }
            catch (Exception ex)
            {
                return new VerifyEmailViewModel
                {
                    Email = account.Profile.Email,
                    Type = VerifyType.Exception,
                    Message = ex.Message
                };
            }

            return new VerifyEmailViewModel
            {
                Email = account.Profile.Email,
                Type = VerifyType.SendMailSuccessfully,
                Message = Regit.Send_Email_Verify_Is_Completed
            };
        }

        public VerifyEmailViewModel VerifyEmail(string email, string token)
        {
            var existAccount = _accountCollection.Find(a => a.Profile.Email.ToLower() == email.ToLower())
                .FirstOrDefault();
            if (existAccount == null)
            {
                return new VerifyEmailViewModel
                {
                    Email = string.Empty,
                    Type = VerifyType.AccountNotFound,
                    Message = Regit.Account_Not_Found_Message
                };
            }

            if (existAccount.EmailVerified)
            {
                return new VerifyEmailViewModel
                {
                    Email = existAccount.Profile.Email,
                    Type = VerifyType.AccountIsVerified,
                    Message = Regit.Account_Is_Verified
                };
            }

            var expiredAfter = int.Parse(ConfigurationManager.AppSettings["VerifyEmailExpiredAfter"]);
            if (existAccount.EmailVerifyTokenDate != null &&
                (existAccount.EmailVerifyToken != token ||
                 existAccount.EmailVerifyTokenDate.Value.AddDays(expiredAfter) < DateTime.Now))
            {
                return new VerifyEmailViewModel
                {
                    Email = existAccount.Profile.Email,
                    Type = VerifyType.SessionIsExpired,
                    Message = Regit.Session_Verify_Is_Expired
                };
            }

            existAccount.EmailVerified = true;
            _accountCollection.UpdateOne(a => a.Id == existAccount.Id,
                Builders<Account>.Update.Set(a => a.EmailVerified, true));
            return new VerifyEmailViewModel
            {
                Email = existAccount.Profile.Email,
                Type = VerifyType.VerifySuccessfully,
                Message = Regit.Verify_Is_Completed
            };
        }

        public IEnumerable<Account> FindUsersByKeyword(string keyword)
        {
            ObjectId objectId;
            var builders = Builders<Account>.Filter;

            var filter =
                builders.Regex("Profile.FirstName",
                    BsonRegularExpression.Create(new Regex(keyword, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.LastName",
                    BsonRegularExpression.Create(new Regex(keyword, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.DisplayName",
                    BsonRegularExpression.Create(new Regex(keyword, RegexOptions.IgnoreCase))) |
                builders.Regex("Profile.Email",
                    BsonRegularExpression.Create(new Regex(keyword, RegexOptions.IgnoreCase))) |
                builders.Regex("CompanyDetails.CompanyName",
                    BsonRegularExpression.Create(new Regex(keyword, RegexOptions.IgnoreCase)));

            if (ObjectId.TryParse(keyword, out objectId))
            {
                filter = filter | builders.Eq("_id", objectId);
            }

            var accounts = _accountCollection.Find(filter);
            var limit = int.Parse(ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"]);

            return accounts.Limit(limit).ToList();
        }

        public Task<UpdateResult> ManualVerifyAccount(IEnumerable<ObjectId> accountIds)
        {
            var builders = Builders<Account>.Filter;
            var filter = builders.In(x => x.Id, accountIds);
            var result = _accountCollection.UpdateManyAsync(filter,
                Builders<Account>.Update.Set(a => a.BusinessAccountVerified, true).CurrentDate(x => x.ModifiedDate));
            return result;
        }

        public string GetCompanyDetailsByAccountId(string accountId)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("AccountId", accountId);
            var accountcurrent = accountCollection.Find(criteria).FirstOrDefault();
            var objectCompanyDetail = accountcurrent["CompanyDetails"];
            return objectCompanyDetail.ToJson();
        }

        public void UpdateCompanyDetailsByAccountId(string accountId, string companyDetails)
        {
            //  var bs = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(companyDetails);
            var accountCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("AccountId", accountId);
            BsonDocument bsCompanyDetails =
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(companyDetails);
            var update = Builders<BsonDocument>.Update.Set("CompanyDetails", bsCompanyDetails);
            accountCollection.UpdateOne(criteria, update);
        }

        public string GetProfileByAccountId(string accountId)
        {
            var accountCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("AccountId", accountId);
            var accountcurrent = accountCollection.Find(criteria).FirstOrDefault();
            var objectCompanyDetail = accountcurrent["Profile"];
            return objectCompanyDetail.ToJson();
        }

        public void UpdateProfileByAccountId(string accountId, string profileString)
        {
            //  var bs = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(companyDetails);
            var accountCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("AccountId", accountId);
            BsonDocument bsProfile = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(profileString);
            var update = Builders<BsonDocument>.Update.Set("Profile", bsProfile);
            accountCollection.UpdateOne(criteria, update);
        }

        public string GetRoleById(ObjectId? id)
        {
            var rs = "";
            if (id != null)
            {
                var role = _roleCollection.Find(r => r.Id == id).FirstOrDefault();
                rs = role.Name;
            }

            return rs;
        }

        public Role GetRoleByName(string name)
        {
            // var joinInvite = new JoiningBusinessInvitationService();
            var role = new Role();
            if (!string.IsNullOrEmpty(name))
            {
                role = _roleCollection.Find(r => r.Name.Equals(name)).FirstOrDefault();
            }

            return role;
        }

        public List<UserRole> GetUserRolePending(List<JoiningBusinessInvitation> lstRolesPending)
        {
            var rs = new List<UserRole>();
            if (lstRolesPending == null)
                return rs;
            foreach (var rp in lstRolesPending)
            {
                var role = new UserRole();
                var user = GetById(rp.To);
                var roles = rp.Roles;
                for (var i = 0; i < roles.Count; i++)
                {
                    role = new UserRole();
                    role.AccountId = user.AccountId;
                    string roleName = (roles[i] == "admin" ? "Administrator" :
                        roles[i] == "editor" ? "Editor" : "Reviewer");
                    var rl = GetRoleByName(roleName);
                    role.RoleId = rl.Id;
                    role.RoleName = rl.Name;
                    role.Status = EnumRoleStatus.Pending;
                    role.SentAt = rp.SentAt;
                    if (!string.IsNullOrEmpty(role.RoleName))
                        rs.Add(role);
                }
            }

            return rs;
        }

        public void FollowRegit(string userId)
        {
            try
            {
                var mailRegit = ConfigurationManager.AppSettings["BusinessRegit"].ToString();
                var businessRegit = this.GetByEmail(mailRegit);
                var currentUser = this.GetByAccountId(userId);
                if (businessRegit == null)
                {
                    throw new CustomException("Business profile does not exist");
                }


                if (currentUser == null)
                {
                    throw new CustomException("User does not exist");
                }

                var businessAccount = this.FollowBusiness(currentUser.Id, businessRegit.Id);
                new BusinessMemberLogic().AddBusinessMember(currentUser, businessAccount);
                _transactionService.InsertTransaction(new FollowTransactionParamter
                {
                    FromUser = currentUser.Id,
                    ToUser = businessRegit.Id,
                    User = currentUser,
                    Date = DateTime.Now.Date,
                    Type = FollowType.Follow
                });
            }
            catch { }
        }


        public bool IsConfirmSMS(string userId)
        {
            if (!ConfigHelp.GetBoolValue("IsEnableSMSAuthencation"))
                return true;
            if (string.IsNullOrEmpty(userId))
                return false;

            return this.IsSMSAuthenticated(userId);
        }
    }
}