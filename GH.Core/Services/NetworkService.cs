using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GH.Core.Helpers;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.DataAccess;
using GH.Core.ViewModels;
using System.Web;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Hosting;
using GH.Core.BlueCode.BusinessLogic;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace GH.Core.Services
{
    public class NetworkService : INetworkService
    {
        private IMongoCollection<Account> _accountCollection;
        private IMongoCollection<FriendInvitation> _invitationCollection;
        private IMongoCollection<Network> _networkCollection;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public NetworkService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _invitationCollection = db.FriendInvitations;
            _networkCollection = db.Networks;
        }

        public FuncResult Invite(ObjectId from, ObjectId to, string inviteId = null)
        {
            if (from == to)
            {
                return new FuncResult(false, "self");
            }

            bool invited = inviteId != null;

            var sender = _accountCollection.Find(a => a.Id == from).FirstOrDefault();
            var receiver = _accountCollection.Find(a => a.Id == to).FirstOrDefault();

            if (sender == null)
            {
                return new FuncResult(false, "noFrom");
            }

            if (receiver == null)
            {
                return new FuncResult(false, "noTo");
            }

            var exist = _invitationCollection.Find(i => i.From == sender.Id && i.To == receiver.Id).FirstOrDefault();
            if (exist != null)
            {
                return new FuncResult(false, "existing");
            }

            var reverseInvite = _invitationCollection.Find(i => i.From == receiver.Id && i.To == sender.Id)
                .FirstOrDefault();
            if (reverseInvite != null)
            {
                return new FuncResult(false, "existingMutual");
            }

            FriendInvitation invite = new FriendInvitation
            {
                Id = ObjectId.GenerateNewId(),
                From = sender.Id,
                To = receiver.Id,
                SentAt = DateTime.Now
            };
            if (invited)
            {
                invite.InviteId = inviteId;
            }

            _invitationCollection.InsertOne(invite);
            try
            {
                // New
                EmailNotificationInviteNetwork(sender.AccountId, receiver.AccountId, invite.Id);
            }
            catch (Exception ex)
            {
                log.Debug("Send  invite friend to email Error:  " + ex.ToString());
            }

            string title = "You invited " + receiver.Profile.DisplayName + " join your network";
            if (invited) title += " (following an invitation by email)";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type =
                invited ? EnumNotificationType.InviteFriendFollowEmail : EnumNotificationType.InviteFriend;
            notificationMessage.FromAccountId = sender.AccountId;
            notificationMessage.FromUserDisplayName = sender.Profile.DisplayName;
            notificationMessage.ToAccountId = receiver.AccountId;
            notificationMessage.ToUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            if (invited)
            {
                notificationMessage.Options = inviteId;
            }
            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);

            return new FuncResult(true, "sent", invite.Id);
        }

        public FriendInvitation GetInvitationById(ObjectId invitationId)
        {
            var invite = new FriendInvitation();

            invite = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invite == null)
            {
                throw new CustomException("Invitation not found");
            }

            return invite;
        }

        public FuncResult AcceptInvitation(ObjectId invitationId, ObjectId accepter)
        {
            var receiver = _accountCollection.Find(a => a.Id == accepter).FirstOrDefault();
            if (receiver == null)
            {
                return new FuncResult(false, "account.notFound");
            }

            var invite = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invite == null)
            {
                return new FuncResult(false, "invitation.notFound");
            }

            if (invite.To != receiver.Id)
            {
                return new FuncResult(false, "invitation.notTargeted");
            }
            var sender = _accountCollection.Find(a => a.Id == invite.From).FirstOrDefault();
            var receiverNetwork = _networkCollection
                .Find(n => n.NetworkOwner == receiver.Id && n.Code == Network.NORMAL).FirstOrDefault();
            var senderNetwork = _networkCollection.Find(n => n.NetworkOwner == sender.Id && n.Code == Network.NORMAL)
                .FirstOrDefault();

            _networkCollection.UpdateOne(n => n.Id == receiverNetwork.Id, Builders<Network>.Update.AddToSet(
                f => f.Friends, new MyFriend
                {
                    Id = sender.Id,
                    UserId = sender.AccountId,
                    DisplayName = sender.Profile.DisplayName
                }));
            _networkCollection.UpdateOne(n => n.Id == senderNetwork.Id, Builders<Network>.Update.AddToSet(
                f => f.Friends, new MyFriend
                {
                    Id = receiver.Id,
                    UserId = receiver.AccountId,
                    DisplayName = receiver.Profile.DisplayName
                }));

            _accountCollection.UpdateMany(a => a.Id == sender.Id || a.Id == receiver.Id,
                Builders<Account>.Update.Set(a => a.ModifiedDate, DateTime.Now));

            _invitationCollection.DeleteOne(i => i.Id == invite.Id);

            try
            {
                EmailNotification(receiver.AccountId, sender.Profile.Email, EnumNotificationType.AcceptFriend);
                log.Debug("Send notification accept to email: " + sender.Profile.Email.ToString());
            }
            catch (Exception ex)
            {
                log.Debug("Send notification accept to email error:  " + ex.ToString());
            }
            string title = "You accepted " + sender.Profile.DisplayName + " to join your network";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.AcceptFriend;
            notificationMessage.FromAccountId = receiver.AccountId;
            notificationMessage.FromUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.ToAccountId = sender.AccountId;
            notificationMessage.ToUserDisplayName = sender.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);

            return new FuncResult(true, "invitation.accepted", sender.AccountId);
        }

        #region TrustNetwork

        public void InviteTrustEmergency(ObjectId from, ObjectId to, string relationship, bool isEmergency, int rate)
        {
            if (from == to)
            {
                throw new CustomException("Cannot invite yourself");
            }

            var sender = _accountCollection.Find(a => a.Id == from).FirstOrDefault();

            var receiver = _accountCollection.Find(a => a.Id == to).FirstOrDefault();

            if (sender == null)
            {
                throw new CustomException("Sender account not found");
            }

            if (receiver == null)
            {
                throw new CustomException("Receiver account not found");
            }

            var exist = _invitationCollection.Find(i => i.From == sender.Id && i.To == receiver.Id).FirstOrDefault();
            if (exist != null)
            {
                log.Debug("Exist invitation from: " + exist.From.ToString() + " to: " + exist.To.ToString());
                return;
            }

            var reverseInvite = _invitationCollection.Find(i => i.From == receiver.Id && i.To == sender.Id)
                .FirstOrDefault();
            if (reverseInvite != null)
            {
                log.Debug("Exist reverse invitation from: " + reverseInvite.From.ToString() + " to: " +
                          reverseInvite.To.ToString());
                return;
            }

            FriendInvitation invite = new FriendInvitation
            {
                From = sender.Id,
                To = receiver.Id,
                NetworkName = "Emergency Network",
                Relationship = relationship,
                IsEmergency = isEmergency,
                Rate = rate,
                SentAt = DateTime.Now
            };

            _invitationCollection.InsertOne(invite);
            try
            {
                EmailNotification(sender.AccountId, receiver.Profile.Email, EnumNotificationType.InviteEmergency);
                log.Debug("Send invite trusted network to email: " + receiver.Profile.Email.ToString());
            }
            catch (Exception ex)
            {
                log.Debug("Send invite trusted network to email error:  " + ex.ToString());
            }

            string title = "Invite emergency";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.InviteEmergency;
            notificationMessage.FromAccountId = sender.AccountId;
            notificationMessage.FromUserDisplayName = sender.Profile.DisplayName;
            notificationMessage.ToAccountId = receiver.AccountId;
            notificationMessage.ToUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void AcceptTrustEmergency(ObjectId invitationId, ObjectId accepter, string relationship,
            bool isEmergency, int rate)
        {
            var receiver = _accountCollection.Find(a => a.Id == accepter).FirstOrDefault();

            if (receiver == null)
            {
                throw new CustomException("Accepter account not found");
            }

            var invite = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invite == null)
            {
                throw new CustomException("Invitation not found");
            }

            if (invite.To != receiver.Id)
            {
                throw new CustomException("Accepter is not allowed to accept the invitation");
            }
            var sender = _accountCollection.Find(a => a.Id == invite.From).FirstOrDefault();
            var receiverNetwork = _networkCollection
                .Find(n => n.NetworkOwner == receiver.Id && n.Code == Network.TRUSTED).FirstOrDefault();
            var senderNetwork = _networkCollection.Find(n => n.NetworkOwner == sender.Id && n.Code == Network.TRUSTED)
                .FirstOrDefault();

            var IsTrust = _networkCollection
                .Find(x => x.Id.Equals(senderNetwork.Id) && x.Friends.Any(y => y.Id.Equals(receiver.Id)))
                .FirstOrDefault();

            if (IsTrust != null)
            {
                var filter = Builders<Network>.Filter.Where(x =>
                    x.Id.Equals(senderNetwork.Id) && x.Friends.Any(y => y.Id.Equals(receiver.Id)));

                if (!string.IsNullOrEmpty(relationship))
                {
                    var update = Builders<Network>.Update.Set(x => x.Friends[-1].Relationship, relationship)
                        .Set(y => y.Friends[-1].IsEmergency, isEmergency).Set(z => z.Friends[-1].Rate, rate);
                    _networkCollection.UpdateOneAsync(filter, update);
                }
                else
                {
                    var update = Builders<Network>.Update.Set(y => y.Friends[-1].IsEmergency, isEmergency)
                        .Set(z => z.Friends[-1].Rate, rate);
                    _networkCollection.UpdateOneAsync(filter, update);
                }


                log.Debug("Updated trusted network " + receiver.Id.ToString());
            }
            else
            {
                _networkCollection.UpdateOne(n => n.Id == senderNetwork.Id, Builders<Network>.Update.AddToSet(
                    f => f.Friends, new MyFriend
                    {
                        Id = receiver.Id,
                        UserId = receiver.AccountId,
                        DisplayName = receiver.Profile.DisplayName,
                        NetworkName = "",
                        IsEmergency = isEmergency,
                        Rate = rate
                    }));
                log.Debug("Add new trusted network " + receiver.Id.ToString());
            }

            //
            _accountCollection.UpdateMany(a => a.Id == sender.Id || a.Id == receiver.Id,
                Builders<Account>.Update.Set(a => a.ModifiedDate, DateTime.Now));

            _invitationCollection.DeleteOne(i => i.Id == invite.Id);
            //Send email 
            try
            {
                EmailNotification(sender.AccountId, receiver.Profile.Email, EnumNotificationType.AcceptFriend);
            }
            catch
            {
            }

            string title = receiver.Profile.DisplayName + " has accepted to be your emergency contact.";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.AcceptEmergency;
            notificationMessage.FromAccountId = receiver.AccountId;
            notificationMessage.FromUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.ToAccountId = sender.AccountId;
            notificationMessage.ToUserDisplayName = sender.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void UpdateTrustEmergency(ObjectId userId, string userAccountId, string displayName, ObjectId networkId,
            ObjectId friendId, string friendAccountId, string friendDisplayName, string relationship, bool isEmergency,
            int rate)
        {
            try
            {
                //
                var filter = Builders<Network>.Filter.Where(x =>
                    x.Id.Equals(networkId) && x.Friends.Any(y => y.Id.Equals(friendId)));
                var update = Builders<Network>.Update.Set(x => x.Friends[-1].Relationship, relationship)
                    .Set(y => y.Friends[-1].IsEmergency, isEmergency).Set(z => z.Friends[-1].Rate, rate);
                _networkCollection.UpdateOneAsync(filter, update);


                //Send notification

                string title = isEmergency ? "Updated emergency contact relationship" : "Removed emergency contact";
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type =
                    isEmergency ? EnumNotificationType.UpdateEmergency : EnumNotificationType.RemoveEmergency;
                notificationMessage.FromAccountId = userAccountId;
                notificationMessage.FromUserDisplayName = displayName;
                notificationMessage.ToAccountId = friendAccountId;
                notificationMessage.ToUserDisplayName = friendDisplayName;
                notificationMessage.Content = title;
                // notificationMessage.PreserveBag = invite.Id;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }
            catch (Exception ex)
            {
            }
        }

        public void UpdateNetworkRelationship(ObjectId userId, string userAccountId, string fromName,
            ObjectId networkId, ObjectId friendId, string friendAccountId, string relationship)
        {
            try
            {
                //
                var filter = Builders<Network>.Filter.Where(x =>
                    x.Id.Equals(networkId) && x.Friends.Any(y => y.Id.Equals(friendId)));
                var update = Builders<Network>.Update.Set(x => x.Friends[-1].Relationship, relationship);
                _networkCollection.UpdateOneAsync(filter, update);


                //Send notification
                string title = "Network relationship updated";
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.UpdateRelationship;
                notificationMessage.FromAccountId = userAccountId;
                notificationMessage.FromUserDisplayName = fromName;
                notificationMessage.ToAccountId = friendAccountId;
                //notificationMessage.ToUserDisplayName = friendDisplayName;
                notificationMessage.Content = title;
                notificationMessage.PreserveBag = relationship;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }
            catch (Exception ex)
            {
            }
        }

        public List<Account> SearchUsersForTrustEmergency(ObjectId userId, string keyword, int? start = null,
            int? length = null)
        {
            if (length == null)
            {
                length = ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"].ParseInt();
            }
            var ignoreIds = _networkCollection.Find(n => n.NetworkOwner == userId && n.Name == "Trusted Network")
                .Project(a => a.Friends).ToList().SelectMany(a => a).Select(a => a.Id).ToList();
            return
                _accountCollection.Find(
                        f =>
                            f.AccountType == AccountType.Personal &&
                            (f.Profile.Email.ToLower().Contains(keyword.ToLower()) ||
                             f.Profile.DisplayName.ToLower().Contains(keyword.ToLower()) ||
                             f.Profile.PhoneNumber.Contains(keyword.ToLower())
                            ) && ignoreIds.Contains(f.Id))
                    .SortByDescending(a => a.Profile.DisplayName)
                    .Skip(start)
                    .Limit(length)
                    .ToList();
        }

        #endregion TrustNetwork

        public void DelegateeJoinsTrustedNetwork(string delegateeAccountId, string delegatorAccountId)
        {
            var delegateeAccount =
                _accountCollection.Find(a => a.AccountId.Equals(delegateeAccountId)).FirstOrDefault();
            var delegateeNetwork = _networkCollection
                .Find(n => n.NetworkOwner.Equals(delegateeAccount.Id) && n.Code == Network.NORMAL).FirstOrDefault();
            var delegatorAccount =
                _accountCollection.Find(a => a.AccountId.Equals(delegatorAccountId)).FirstOrDefault();
            var delegatorNetwork = _networkCollection
                .Find(n => n.NetworkOwner.Equals(delegatorAccount.Id) && n.Code == Network.TRUSTED).FirstOrDefault();

            if (delegateeAccount == null || delegateeNetwork == null || delegatorAccount == null ||
                delegatorNetwork == null)
                return;

            _networkCollection.UpdateOne(n => n.Id == delegateeNetwork.Id, Builders<Network>.Update.AddToSet(
                f => f.Friends, new MyFriend
                {
                    Id = delegatorAccount.Id,
                    UserId = delegatorAccount.AccountId,
                    DisplayName = delegatorAccount.Profile.DisplayName
                }));
            _networkCollection.UpdateOne(n => n.Id == delegatorNetwork.Id, Builders<Network>.Update.AddToSet(
                f => f.Friends, new MyFriend
                {
                    Id = delegateeAccount.Id,
                    UserId = delegateeAccount.AccountId,
                    DisplayName = delegateeAccount.Profile.DisplayName
                }));
        }

        public void RemoveInvitation(ObjectId invitationId, ObjectId removerId)
        {
            var remover = _accountCollection.Find(a => a.Id == removerId).FirstOrDefault();

            if (remover == null)
            {
                throw new CustomException("Denier account not found");
            }

            var invite = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invite == null)
            {
                throw new CustomException("Invitation not found");
            }

            if (invite.From != remover.Id)
            {
                throw new CustomException("User not allowed to remove the invitation");
            }

            var receiver = _accountCollection.Find(a => a.Id == invite.To).FirstOrDefault();

            _invitationCollection.DeleteOne(i => i.Id == invite.Id);


            string title = "You removed network invitation to " + remover.Profile.DisplayName;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.CancelFriend;
            notificationMessage.FromAccountId = remover.AccountId;
            notificationMessage.FromUserDisplayName = remover.Profile.DisplayName;
            notificationMessage.ToAccountId = receiver.AccountId;
            notificationMessage.ToUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public FuncResult DenyInvitation(ObjectId invitationId, ObjectId denier)
        {
            var receiver = _accountCollection.Find(a => a.Id == denier).FirstOrDefault();

            if (receiver == null)
            {
                return new FuncResult(false, "account.notFound");
            }

            var invite = _invitationCollection.Find(i => i.Id == invitationId).FirstOrDefault();
            if (invite == null)
            {
                return new FuncResult(false, "invitation.notFound");
            }

            if (invite.To != receiver.Id)
            {
                return new FuncResult(false, "invitation.notTargeted");
            }

            var sender = _accountCollection.Find(a => a.Id == invite.From).FirstOrDefault();

            _invitationCollection.DeleteOne(i => i.Id == invite.Id);

            bool isEmergency = invite.IsEmergency;

            string title = "You denied " + sender.Profile.DisplayName + " join your network";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type =
                isEmergency ? EnumNotificationType.DenyEmergency : EnumNotificationType.DenyFriend;
            notificationMessage.FromAccountId = receiver.AccountId;
            notificationMessage.FromUserDisplayName = receiver.Profile.DisplayName;
            notificationMessage.ToAccountId = sender.AccountId;
            notificationMessage.ToUserDisplayName = sender.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = invite.Id;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);

            return new FuncResult(true, "invitation.denied", sender.AccountId);
        }

        public List<FriendInvitation> GetReceivedInvitations(ObjectId receiver, ObjectId? fromId)
        {
            if (!fromId.HasValue)
            {
                return _invitationCollection.Find(i => i.To == receiver).ToList();
            }
            else
            {
                return _invitationCollection.Find(i => i.To == receiver && i.Id > fromId).ToList();
            }
        }

        public List<FriendInvitation> GetSendInvitations(ObjectId receiver)
        {
            var rs = new List<FriendInvitation>();
            try
            {
                rs = _invitationCollection.Find(i => i.From == receiver).ToList();
            }
            catch
            {
            }

            return rs;
        }

        public List<MyFriend> GetNetworkFriends(string accountId, string networkName)
        {
            var account = _accountCollection.Find(a => a.AccountId.Equals(accountId)).FirstOrDefault();
            var network = _networkCollection.Find(n => n.NetworkOwner.Equals(account.Id) && n.Name.Equals(networkName))
                .FirstOrDefault();
            if (network != null)
            {
                return network.Friends;
            }
            return null;
        }

        public Network GetNetworkByName(string accountId, string networkName)
        {
            var account = _accountCollection.Find(a => a.AccountId.Equals(accountId)).FirstOrDefault();
            var network = _networkCollection.Find(n => n.NetworkOwner.Equals(account.Id) && n.Name.Equals(networkName))
                .FirstOrDefault();
            return network;
        }

        public void MoveNetwork(ObjectId userId, ObjectId friendId, ObjectId fromNetworkId, ObjectId toNetworkId)
        {
            var account = _accountCollection.Find(a => a.Id == userId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException("User account not found");
            }

            var friend = _accountCollection.Find(a => a.Id == friendId).FirstOrDefault();
            if (friend == null)
            {
                throw new CustomException("Friend account not found");
            }

            var fromNetwork = _networkCollection.Find(n => n.NetworkOwner == account.Id && n.Id == fromNetworkId)
                .FirstOrDefault();
            if (fromNetwork == null)
            {
                throw new CustomException("From network not found");
            }

            var toNetwork = _networkCollection.Find(n => n.NetworkOwner == account.Id && n.Id == toNetworkId)
                .FirstOrDefault();
            if (toNetwork == null)
            {
                throw new CustomException("Destination network not found");
            }

            if (!fromNetwork.Friends.Any(f => f.Id == friend.Id))
            {
                throw new CustomException("Friend not found in network");
            }

            _networkCollection.UpdateOne(n => n.Id == fromNetwork.Id,
                Builders<Network>.Update.PullFilter(f => f.Friends, f => f.Id == friend.Id));
            _networkCollection.UpdateOne(n => n.Id == toNetwork.Id, Builders<Network>.Update.AddToSet(f => f.Friends,
                new MyFriend
                {
                    Id = friend.Id,
                    UserId = friend.AccountId,
                    DisplayName = friend.Profile.DisplayName
                }));
            _accountCollection.UpdateMany(a => a.Id == account.Id || a.Id == friend.Id,
                Builders<Account>.Update.Set(a => a.ModifiedDate, DateTime.Now));
            string title = "You moved " + friend.Profile.DisplayName + " to " + toNetwork.Name;
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.MoveFriend;
            notificationMessage.FromAccountId = account.AccountId;
            notificationMessage.FromUserDisplayName = account.Profile.DisplayName;
            notificationMessage.ToAccountId = friend.AccountId;
            notificationMessage.ToUserDisplayName = friend.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = toNetwork.Name;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }

        public void RemoveFromNetwork(ObjectId userId, ObjectId friendId, ObjectId networkId)
        {
            var account = _accountCollection.Find(a => a.Id == userId).FirstOrDefault();
            if (account == null)
            {
                throw new CustomException("User account not found");
            }

            var friend = _accountCollection.Find(a => a.Id == friendId).FirstOrDefault();
            if (friend == null)
            {
                throw new CustomException("Friend account not found");
            }

            var network = _networkCollection.Find(n => n.NetworkOwner == account.Id && n.Id == networkId)
                .FirstOrDefault();

            if (network != null)
            {
                _networkCollection.UpdateOne(n => n.Id == network.Id,
                    Builders<Network>.Update.PullFilter(f => f.Friends, f => f.Id == friend.Id));
            }

            var friendNetwork = _networkCollection
                .Find(n => n.NetworkOwner == friend.Id && n.Friends.Any(f => f.Id == account.Id)).FirstOrDefault();
            if (friendNetwork != null)
            {
                _networkCollection.UpdateOne(n => n.Id == friendNetwork.Id,
                    Builders<Network>.Update.PullFilter(f => f.Friends, f => f.Id == account.Id));
            }
            _accountCollection.UpdateMany(a => a.Id == account.Id || a.Id == friend.Id,
                Builders<Account>.Update.Set(a => a.ModifiedDate, DateTime.Now));
            string title = "You removed " + friend.Profile.DisplayName + " from your network";
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.RemoveFriend;
            notificationMessage.FromAccountId = account.AccountId;
            notificationMessage.FromUserDisplayName = account.Profile.DisplayName;
            notificationMessage.ToAccountId = friend.AccountId;
            notificationMessage.ToUserDisplayName = friend.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = "";
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);
        }


        public Network InsertNetwork(Network network)
        {
            var exist = _networkCollection.Find(n => n.NetworkOwner == network.NetworkOwner && n.Code == network.Code)
                .Any();
            if (exist)
            {
                throw new CustomException("Duplicated network");
            }
            _networkCollection.InsertOne(network);
            return network;
        }

        public List<Network> GetNetworksOfuser(ObjectId userId)
        {
            return _networkCollection.Find(n => n.NetworkOwner == userId).ToList();
        }

        public Network GetNetworkById(ObjectId id)
        {
            return _networkCollection.Find(n => n.Id == id).FirstOrDefault();
        }

        public List<MyFriend> GetListEmergencyById(ObjectId id)
        {
            return _networkCollection.Find(n => n.Id == id).FirstOrDefault().Friends.ToList();
        }


        public List<Account> SearchUsersForInvitation(ObjectId userId, string keyword, int? start = null,
            int? length = null)
        {
            if (length == null)
            {
                length = ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"].ParseInt();
            }

            var ignoreIds = _networkCollection.Find(n => n.NetworkOwner == userId).Project(a => a.Friends).ToList()
                .SelectMany(a => a).Select(a => a.Id).ToList();
            var ignore2 = _invitationCollection.Find(i => i.To == userId).Project(a => a.From).ToList();
            var ignore3 = _invitationCollection.Find(i => i.From == userId).Project(a => a.To).ToList();
            ignoreIds = ignoreIds.Concat(ignore2).Concat(ignore3).ToList();
            ignoreIds.Add(userId);

            return
                _accountCollection.Find(
                        f =>
                            f.AccountType == AccountType.Personal &&
                            (f.Profile.Email.ToLower().Contains(keyword.ToLower()) ||
                             f.Profile.DisplayName.ToLower().Contains(keyword.ToLower())
                             || f.Profile.PhoneNumber.Contains(keyword.ToLower())
                            ) && !ignoreIds.Contains(f.Id))
                    .SortByDescending(a => a.Profile.DisplayName)
                    .Skip(start)
                    .Limit(length)
                    .ToList();
        }


        public List<Network> GetNetworksUserBelongTo(ObjectId userId)
        {
            return _networkCollection.Find(n => n.Friends.Any(f => f.Id == userId)).ToList();
        }

        public IEnumerable<MyFriend> GetFriendsForDelegation(string accountId)
        {
            var account = _accountCollection.Find<Account>(a => a.AccountId.Equals(accountId)).FirstOrDefault();
            if (account != null)
            {
                IEnumerable<string> delegatedAccountIds = null;
                if (account.Delegations != null)
                {
                    delegatedAccountIds = account.Delegations.Select(d => d.ToAccountId);
                }
                var trustedNetworkownerNormal = _networkCollection
                    .Find(n => (n.NetworkOwner.Equals(account.Id)) && n.Code.Equals("NORMAL")).FirstOrDefault();
                var trustedNetworkownerTRUSTED = _networkCollection
                    .Find(n => (n.NetworkOwner.Equals(account.Id)) && (n.Code.Equals("TRUSTED"))).FirstOrDefault();
                var trustedNetworknotowners = _networkCollection.Find(n =>
                    (n.Friends.Any(y => y.UserId == account.AccountId.ToString())) &&
                    (n.Code.Equals("NORMAL") || n.Code.Equals("TRUSTED"))).ToList();
                List<MyFriend> listfriends = new List<MyFriend>();

                if (trustedNetworkownerNormal.Friends != null)
                {
                    if (delegatedAccountIds == null)
                    {
                        listfriends = trustedNetworkownerNormal.Friends.ToList();
                    }
                    else
                    {
                        listfriends = trustedNetworkownerNormal.Friends
                            .Where(f => !delegatedAccountIds.Contains(f.UserId)).ToList();
                    }
                }
                if (trustedNetworkownerTRUSTED.Friends != null)
                {
                    if (delegatedAccountIds == null)
                    {
                        listfriends.AddRange(trustedNetworkownerTRUSTED.Friends.ToList());
                    }
                    else
                    {
                        listfriends.AddRange(trustedNetworkownerTRUSTED.Friends
                            .Where(f => !delegatedAccountIds.Contains(f.UserId)).ToList());
                    }
                }
                foreach (var trustedNetworknotowner in trustedNetworknotowners)
                {
                    var trustedNetworknotowneraccount =
                        new AccountService().GetById(trustedNetworknotowner.NetworkOwner);
                    if (delegatedAccountIds.Contains(trustedNetworknotowneraccount.AccountId))
                        continue;
                    listfriends.Add(new MyFriend
                    {
                        DisplayName = trustedNetworknotowneraccount.Profile.DisplayName,
                        UserId = trustedNetworknotowneraccount.AccountId.ToString()
                    });
                }

                var newlistfriend = new List<MyFriend>();
                for (int i = 0; i < listfriends.Count; i++)
                {
                    var friendaccount = new AccountService().GetByAccountId(listfriends[i].UserId);
                    listfriends[i].DisplayName = friendaccount.Profile.DisplayName;
                    if (!newlistfriend.Any(x => x.UserId == listfriends[i].UserId))
                    {
                        newlistfriend.Add(listfriends[i]);
                    }
                }
                return newlistfriend;
            }

            return null;
        }

        public IEnumerable<Account> GetFriends(string userId)
        {
            var account = _accountCollection.Find(a => a.AccountId.Equals(userId)).FirstOrDefault();
            var normalNetwork = _networkCollection
                .Find(n => n.NetworkOwner.Equals(account.Id) && n.Code.Equals(Network.NORMAL)).FirstOrDefault();
            var trustedNetwork = _networkCollection
                .Find(n => n.NetworkOwner.Equals(account.Id) && n.Code.Equals(Network.TRUSTED)).FirstOrDefault();
            IEnumerable<MyFriend> myFriends = null;
            IEnumerable<string> friendIds = null;
            if (normalNetwork != null && normalNetwork.Friends != null)
            {
                myFriends = normalNetwork.Friends;
            }
            if (trustedNetwork != null && trustedNetwork.Friends != null)
            {
                myFriends = myFriends.Concat(trustedNetwork.Friends);
            }
            if (myFriends != null)
            {
                friendIds = myFriends.Select(f => f.UserId);
            }
            var filter = Builders<Account>.Filter;
            var criteria = filter.In("AccountId", friendIds);
            var friends = _accountCollection.Find(criteria).ToList();

            return friends;
        }

        public FuncResult IsFriend(string userId, string userId2)
        {
            if (!ObjectId.TryParse(userId, out var fromId) || !ObjectId.TryParse(userId2, out var toId))
                return new FuncResult(false, "id.invalid");

            var account = _accountCollection.Find(a => a.Id.Equals(fromId)).FirstOrDefault();
            if (account == null)
            {
                return new FuncResult(false, "self.notFound");
            }
            var account2 = _accountCollection.Find(a => a.Id.Equals(toId)).FirstOrDefault();
            if (account2 == null)
            {
                return new FuncResult(false, "other.notFound");
            }

            var networkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            var filternetwork = Builders<Network>.Filter;
            var filter = filternetwork.Where(x => x.NetworkOwner.Equals(fromId))
                         & filternetwork.ElemMatch(x => x.Friends, g => g.Id.Equals(toId))
                         | filternetwork.Eq("NetworkOwner", ObjectId.Parse(userId2))
                         & filternetwork.ElemMatch(x => x.Friends, g => g.Id.Equals(fromId));
            var network = networkCollection.Find(filter).FirstOrDefault();

            if (network != null)
            {
                if (network.Code.Equals(Network.NORMAL))
                    return new FuncResult(true, "found.normal");
                if (network.Code.Equals(Network.TRUSTED))
                    return new FuncResult(true, "found.trust");
            }

            var inviteCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("FriendInvitation");
            var filterInvite = Builders<BsonDocument>.Filter;
            var filter2 = filterInvite.Eq("From", fromId) & filterInvite.Eq("To", toId) |
                          filterInvite.Eq("From", toId) & filterInvite.Eq("To", fromId);
            var invite = inviteCollection.Find(filter2).FirstOrDefault();

            if (invite != null)
            {
                return new FuncResult(true, "found.pending", invite["_id"].ToString());
            }

            return new FuncResult(false, "notFound");
        }
        public FuncResult IsFriendByAccountId(string accountId, string accountId2)
        {

            var account = _accountCollection.Find(a => a.AccountId.Equals(accountId)).FirstOrDefault();
            if (account == null)
            {
                return new FuncResult(false, "self.notFound");
            }
            var account2 = _accountCollection.Find(a => a.AccountId.Equals(accountId2)).FirstOrDefault();
            if (account2 == null)
            {
                return new FuncResult(false, "other.notFound");
            }

            var fromId = account.Id;
            var toId = account2.Id;

            var networkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            var filternetwork = Builders<Network>.Filter;
            var filter = filternetwork.Where(x => x.NetworkOwner.Equals(fromId))
                         & filternetwork.ElemMatch(x => x.Friends, g => g.Id.Equals(toId))
                         | filternetwork.Eq("NetworkOwner", toId)
                         & filternetwork.ElemMatch(x => x.Friends, g => g.Id.Equals(fromId));
            var network = networkCollection.Find(filter).FirstOrDefault();

            if (network != null)
            {
                if (network.Code.Equals(Network.NORMAL))
                    return new FuncResult(true, "found.normal");
                if (network.Code.Equals(Network.TRUSTED))
                    return new FuncResult(true, "found.trust");
            }

            var inviteCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("FriendInvitation");
            var filterInvite = Builders<BsonDocument>.Filter;
            var filter2 = filterInvite.Eq("From", fromId) & filterInvite.Eq("To", toId) |
                          filterInvite.Eq("From", toId) & filterInvite.Eq("To", fromId);
            var invite = inviteCollection.Find(filter2).FirstOrDefault();

            if (invite != null)
            {
                return new FuncResult(true, "found.pending", invite["_id"].ToString());
            }

            return new FuncResult(false, "notFound");
        }

        public async Task<FuncResult> MoveFriendAsync(Account account, Account friend, bool toTrust = false)
        {
            var myId = account.Id;
            var friendId = friend.Id;


            var fromNet = toTrust ? Network.NORMAL : Network.TRUSTED;
            var toNet = toTrust ? Network.TRUSTED : Network.NORMAL;

            var networkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            var builder = Builders<Network>.Filter;
            var builderFriend = Builders<MyFriend>.Filter;
            var filter = builder.Eq("NetworkOwner", myId);

            var network = await networkCollection.Find(filter & builder.Eq("Code", fromNet)
                                                       & builder.ElemMatch("Friends", builderFriend.Eq("Id", friendId)))
                .FirstOrDefaultAsync();
            if (network == null)
                return new ErrResult("friend.notFound");

            var toNetwork = await networkCollection.Find(filter & builder.Eq("Code", toNet)).FirstOrDefaultAsync();
            if (toNetwork == null)
                return new ErrResult("friend.notFound");

            var filterFriend = builder.Eq("NetworkOwner", friendId);
            var friendNetwork = await networkCollection.Find(filterFriend & builder.Eq("Code", fromNet)
                                                             & builder.ElemMatch("Friends", builderFriend.Eq("Id", myId)))
                .FirstOrDefaultAsync();
            if (friendNetwork == null)
                return new ErrResult("friend.notFound");

            var friendToNetwork =
                await networkCollection.Find(filterFriend & builder.Eq("Code", toNet)).FirstOrDefaultAsync();
            if (friendToNetwork == null)
                return new ErrResult("friend.notFound");

            var update = Builders<Network>.Update;

            var updateResult = await networkCollection.UpdateOneAsync(builder.Eq("Id", network.Id),
                update.PullFilter("Friends", builderFriend.Eq("Id", friendId)));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("notUpdated");
            updateResult = await networkCollection.UpdateOneAsync(builder.Eq("Id", toNetwork.Id),
                update.AddToSet(f => f.Friends, new MyFriend
                    {
                        Id = friendId,
                        UserId = friend.AccountId,
                        DisplayName = friend.Profile.DisplayName
                    }));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("aborted");

            updateResult = await networkCollection.UpdateOneAsync(builder.Eq("Id", friendNetwork.Id),
                update.PullFilter("Friends", builderFriend.Eq("Id", myId)));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("aborted");
            updateResult = await networkCollection.UpdateOneAsync(builder.Eq("Id", friendToNetwork.Id),
                update.AddToSet(f => f.Friends, new MyFriend
                    {
                        Id = myId,
                        UserId = account.AccountId,
                        DisplayName = account.Profile.DisplayName
                    }));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("aborted");

            string title = $"{account.Profile.DisplayName} moved you to {toNetwork.Name}";
            
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.MoveFriend;
            notificationMessage.FromAccountId = account.AccountId;
            notificationMessage.FromUserDisplayName = account.Profile.DisplayName;
            notificationMessage.ToAccountId = friend.AccountId;
            notificationMessage.ToUserDisplayName = friend.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = toNetwork.Name;
            new NotificationBusinessLogic().SendNotification(notificationMessage);
            
            // Write activity log
            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                title = $"You moved {friend.Profile.DisplayName} to {toNet.ToLower()} network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, "network");
            }
            
            return new OkResult();
        }
        public async Task<FuncResult> RemoveFriendAsync(Account account, Account friend)
        {
            var myId = account.Id;
            var friendId = friend.Id;

            var networkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            
            var builder = Builders<Network>.Filter;
            var builderFriend = Builders<MyFriend>.Filter;
            var filter = builder.Eq("NetworkOwner", myId) 
                         & builder.ElemMatch("Friends", builderFriend.Eq("Id", friendId));
            bool isMember = await networkCollection.Find(filter).AnyAsync();
            if (!isMember)
                return new ErrResult("friend.notFound");

            var filterFriend = builder.Eq("NetworkOwner", friendId)
                               & builder.ElemMatch("Friends", builderFriend.Eq("Id", myId));

            var update = Builders<Network>.Update;

            var updateResult = await networkCollection.UpdateManyAsync(filter,
                update.PullFilter("Friends", builderFriend.Eq("Id", friendId)));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("notUpdated");
            
            updateResult = await networkCollection.UpdateManyAsync(filterFriend,
                update.PullFilter("Friends", builderFriend.Eq("Id", myId)));
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                return new ErrResult("aborted");

            string title = $"{account.Profile.DisplayName} removed you from their network";
          
            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.RemoveFriend;
            notificationMessage.FromAccountId = account.AccountId;
            notificationMessage.FromUserDisplayName = account.Profile.DisplayName;
            notificationMessage.ToAccountId = friend.AccountId;
            notificationMessage.ToUserDisplayName = friend.Profile.DisplayName;
            notificationMessage.Content = title;
            notificationMessage.PreserveBag = "";
            new NotificationBusinessLogic().SendNotification(notificationMessage);
            
            var delegation = new DelegationBusinessLogic();
            try
            {
                delegation.DeleteDelegation(account.AccountId, friend.AccountId);
            }
            catch
            {
            }
            
            // Write activity log
            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                title = $"You removed {friend.Profile.DisplayName} from network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, "network");
            }
            
            return new OkResult();
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class NetworkMember
        {
            public string UserId { get; set; }
            public string AccountId { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
            public bool IsMember { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPending { get; set; }         
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Direction{ get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Network { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string InvitationId { get; set; }
        }

        public async Task<FuncResult> GetFriendsAsync(Account account)
        {
            var userId = account.Id;
            var accountId = account.AccountId;
            
            var networkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            var builder = Builders<Network>.Filter;
            var filter = builder.Eq("NetworkOwner", userId);
            var networks = await networkCollection.Find(filter).ToListAsync();

            var friends = new List<NetworkMember>();

            if (networks.Count > 0)
            {
                foreach (var network in networks)
                {
                    var code = network.Code;
                    if (code != Network.NORMAL && code != Network.TRUSTED) continue;
                    foreach (var member in network.Friends)
                    {
                        var friendId = member.Id.ToString();
                        var friendAccount = new AccountService().GetById(member.Id);
                        if (friendAccount == null) continue;
                        var friend = new NetworkMember()
                        {
                            UserId = friendId,
                            AccountId = friendAccount.AccountId,
                            DisplayName = friendAccount.Profile.DisplayName,
                            Avatar = friendAccount.Profile.PhotoUrl,
                            IsMember = true,
                            Network = code == Network.TRUSTED ? "trust" : "normal"
                        };

                        friends.Add(friend);
                    }
                }
            }


            var inviteCollection = MongoDBConnection.Database.GetCollection<FriendInvitation>("FriendInvitation");
            var builderInvites = Builders<FriendInvitation>.Filter;
            var filter2 = builderInvites.Eq("From", userId) |
                          builderInvites.Eq("To", userId);
            var invites = await inviteCollection.Find(filter2).ToListAsync();


            foreach (var invite in invites)
            {
                var outgoing = invite.From == userId;
                var friendId =  outgoing ? invite.To : invite.From;
                if (!friends.Exists(f => f.UserId == friendId.ToString()))
                {
                    var friendAccount = new AccountService().GetById(friendId);
                    if (friendAccount != null)
                    {
                        var friend = new NetworkMember()
                        {
                            UserId = friendId.ToString(),
                            AccountId = friendAccount.AccountId,
                            DisplayName = friendAccount.Profile.DisplayName,
                            Avatar = friendAccount.Profile.PhotoUrl,
                            IsMember = false,
                            IsPending = true,
                            Direction = outgoing ? "out" : "in",
                            InvitationId = invite.Id.ToString()
                        };
                        friends.Add(friend);
                    }
                }
            }

            if (friends.Count == 0)
                return new FuncResult(false, "notFound");

            return new FuncResult(true, "network.members", friends);
        }


        public string GetStatusFriend(Account usercurrent, Account usercheck)
        {
            string status = "";

            try
            {
                string usercurrentid = usercurrent.Id.ToString();
                string usercurrentaccountid = usercurrent.AccountId.ToString();

                string usercheckid = usercheck.Id.ToString();
                string usercheckaccountid = usercheck.AccountId.ToString();
                var NetworkCollection =
                    MongoDBConnection.Database.GetCollection<GH.Core.BlueCode.Entity.Network.Network>("Network");
                var filternetwork = Builders<GH.Core.BlueCode.Entity.Network.Network>.Filter;
                var filterusercurrentnetwork =
                    filternetwork.Where(x => (x.NetworkOwner.Equals(ObjectId.Parse(usercurrentid))));
                filterusercurrentnetwork = filterusercurrentnetwork &
                                           filternetwork.ElemMatch(x => x.Friends, g => g.UserId == usercheckaccountid);
                filterusercurrentnetwork = filterusercurrentnetwork |
                                           (filternetwork.Eq("NetworkOwner", usercheck.Id) &
                                            filternetwork.ElemMatch(x => x.Friends,
                                                g => g.UserId == usercurrentaccountid));
                var listnetwork = NetworkCollection.Find(filterusercurrentnetwork).ToList();
                if (listnetwork.Count() > 0)
                {
                    status = listnetwork.FirstOrDefault().Code.ToLower();
                }
                else
                {
                    var FriendCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("FriendInvitation");
                    var filterinvitefriend = Builders<BsonDocument>.Filter;
                    var filterusercurrentinvitefriend = filterinvitefriend.Eq("From", usercurrent.Id);
                    filterusercurrentinvitefriend =
                        filterusercurrentinvitefriend & filterinvitefriend.Eq("To", usercheck.Id) |
                        (
                            filterinvitefriend.Eq("To", usercurrent.Id) & filterinvitefriend.Eq("From", usercheck.Id)
                        );
                    if (FriendCollection.Find(filterusercurrentinvitefriend).Count() > 0)
                    {
                        status = "pending";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }

        //Send mail
        public void InviteEmailOutsideForEmergency(string fromAccountId, string toEmail, string inviteId)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_InviteForEmercy.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_InviteForEmercy.html");
            }
            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var fromAccount = _accountService.GetByAccountId(fromAccountId);
                var fullName = fromAccount.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignIn", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                var subject = string.Format("{0} invites you to join Regit", fullName);
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                });
            }
        }

        public void SyncMailHandShake(string displayName, string toEmail, string userId, string filePath,
            string contentHtml, string baseUrl)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;

            emailTemplate =
                HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_HandShakeSyncMail.html");
            if (filePath != "")
            {
                filePath = HostingEnvironment.MapPath(Path.Combine("~/Content/vault/HandShake/", userId, filePath));
            }

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                emailContent = emailContent.Replace("[username]", displayName);
                var subject = string.Format("{0} Sync Update Notification", displayName);
                IMailService mailService = new MailService();
                var noti = new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                };
                mailService.SendMailAttachmentAsync(noti, filePath, contentHtml);
            }
        }

        //Send mailb 5t55
        public void SyncListMailHandShake(string handShakeId, string displayName, string toEmail, string[] toListEmail,
            string userId, string filePath, string contentHtml, string baseUrl, string companyName, string campaignName)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;

            emailTemplate =
                HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_HandShakeSyncMail.html");
            if (filePath != "")
            {
                filePath = HostingEnvironment.MapPath(Path.Combine("~/Content/vault/HandShake/", handShakeId,
                    filePath));
            }

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var subject = string.Format("Handshake notification from Regit - " + campaignName);
                emailContent = emailContent.Replace("[username]", displayName);
                emailContent = emailContent.Replace("[CompanyName]", companyName);
                emailContent = emailContent.Replace("[CampaignName]", campaignName);
                IMailService mailService = new MailService();
                if (toListEmail.Length > 0)
                {
                    var notiList = new NotificationContent
                    {
                        Title = subject,
                        Body = string.Format(emailContent, ""),
                        SendTo = toListEmail
                    };
                    mailService.SendMailAttachmentAsync(notiList, filePath, contentHtml);
                }

                if (toEmail != "")
                {
                    var noti = new NotificationContent
                    {
                        Title = subject,
                        Body = string.Format(emailContent, ""),
                        SendTo = new[] {toEmail}
                    };
                    mailService.SendMailAttachmentAsync(noti, filePath, contentHtml);
                }
            }
        }

        //Send mail
        public void InviteListEmailOutsideForEmergency(string fromAccountId, string[] toEmail, string inviteId)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_InviteForEmercy.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_InviteForEmercy.html");
            }
            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var fromAccount = _accountService.GetByAccountId(fromAccountId);
                var fullName = fromAccount.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignUp", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                var subject = string.Format("{0} invites you to join Regit", fullName);
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = toEmail
                });
            }
        }

        //Send mail notification
        public void EmailNotificationInviteNetwork(string fromAccountId, string toAccountId, ObjectId inviteId)
        {
            var _accountService = new AccountService();
            var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();

            var toAccount = _accountService.GetByAccountId(toAccountId);
            var fromAccount = _accountService.GetByAccountId(fromAccountId);

            var toUser = toAccount.Profile.DisplayName ??
                         toAccount.Profile.FirstName + " " + toAccount.Profile.LastName;
            var fromAvatarUrl = fromAccount.Profile.PhotoUrl;
            if (string.IsNullOrEmpty(fromAvatarUrl))
                fromAvatarUrl = "/Areas/Beta/img/profile-picture.png";
            var fromProfilePhoto = String.Format("{0}{1}", baseUrl, fromAvatarUrl);
            var fromDiplayName = fromAccount.Profile.DisplayName ??
                                 fromAccount.Profile.FirstName + " " + fromAccount.Profile.LastName;
            var fromAboutMe = fromAccount.Profile.Status;
            var fromCountry = fromAccount.Profile.Country;
            var viewFromProfile = String.Format("{0}/User/Profile/{1}", baseUrl, fromAccount.Id.ToString());
            var acceptInvite = String.Format("{0}/user/acceptinvite?id={1}", baseUrl, inviteId.ToString());

            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_NetworkInvitation.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_NetworkInvitation.html");
            }

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);

                emailContent = emailContent.Replace("[to-user]", toUser);
                emailContent = emailContent.Replace("[from-profile-photo]", fromProfilePhoto);
                emailContent = emailContent.Replace("[from-display-name]", fromDiplayName);
                emailContent = emailContent.Replace("[from-about-me]", fromAboutMe);
                emailContent = emailContent.Replace("[from-country]", fromCountry);

                emailContent = emailContent.Replace("[view-from-profile]", viewFromProfile);
                emailContent = emailContent.Replace("[accept-invite]", acceptInvite);
                string notification = toUser + ", please add me to your network.";

                IMailService mailService = new MailService();
                mailService.SendMailAsync(new NotificationContent
                {
                    Title = notification,
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toAccount.Profile.Email}
                });
            }
        }

        //Send mail notification
        public void EmailNotification(string fromAccountId, string toEmail, string notificationType)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath("/Content/EmailTemplates/email_template_notification.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\email_template_notification.html");
            }
            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var fromAccount = _accountService.GetByAccountId(fromAccountId);
                var fullName = fromAccount.Profile.DisplayName;

                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignUp", baseUrl);

                emailContent = emailContent.Replace("[callbacklink]", callbackLink);

                string notification = fullName + " has a notification from Regit.";
                string titleLink = "Login to Regit.";
                switch (notificationType)
                {
                    case "Invite Friend":
                    {
                        notification = "You have a new invitation.";
                        titleLink = "Login to accept.";
                        break;
                    }
                    case "Accept Friend":
                    {
                        notification = fullName + " accepted your invitation request.";
                        break;
                    }
                    case "Deny Friend":
                    {
                        notification = fullName + " denied your network invitation.";
                        break;
                    }

                    case "Invite Emergency":
                    {
                        notification = fullName + " invited you to be an emergency contact.";
                        titleLink = "Login to view.";
                        break;
                    }

                    case "Accept Emergency":
                    {
                        notification = fullName + " accepted to be your emergency contact.";
                        break;
                    }
                    case "Deny Emergency":
                    {
                        notification = fullName + " declined to be your emergency contact.";
                        break;
                    }
                    case "Remove Emergency":
                    {
                        notification = fullName + " removed you as an emergency contact.";
                        break;
                    }
                    case "Update Emergency":
                    {
                        notification = fullName + " updated contact information.";
                        break;
                    }

                    case "Invite Family":
                    {
                        notification =
                            fullName + " invited you to be part of their family network so you can stay in touch.";
                        titleLink = "Login to view.";
                        break;
                    }
                    case "Accept Family":
                    {
                        notification = fullName + " accepted to be part of your family network.";
                        break;
                    }
                    case "Remove Family":
                    {
                        notification = fullName + " removed you from their family network.";
                        break;
                    }
                    case "Update Family":
                    {
                        notification = fullName + " changed your relationship in their family network.";
                        break;
                    }

                    default:
                    {
                        notification = fullName + " has a notification from Regit.";
                        titleLink = "Login to Regit.";
                        break;
                    }
                }
                //
                emailContent = emailContent.Replace("[content]", notification);
                emailContent = emailContent.Replace("[titlelink]", titleLink);
                IMailService mailService = new MailService();
                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                });
            }
        }


        public List<EmergencyContact> GetEmergencyContactByUserid(string userid)
        {
            var account = new AccountService().GetByAccountId(userid);

            var networks = GetNetworksOfuser(account.Id).Where(x => x.Name == "Trusted Network");
            var listmergencyContact = new List<EmergencyContact>();
            foreach (var network in networks)
            {
                if (network.Friends != null && network.Friends.Count > 0)
                {
                    listmergencyContact = network.Friends.Where(x => x.IsEmergency).Select(y => new EmergencyContact
                    {
                        AccountId = y.UserId,
                        DisplayName = y.DisplayName
                    }).ToList();
                }


                // network.Friends[0].
            }
            return listmergencyContact;
            // return networks.Select(n => new NetworkViewModel { Id = n.Id.ToString(), Name = n.Name }).OrderByDescending(n => n.Name).ToList();
        }

        public void UpdateNetworkTrustData()
        {
            var filter = Builders<Network>.Filter.Where(x => x.Name.Equals("Trusted Network"));

            var update = Builders<Network>.Update.Set(y => y.Name, Network.TRUSTED_NETWORK);
            _networkCollection.UpdateManyAsync(filter, update);
        }
    }
}