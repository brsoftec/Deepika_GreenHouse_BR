using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using GH.Core.Services;
using GH.Core.ViewModels;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using RegitSocial.Business.Notification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class DelegationBusinessLogic : IDelegationBusinessLogic
    {
        private IAccountService accountService = new AccountService();
        private Logger log = LogManager.GetCurrentClassLogger();

        public BsonDocument GetDelegationItemTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "delegationItemTemplate");
            var result = settingCollection.Find(filter).FirstOrDefault();
            var delegationItemTemplateBson = result["value"].AsBsonDocument;
            return delegationItemTemplateBson;
        }

        public DelegationItemTemplate GetDelegationById(string accountId, string delegationId)
        {
            var account = accountService.GetByAccountId(accountId);
            return account.Delegations.Single(d => d.DelegationId.Equals(delegationId));
        }

        public DelegationItemTemplate GetDelegationById(string delegationId)
        {
            //MongoRepository<Account> accountCollection = new MongoRepository<Account>();
            /*
            var ownerAccount = accountService.Search(a => a.Delegations != null && a.Delegations.Any(d => d.DelegationId.Equals(delegationId))).SingleOrDefault();
            if(ownerAccount != null)
            {
                return ownerAccount.Delegations.SingleOrDefault(d => d.DelegationId.Equals(delegationId));
            }
            */
            return null;
        }

        public IEnumerable<DelegationItemTemplate> GetDelegationList(string accountId, string delegateDirection = "All")
        {
            var account = accountService.GetByAccountId(accountId);
            if (account != null && account.Delegations != null && account.Delegations.Count() > 0)
            {
                if (!string.IsNullOrEmpty(delegateDirection) && !delegateDirection.ToLower().Equals("all"))
                    return account.Delegations.Where(d => d.Direction.Equals(delegateDirection)).ToList();
                else
                    return account.Delegations;
            }

            return null;
        }

        public FuncResult GetUserDelegations(Account account, string direction = "DelegationIn")
        {
            if (account.Delegations == null || !account.Delegations.Any())
                return new FuncResult(false, "notFound");

            direction = direction?.ToLower();

            List<DelegationItemTemplate> items;
            if (string.IsNullOrEmpty(direction) || direction.Equals("all"))
                items = account.Delegations.ToList();
            else
                items = account.Delegations.Where(d => d.Direction.ToLower().Equals(direction)).ToList();
//                                                       && d.DelegationRole != "Emergency").ToList();
            if (!items.Any()) return new FuncResult(false, "notFound");

            var delegations = items.Select(d =>
            {
                var delegator = accountService.GetByAccountId(d.FromAccountId);
                return new UserDelegation
                {
                    DelegationId = d.DelegationId,
                    DelegationRole = d.DelegationRole,
                    Direction = d.Direction,
                    FromAccountId = d.FromAccountId,
                    FromProfile = new DelegationProfile
                    {
                        Id = delegator.Id.ToString(),
                        DisplayName = delegator.Profile.DisplayName,
                        Avatar = delegator.Profile.PhotoUrl
                    },
                    ToAccountId = d.ToAccountId,
                    Status = d.Status.ToLower(),
                    Permissions = d.DelegationRole == "Normal" || d.DelegationRole == "Super"
                        ? null
                        : d.GroupVaultsPermission,

                    Begins = d.EffectiveDate,
                    Expires = d.ExpiredDate
                };
            });


            return new FuncResult(true, "delegation.list", delegations);
        }

        public FuncResult GetUserDelegation(string delegationId, Account account)
        {
            if (string.IsNullOrEmpty(delegationId))
                return new FuncResult(false, "missing.id");

            var del = account.Delegations.FirstOrDefault(d => d.DelegationId.Equals(delegationId));

            if (del == null) return new FuncResult(false, "notFound");
            
            var delegator = accountService.GetByAccountId(del.FromAccountId);
            var delegatee = accountService.GetByAccountId(del.ToAccountId);

            var delegation = new UserDelegation
            {
                DelegationId = del.DelegationId,
                DelegationRole = del.DelegationRole,
                Direction = del.Direction,
                FromAccountId = del.FromAccountId,
                FromProfile = new DelegationProfile
                {
                    Id = delegator.Id.ToString(),
                    DisplayName = delegator.Profile.DisplayName,
                    Avatar = delegator.Profile.PhotoUrl
                },
                ToAccountId = del.ToAccountId,
                ToProfile = new DelegationProfile
                {
                    Id = delegatee.Id.ToString(),
                    DisplayName = delegatee.Profile.DisplayName,
                    Avatar = delegatee.Profile.PhotoUrl
                },
                Status = del.Status.ToLower(),
                Permissions = del.DelegationRole == "Custom" ? del.GroupVaultsPermission : null,
                Begins = del.EffectiveDate,
                Expires = del.ExpiredDate
            };

            return new FuncResult(true, "delegation.details", delegation);
        }

        public void RequestDelegation(string fromAccountId, DelegationItemTemplate delegationMessage)
        {
            var accountService = new AccountService();
            var fromAccount = accountService.GetByAccountId(fromAccountId);
            var toAccountId = delegationMessage.ToAccountId;
            var toAccount = accountService.GetByAccountId(toAccountId);

            bool inviteUserInsideSystem = !string.IsNullOrEmpty(toAccountId);

            if (fromAccount == null ||
                (toAccount == null && string.IsNullOrEmpty(delegationMessage.InvitedEmail))) return;


            //Add delegation for FromUser
            delegationMessage.DelegationId = BsonHelper.GenerateObjectIdString();
            delegationMessage.FromAccountId = fromAccountId;
            delegationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
            delegationMessage.Direction = EnumDelegationDirection.DelegationOut;
            delegationMessage.Status = EnumDelegationStatus.Pending;
            //delegationMessage.InvitedEmail = toAccount.Profile.Email;
            if (inviteUserInsideSystem)
            {
                delegationMessage.ToUserDisplayName = toAccount.Profile.DisplayName;
            }

            List<DelegationItemTemplate> delegations = null;
            if (fromAccount.Delegations == null) delegations = new List<DelegationItemTemplate>();
            else delegations = fromAccount.Delegations.ToList();
            delegations.Add(delegationMessage);
            fromAccount.Delegations = delegations;
            accountService.UpdateDelegation(delegations, fromAccountId);

            if (inviteUserInsideSystem)
            {
                //Send to Friend
                //add delegation record for Friend
                delegationMessage.Direction = EnumDelegationDirection.DelegationIn;
                if (toAccount.Delegations == null) delegations = new List<DelegationItemTemplate>();
                else delegations = toAccount.Delegations.ToList();
                delegations.Add(delegationMessage);
                accountService.UpdateDelegation(delegations, toAccountId);


                //Send notification
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.DelegationRequest;
                notificationMessage.FromAccountId = fromAccountId;
                notificationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = toAccountId;
                notificationMessage.ToUserDisplayName = toAccount.Profile.DisplayName;
                notificationMessage.Content = delegationMessage.Message;
                notificationMessage.PreserveBag = delegationMessage.DelegationId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }


            //Invite user outside of Regit
            if (!string.IsNullOrEmpty(delegationMessage.InvitedEmail) &&
                string.IsNullOrEmpty(delegationMessage.ToAccountId))
            {
                InviteUserOutsideForDelegation(fromAccountId, delegationMessage.InvitedEmail,
                    delegationMessage.DelegationId);
            }
        }

        private void InviteUserOutsideForDelegation(string fromAccountId, string toEmail, string withDelegationId)
        {
            var emailTemplate = string.Empty;
            if (HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath(
                        "/EmailTemplates/EmailTemplate_InviteUserOutsideForDelegation.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir,
                    @"EmailTemplates\EmailTemplate_InviteUserOutsideForDelegation.html");
            }

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var fromAccount = accountService.GetByAccountId(fromAccountId);
                var fullName = fromAccount.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/user/signup?invitedemail={1}&delegationid={2}", baseUrl, toEmail,
                    withDelegationId);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                var subject = string.Format("{0} invites you to join Regit Social", fullName);
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                });

                // Util.EmailHelper.SendEmail("Regit Social", toEmail, subject, emailContent, true, true);
            }
        }

        public FuncResult AcceptDelegation(string accountId, string delegationId)
        {
            var delegateeAccount = accountService.GetByAccountId(accountId);

            List<DelegationItemTemplate> delegations = null;
            if (delegateeAccount.Delegations != null)
            {
                //Update delegation status for delegatEE
                delegations = delegateeAccount.Delegations.ToList();
                var delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                if (delegation == null)
                    return new FuncResult(false, "delegation.notFound");

                delegation.Status = EnumDelegationStatus.Accepted;
                accountService.UpdateDelegation(delegations, accountId);


                //Update delegation status for delegatOR
                var delegatorAccount = accountService.GetByAccountId(delegation.FromAccountId);

                if (delegatorAccount != null)
                {
                    log.Debug("delegation.FromAccountId:  " + delegation.FromAccountId);
                    delegations = delegatorAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegation.Status = EnumDelegationStatus.Accepted;
                        accountService.UpdateDelegation(delegations, delegation.FromAccountId);
                    }

                    //Update to trusted network
                    var networkBus = new NetworkService();
                    var trustedFriends =
                        networkBus.GetNetworkFriends(delegatorAccount.AccountId, Network.TRUSTED_NETWORK);
                    log.Debug("trustedFriends not null:  " + delegatorAccount.AccountId);
                    if (trustedFriends != null)
                    {
                        log.Debug("trustedFriends not null:  " + delegatorAccount.AccountId);
                        if (trustedFriends.FirstOrDefault(f => f.UserId.Equals(delegateeAccount.AccountId)) == null)
                        {
                            var trustedNetwork =
                                networkBus.GetNetworkByName(delegatorAccount.AccountId, Network.TRUSTED_NETWORK);
                            var normalNetwork =
                                networkBus.GetNetworkByName(delegatorAccount.AccountId, Network.NORMAL_NETWORK);
                            networkBus.MoveNetwork(delegatorAccount.Id, delegateeAccount.Id, normalNetwork.Id,
                                trustedNetwork.Id);
                        }
                    }
                    else
                    {
                        log.Debug("trustedFriends null:  " + delegatorAccount.AccountId);
                    }

                    //Send notification to delegatOR
                    var notificationMessage = new NotificationMessage();
                    notificationMessage.Id = ObjectId.GenerateNewId();
                    notificationMessage.Type = EnumNotificationType.DelegationAccept;
                    notificationMessage.FromAccountId = accountId;
                    notificationMessage.FromUserDisplayName = delegateeAccount.Profile.DisplayName;
                    notificationMessage.ToAccountId = delegatorAccount.AccountId;
                    notificationMessage.ToUserDisplayName = delegatorAccount.Profile.DisplayName;
                    notificationMessage.Content = delegation.Message;
                    notificationMessage.PreserveBag = delegation.DelegationId;
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessage);
                }

                return new FuncResult(true, "delegation.accepted");
            }
            else
            {
                return new FuncResult(false, "delegation.notAccepted");
            }
        }

        // Vu
        public void ActivatedDelegation(string accountId, string delegationId)
        {
            var delegateeAccount = accountService.GetByAccountId(accountId);
            List<DelegationItemTemplate> delegations = null;
            if (delegateeAccount.Delegations != null)
            {
                //Update delegation status for delegatEE
                delegations = delegateeAccount.Delegations.ToList();
                var delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                if (delegation != null)
                {
                    delegation.Status = EnumDelegationStatus.Activated;
                    accountService.UpdateDelegation(delegations, accountId);
                }

                //Update delegation status for delegatOR
                var delegatorAccount = accountService.GetByAccountId(delegation.FromAccountId);
                if (delegatorAccount != null)
                {
                    delegations = delegatorAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegation.Status = EnumDelegationStatus.Activated;
                        accountService.UpdateDelegation(delegations, delegation.FromAccountId);
                    }

                    //Update to trusted network
                    var networkBus = new NetworkService();
                    var trustedFriends =
                        networkBus.GetNetworkFriends(delegatorAccount.AccountId, Network.TRUSTED_NETWORK);
                    if (trustedFriends != null)
                    {
                        if (trustedFriends.FirstOrDefault(f => f.UserId.Equals(delegateeAccount.AccountId)) == null)
                        {
                            var trustedNetwork =
                                networkBus.GetNetworkByName(delegatorAccount.AccountId, Network.TRUSTED_NETWORK);
                            var normalNetwork =
                                networkBus.GetNetworkByName(delegatorAccount.AccountId, Network.NORMAL_NETWORK);
                            networkBus.MoveNetwork(delegatorAccount.Id, delegateeAccount.Id, normalNetwork.Id,
                                trustedNetwork.Id);
                        }
                    }

                    //Send notification to delegatOR
                    var notificationMessage = new NotificationMessage();
                    notificationMessage.Id = ObjectId.GenerateNewId();
                    notificationMessage.Type = EnumNotificationType.DelegationActivated;
                    notificationMessage.FromAccountId = accountId;
                    notificationMessage.FromUserDisplayName = delegateeAccount.Profile.DisplayName;
                    notificationMessage.ToAccountId = delegatorAccount.AccountId;
                    notificationMessage.ToUserDisplayName = delegatorAccount.Profile.DisplayName;
                    notificationMessage.Content = delegation.Message;
                    notificationMessage.PreserveBag = delegation.DelegationId;
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessage);

                    //Add Activity Log
                    /*
                    var activityLogBus = new ActivityLogBusinessLogic();
                    var activityLog = new ActivityLog();
                    activityLog.Id = ObjectId.GenerateNewId();
                    activityLog.DateTime = DateTime.Today.ToString("yyyy-MM-dd-hh-mm-ss");
                    activityLog.ActivityType = EnumActivityType.DelegationAccept;
                    activityLog.Title = "You have accepted a delegation request from [user]";
                    activityLog.FromUserId = accountId;
                    activityLog.FromUserName = userName;
                    activityLog.ToUserId = delegatorId;
                    activityLog.ToUserEmail = delegatorAccount.Email;
                    activityLog.ToUserName = delegatorName;
                    activityLog.TargetOjectId = delegation.DelegationId;
                    activityLogBus.AddActivityLog(activityLog);
                    */
                }
            }
            else
            {
                //Throw ERROR
            }
        }

        public FuncResult DenyDelegation(string accountId, string delegationId)
        {
            var delegateeAccount = accountService.GetByAccountId(accountId);
            List<DelegationItemTemplate> delegations = null;
            delegations = delegateeAccount.Delegations.ToList();
            var delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
            if (delegation == null)
                return new FuncResult(false, "delegation.notFound");
            var notiType = EnumNotificationType.DelegationDeny;
            if (delegation.Status != "Pending")
            {
                notiType = EnumNotificationType.DelegationRemove;
            }

            var fromAccount = accountService.GetByAccountId(delegation.FromAccountId);
            var toAccount = accountService.GetByAccountId(delegation.ToAccountId);
            if (delegation != null)
            {
                if (fromAccount != null)
                {
                    delegations = fromAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegations.Remove(delegation);
                        accountService.UpdateDelegation(delegations, delegation.FromAccountId);
                    }

                    //Noti
                    if (fromAccount.AccountId != accountId)
                    {
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = notiType;
                        notificationMessage.FromAccountId = toAccount.AccountId;
                        notificationMessage.FromUserDisplayName = toAccount.Profile.DisplayName;
                        notificationMessage.ToAccountId = fromAccount.AccountId;
                        notificationMessage.ToUserDisplayName = fromAccount.Profile.DisplayName;
                        notificationMessage.Content = delegation.Message;
                        notificationMessage.PreserveBag = delegation.DelegationId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                    }
                }

                //To
                if (toAccount != null)
                {
                    delegations = toAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegations.Remove(delegation);
                        accountService.UpdateDelegation(delegations, delegation.ToAccountId);
                    }

                    //Noti
                    if (toAccount.AccountId != accountId)
                    {
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = notiType;
                        notificationMessage.FromAccountId = fromAccount.AccountId;
                        notificationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
                        notificationMessage.ToAccountId = toAccount.AccountId;
                        notificationMessage.ToUserDisplayName = toAccount.Profile.DisplayName;
                        notificationMessage.Content = delegation.Message;
                        notificationMessage.PreserveBag = delegation.DelegationId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                    }
                }

                return new FuncResult(true, "delegation.denied");
            }
            else
            {
                return new FuncResult(false, "delegation.error");
            }
        }

        public void RemoveDelegation(string accountId, string delegationId)
        {
            var delegateeAccount = accountService.GetByAccountId(accountId);
            List<DelegationItemTemplate> delegations = null;
            delegations = delegateeAccount.Delegations.ToList();
            var delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
            var notiType = EnumNotificationType.DelegationRemove;
            var fromAccount = accountService.GetByAccountId(delegation.FromAccountId);
            var toAccount = accountService.GetByAccountId(delegation.ToAccountId);
            if (delegation != null)
            {
                if (fromAccount != null)
                {
                    delegations = fromAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegations.Remove(delegation);
                        accountService.UpdateDelegation(delegations, delegation.FromAccountId);
                    }

                    //Noti
                    if (fromAccount.AccountId != accountId)
                    {
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = notiType;
                        notificationMessage.FromAccountId = toAccount.AccountId;
                        notificationMessage.FromUserDisplayName = toAccount.Profile.DisplayName;
                        notificationMessage.ToAccountId = fromAccount.AccountId;
                        notificationMessage.ToUserDisplayName = fromAccount.Profile.DisplayName;
                        notificationMessage.Content = delegation.Message;
                        notificationMessage.PreserveBag = delegation.DelegationId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                    }
                }

                //To
                if (toAccount != null)
                {
                    delegations = toAccount.Delegations.ToList();
                    delegation = delegations.Where(d => d.DelegationId.Equals(delegationId)).FirstOrDefault();
                    if (delegation != null)
                    {
                        delegations.Remove(delegation);
                        accountService.UpdateDelegation(delegations, delegation.ToAccountId);
                    }

                    //Noti
                    if (toAccount.AccountId != accountId)
                    {
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = notiType;
                        notificationMessage.FromAccountId = fromAccount.AccountId;
                        notificationMessage.FromUserDisplayName = fromAccount.Profile.DisplayName;
                        notificationMessage.ToAccountId = toAccount.AccountId;
                        notificationMessage.ToUserDisplayName = toAccount.Profile.DisplayName;
                        notificationMessage.Content = delegation.Message;
                        notificationMessage.PreserveBag = delegation.DelegationId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                    }
                }
            }
            else
            {
                //Throw ERROR
            }
        }

        public bool checkIfInvitedEmailForDelegation(string delegatorAccountId, string delegateeEmail)
        {
            var accountBus = new AccountService();
            var account = accountBus.GetByAccountId(delegatorAccountId);
            if (account != null && account.Delegations != null)
            {
                var delegation = account.Delegations.FirstOrDefault(d =>
                    !string.IsNullOrEmpty(d.InvitedEmail) && d.InvitedEmail.ToLower().Equals(delegateeEmail));
                if (delegation != null) return true;
                else return false;
            }

            return false;
        }

        public void DeleteDelegation(string accountIdTo, string accountIdFrom)
        {
            var notiType = EnumNotificationType.DelegationRemove;
            try
            {
                var accountTo = accountService.GetByAccountId(accountIdTo);
                var accountFrom = accountService.GetByAccountId(accountIdFrom);
                var _delegationsTo = accountTo.Delegations.ToList();
                var _delegationsFrom = accountFrom.Delegations.ToList();


                if (_delegationsTo != null)
                {
                    _delegationsTo.RemoveAll(d =>
                        (d.FromAccountId == accountIdFrom && d.ToAccountId == accountIdTo) ||
                        (d.FromAccountId == accountIdTo && d.ToAccountId == accountIdFrom));
                    accountService.UpdateDelegation(_delegationsTo, accountIdTo);
                }

                if (_delegationsFrom != null)
                {
                    _delegationsFrom.RemoveAll(d =>
                        (d.FromAccountId == accountIdFrom && d.ToAccountId == accountIdTo) ||
                        (d.FromAccountId == accountIdTo && d.ToAccountId == accountIdFrom));
                    accountService.UpdateDelegation(_delegationsFrom, accountIdFrom);
                }
            }
            catch
            {
            }
        }
    }
}