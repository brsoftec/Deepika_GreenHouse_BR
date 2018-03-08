using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using GH.Core.BlueCode.Entity.Notification;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Helpers;
using RegitSocial.Business.Notification;

namespace GH.Core.Services
{
    public class JoiningBusinessInvitationService : IJoiningBusinessInvitationService
    {
        IMongoCollection<Account> _accountCollection;
        IMongoCollection<JoiningBusinessInvitation> _invitationCollection;
        IMongoCollection<Role> _roleCollection;

        public JoiningBusinessInvitationService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _invitationCollection = db.JoiningBusinessInvitations;
            _roleCollection = db.Roles;
        }

        public void AcceptInvitation(ObjectId invitationId)
        {
            var invitation = new JoiningBusinessInvitation();
            try
            {
                invitation = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            }
            catch
            {
            }
            if (invitation == null)
            {
                throw new CustomException("Invitation does not exist");
            }

            var businessAccount = _accountCollection.Find(a => a.Id == invitation.From).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business does not exist");
            }
            var personalAccount = _accountCollection.Find(a => a.Id == invitation.To).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("User does not exist");
            }

            var editorRole = _roleCollection.Find(r => r.Name == Role.ROLE_EDITOR).FirstOrDefault();
            var adminRole = _roleCollection.Find(r => r.Name == Role.ROLE_ADMIN).FirstOrDefault();
            var approverRole = _roleCollection.Find(r => r.Name == Role.ROLE_REVIEWER).FirstOrDefault();

            if (editorRole == null || adminRole == null || approverRole == null)
            {
                throw new CustomException("Role does not exist");
            }

            var accountId = personalAccount.Id;

            foreach (string roleName in invitation.Roles)
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

                var businessRole = new BusinessAccountRole
                {
                    AccountId = accountId,
                    RoleId = role.Id
                };
                var personRole = new BusinessAccountRole
                {
                    AccountId = businessAccount.Id,
                    RoleId = role.Id
                };
                businessAccount.BusinessAccountRoles.Add(businessRole);
                _accountCollection.UpdateOne(a => a.Id == businessAccount.Id,
                    Builders<Account>.Update.AddToSet(x => x.BusinessAccountRoles, businessRole));
                _accountCollection.UpdateOne(a => a.Id == accountId,
                    Builders<Account>.Update.AddToSet(x => x.BusinessAccountRoles, personRole));
            }


            _invitationCollection.DeleteOne(i => i.Id == invitationId);

            string title = "You accepted to be workflow member to " + businessAccount.Profile.DisplayName;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.WorkflowAcceptInvitation;
            notificationMessage.FromAccountId = personalAccount.AccountId;
            notificationMessage.FromUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = businessAccount.AccountId;
            notificationMessage.ToUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invitation.Id;

            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void Accept(ObjectId invitationId, ObjectId accountId)
        {
            var invitation = new JoiningBusinessInvitation();
            try
            {
                invitation = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            }
            catch
            {
            }
            if (invitation == null)
            {
                throw new CustomException("Invitation does not exist");
            }
            else if (invitation.To != accountId)
            {
                throw new CustomException("Account cannot accept the invitation");
            }

            var businessAccount = _accountCollection.Find(a => a.Id == invitation.From).FirstOrDefault();
            var editorRole = _roleCollection.Find(r => r.Name == Role.ROLE_EDITOR).FirstOrDefault();

            //if (editorRole == null)
            //{
            //    throw new CustomException("editorRole does not exist");
            //}
            //if (businessAccount == null)
            //{
            //    throw new CustomException("businessAccount does not exist");
            //}
            var newLink = new BusinessAccountRole
            {
                AccountId = accountId,
                RoleId = editorRole.Id
            };
            businessAccount.BusinessAccountRoles.Add(newLink);

            _accountCollection.UpdateOne(a => a.Id == businessAccount.Id,
                Builders<Account>.Update.AddToSet(x => x.BusinessAccountRoles, newLink));
            _accountCollection.UpdateOne(a => a.Id == accountId,
                Builders<Account>.Update.AddToSet(x => x.BusinessAccountRoles,
                    new BusinessAccountRole {AccountId = businessAccount.Id, RoleId = editorRole.Id}));
            _invitationCollection.DeleteOne(i => i.Id == invitationId);
        }

        public void DenyInvitation(ObjectId invitationId)
        {
            var invitation = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invitation == null)
            {
                throw new CustomException("Invitation does not exist");
            }
//            else if (invitation.To != accountId)
//            {
//                throw new CustomException("Account cannot deny the invitation");
//            }
            var businessAccount = _accountCollection.Find(a => a.Id == invitation.From).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business does not exist");
            }
            var personalAccount = _accountCollection.Find(a => a.Id == invitation.To).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("User does not exist");
            }

            _invitationCollection.DeleteOne(i => i.Id == invitationId);

            string title = "You denied workflow invitation from " + businessAccount.Profile.DisplayName;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.WorkflowDenyInvitation;
            notificationMessage.FromAccountId = personalAccount.AccountId;
            notificationMessage.FromUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = businessAccount.AccountId;
            notificationMessage.ToUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invitation.Id;

            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void Deny(ObjectId invitationId, ObjectId accountId)
        {
            var invitation = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invitation == null)
            {
                throw new CustomException("Invitation does not exist");
            }
            else if (invitation.To != accountId)
            {
                throw new CustomException("Account cannot deny the invitation");
            }
        }

        public void RemoveInvitation(ObjectId invitationId)
        {
            var invitation = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invitation == null)
            {
                throw new CustomException("Invitation does not exist");
            }
//            else if (invitation.To != accountId)
//            {
//                throw new CustomException("Account cannot deny the invitation");
//            }

            var businessAccount = _accountCollection.Find(a => a.Id == invitation.From).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business does not exist");
            }
            var personalAccount = _accountCollection.Find(a => a.Id == invitation.To).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("User does not exist");
            }

            _invitationCollection.DeleteOne(i => i.Id == invitationId);

            string title = "You cancelled workflow invitation to " + personalAccount.Profile.DisplayName;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.WorkflowCancelInvitation;
            notificationMessage.FromAccountId = businessAccount.AccountId;
            notificationMessage.FromUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = personalAccount.AccountId;
            notificationMessage.ToUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invitation.Id;

            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public List<JoiningBusinessInvitation> GetAllJoiningBusinessInivations(ObjectId accountId)
        {
            return _invitationCollection.Find(i => i.To == accountId).ToList();
        }

        public List<JoiningBusinessInvitation> GetAllWorkflowInvitationsFromBusiness(ObjectId businessId)
        {
            return _invitationCollection.Find(i => i.From == businessId).ToList();
        }

      
        public JoiningBusinessInvitation Invite(ObjectId from, ObjectId to, List<string> roles, string inviteId = null)
        {
            var businessAccount = _accountCollection.Find(a => a.Id == from).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            }
            else if (businessAccount.AccountType != AccountType.Business)
            {
                throw new CustomException("Invitation sender is not business account");
            }

            var personalAccount = _accountCollection.Find(a => a.Id == to).FirstOrDefault();
            if (personalAccount == null)
            {
                throw new CustomException("Personal account does not exist");
            }
            else if (personalAccount.AccountType != AccountType.Personal)
            {
                throw new CustomException("Invitation receiver is not personal account");
            }

            if (_invitationCollection.Find(i => i.From == from && i.To == to).Any())
            {
                throw new CustomException("Personal account has already recevied an invitation");
            }

            if (businessAccount.BusinessAccountRoles.Any(a => a.AccountId == personalAccount.Id))
            {
                throw new CustomException("Personal account has already joined business");
            }

            bool invited = inviteId != null;

            var invitation = new JoiningBusinessInvitation
            {
                Id = ObjectId.GenerateNewId(),
                From = from,
                To = to,
                Roles = roles,
                SentAt = DateTime.Now
            };
            if (invited)
            {
                invitation.InviteId = inviteId;
            }
            _invitationCollection.InsertOne(invitation);

            var roleNames = string.Join(", ", roles);
            CultureInfo culture_info = Thread.CurrentThread.CurrentCulture;
            TextInfo text_info = culture_info.TextInfo;
            roleNames = text_info.ToTitleCase(roleNames);


            string title = "You invited " + personalAccount.Profile.DisplayName + " to your business workflow";
            if (invited) title += " (following an invitation by email)";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = invited
                ? EnumNotificationType.WorkflowInviteMemberFollowEmail
                : EnumNotificationType.WorkflowInviteMember;
            notificationMessage.FromAccountId = businessAccount.AccountId;
            notificationMessage.FromUserDisplayName = businessAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = personalAccount.AccountId;
            notificationMessage.ToUserDisplayName = personalAccount.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = (string)invitation.Id.ToString();
            notificationMessage.Payload = roleNames;
            if (invited)
            {
                notificationMessage.Options = inviteId;
            }

            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);

            return invitation;
        }

        public List<Account> SearchUserForInvitation(string keyword, ObjectId businessAccountId, int? start = null,
            int? length = null)
        {
            var ignoreIds =
                _accountCollection.Find(a => a.AccountType == AccountType.Personal && a.BusinessAccountRoles.Any())
                    .Project(a => a.Id)
                    .ToList();

            if (length == null)
            {
                length = ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"].ParseInt();
            }

            keyword = keyword.ToLower();
            return
                _accountCollection.Find(
                        a =>
                            a.AccountType == AccountType.Personal && !ignoreIds.Contains(a.Id) &&
                            (a.Profile.DisplayName.ToLower().Contains(keyword) || a.Profile.Email.ToLower() == keyword))
                    .SortBy(a => a.Profile.DisplayName)
                    .Skip(start)
                    .Limit(length)
                    .ToList();
        }
    }
}