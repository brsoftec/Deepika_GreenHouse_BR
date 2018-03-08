using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.BlueCode.DataAccess;
using GH.Core.Services;
using Caliburn.Micro;
using GH.Core.SignalR.Events;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using GH.Core.BlueCode.BusinessLogic;
using Microsoft.AspNet.Identity;
using NLog;
using NLog.Fluent;


namespace RegitSocial.Business.Notification
{
    public class NotificationBusinessLogic : INotificationBusinessLogic
    {
        private MongoRepository<NotificationMessage> repository = new MongoRepository<NotificationMessage>();
        private AccountService accountService = new AccountService();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private object locker = new object();

        private string ProperLabel(string text)
        {
            string proper = Regex.Replace(text, "(\\B[A-Z])", " $1");
            return proper.First().ToString().ToUpper() + String.Join("", proper.Skip(1));
        }

        public void SendNotification(NotificationMessage notificationMessage)
        {
            var title = string.Empty;
            var cat = "";
            object payload = notificationMessage.Payload;
            if (payload == null)
            {
                payload = notificationMessage.Payload = string.Empty;
            }

            var accountId = notificationMessage.FromAccountId;
            //var account = accountService.GetByAccountId(accountId);
            var fromAccount = accountService.GetByAccountId(accountId);
            var toAccount = accountService.GetByAccountId(notificationMessage.ToAccountId);

            switch (notificationMessage.Type)
            {
                case "Delegation Request":
                {
                    title = "[fromuser] has delegated their information to you";
                    cat = "delegation";
                    break;
                }
                case "Delegation Accept":
                {
                    title = "[fromuser] has accepted your delegation.";
                    cat = "delegation";
                    break;
                }
                case "Delegation Deny":
                {
                    title = "[fromuser] has denied your delegation request.";
                    cat = "delegation";
                    break;
                }
                case "Delegation Activated":
                {
                    title = "[fromuser] has activated your emergency delegation.";
                    cat = "delegation";
                    break;
                }
                //Delegation Remove
                case "Delegation Remove":
                {
                    title = "[fromuser] has removed a delegation relationship with you.";
                    cat = "delegation";
                    break;
                }
                case "Notify Missing Information Vault For Registration":
                {
                    title = "[fromuser] as a delegatee needs you to fill in missing information in your vault.";
                    cat = "delegation";
                    break;
                }
                case "Push To Vault":
                {
                    title = "[fromuser] has requested to push information to your vault.";
                    cat = "push";
                    break;
                }
                case "Push To Vault Accecpt":

                {
                    title = "[fromuser] accepted your push-to-vault request.";
                    cat = "push";
                    break;
                }
                case "Push To Vault Deny":
                {
                    title = "[fromuser] denied your push-to-vault request.";
                    cat = "push";
                    break;
                }

                case "HandShake Vault Changed":
                {
                    title = "[fromuser] changed his/her vault subject to a handshake.";
                    cat = "handshake";
                    break;
                }

                case "Business Unregister":
                {
                    title = "[fromuser] has removed your registration.";
                    cat = "interaction";
                    break;
                }
                case "Register Interaction":
                {
                    title = "[fromuser] has joined an interaction.";
                    cat = "interaction";
                    break;
                }

                case "Invited Handshake":
                {
                    title = "[fromuser] has invited you to join a handshake.";
                    cat = "handshake";
                    break;
                }
                case "Join Handshake":
                {
                    title = "[fromuser] joined a handshake.";
                    cat = "handshake";
                    break;
                }
                case "Pause Handshake":
                {
                    title = "[fromuser] paused a handshake.";
                    cat = "handshake";
                    break;
                }
                case "Resume Handshake":
                {
                    title = "[fromuser] resumed a handshake.";
                    cat = "handshake";
                    break;
                }
                case "Terminate Handshake":
                {
                    title = "[fromuser] terminated a handshake.";
                    cat = "handshake";
                    break;
                }
                case "Terminate Individual Handshake":
                {
                    title = "[fromuser] terminated an individual handshake.";
                    cat = "handshake";
                    break;
                }
                case "Acknowledge Handshake":
                {
                    title = "[fromuser] has acknowledged your update";
                    cat = "handshake";
                    break;
                }

                case "Expired date Handshake":
                {
                    title = "Expired date Handshake";
                    cat = "handshake";
                    break;
                }
                case "Request Handshake":
                {
                    title = "[fromuser] sent you a handshake request.";
                    cat = "handshake";
                    break;
                }
                case "Invite Friend":
                {
                    title = "Network invitation from [fromuser]";
                    cat = "network";
                    break;
                }
                case "Invite Friend Follow Email":
                {
                    title = "Network invitation from [fromuser], following invitation by email";
                    cat = "network";
                    break;
                }
                case "Cancel Friend":
                {
                    title = "[fromuser] has cancelled invitation to their network.";
                    cat = "network";
                    break;
                }
                case "Accept Friend":
                {
                    title = "[fromuser] has accepted you to their network.";
                    cat = "network";
                    break;
                }
                case "Deny Friend":
                {
                    title = "[fromuser] has denied you to their network.";
                    cat = "network";
                    break;
                }
                case "Move Friend":
                {
                    title = "[fromuser] has moved you to " + notificationMessage.PreserveBag;
                    cat = "network";
                    break;
                }
                case "Remove Friend":
                {
                    title = "[fromuser] has removed you from their network.";
                    cat = "network";
                    break;
                }

                case "Invite Emergency":
                {
                    title = "[fromuser] has requested you to be an emergency contact.";
                    cat = "network";
                    break;
                }

                case "Accept Emergency":
                {
                    title = "[fromuser] has accepted to be your emergency contact.";
                    cat = "network";
                    break;
                }
                case "Deny Emergency":
                {
                    title = "[fromuser] has declined to be your emergency contact.";
                    cat = "network";
                    break;
                }
                case "Remove Emergency":
                {
                    title = "[fromuser] has removed you as their emergency contact.";
                    cat = "network";
                    break;
                }
                case "Update Emergency":
                {
                    title = "[fromuser] has updated contact information.";
                    cat = "network";
                    break;
                }

                case "Invite Family":
                {
                    title = "Family invitation from [fromuser]";
                    cat = "network";
                    break;
                }
                case "Accept Family":
                {
                    title = "[fromuser] has accepted to be part of your family network.";
                    cat = "network";
                    break;
                }
                case "Remove Family":
                {
                    title = "[fromuser] has removed you from their family network.";
                    cat = "network";
                    break;
                }
                case "Update Family":
                {
                    title = "[fromuser] has changed your relationship in their family network.";
                    cat = "network";
                    break;
                }
                case "Update Relationship":
                {
                    title = "[fromuser] has changed your relationship with them to " +
                            (string) notificationMessage.PreserveBag + ".";
                    cat = "network";
                    break;
                }
                case "Workflow Invite Member":
                {
                    title = "[fromuser] has invited you to be member of its workflow as " + (string) payload;
                    cat = "workflow";
                    break;
                }
                case "Workflow Invite Member Follow Email":
                {
                    title = "[fromuser] has invited you to be member of its workflow as " + (string) payload
                                                                                          + ", following invitation by email";
                    cat = "workflow";
                    break;
                }
                case "Workflow Cancel Invitation":
                {
                    title = "[fromuser] has cancelled workflow invitation to you.";
                    cat = "workflow";
                    break;
                }
                case "Workflow Accept Invitation":
                {
                    title = "[fromuser] has accepted your invitation to become a workflow member.";
                    cat = "workflow";
                    break;
                }
                case "Workflow Deny Invitation":
                {
                    title = "[fromuser] has denied your workflow invitation.";
                    cat = "workflow";
                    break;
                }
                case "Workflow Update Member":
                {
                    title = "[fromuser] has updated your workflow roles.";
                    cat = "workflow";
                    break;
                }
                case "Workflow Remove Member":
                {
                    title = "[fromuser] has removed you as member from its workflow.";
                    cat = "workflow";
                    break;
                }
                case "Interaction Participate Delegated":
                {
                    title = "[fromuser] has registered on your behalf for an interaction.";
                    cat = "interaction";
                    break;
                }
                case "Interaction Unparticipate":
                {
                    title = "[fromuser] has unregistered you from an interaction.";
                    cat = "interaction";
                    break;
                }
                case "Billing First":
                {
                    title = "Payment charged successfully.";
                    cat = "billing";
                    break;
                }
                case "Billing Failed":
                {
                    title = "Payment charged unsuccessfully";
                    cat = "billing";
                    break;
                }
                case "Billing Renew":
                {
                    title = "Plan renewed successfully";
                    cat = "billing";
                    break;
                }
                case "Invited SRFI":
                {
                    title = notificationMessage.Content == "srfi"
                        ? "[fromuser] has requested you to submit your personal information"
                        : "[fromuser] has invited you to join an interaction";
                    cat = "srfi";
                    break;
                }
                default:
                    break;
            }


            notificationMessage.Id = ObjectId.GenerateNewId();
//            notificationMessage.ObjId = notificationMessage.Id.ToString();
            notificationMessage.Category = cat;
            notificationMessage.Content = notificationMessage.Content ?? string.Empty;
            notificationMessage.Options = notificationMessage.Options ?? string.Empty;


            notificationMessage.Title = title.Replace("[fromuser]", notificationMessage.FromUserDisplayName);
            //notificationMessage.DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
            repository.Add(notificationMessage);

            //
            if (notificationMessage.Type != EnumNotificationType.NotifyInvitedHandshakeOutsite)
            {
                if (notificationMessage.Type == EnumNotificationType.InviteFriend)
                {
                    notificationMessage.PayloadStatus = "isPending";
                }

                //push notificaiton to user
                var eventAggregator =
                    (IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                        typeof(IEventAggregator));
                //Send notification to specific user
                eventAggregator.Publish(new ToUserConstrainedEvent
                {
                    AccountId = notificationMessage.ToAccountId,
                    RuntimeNotifyType = "Notify",
                    Message = new JavaScriptSerializer().Serialize(notificationMessage)
                });

                var _ = new AuthTokensLogic().PostToFcmAsync(toAccount, fromAccount, "notification",
                    $"Notification from {notificationMessage.FromUserDisplayName}", notificationMessage.Title,
                    notificationMessage);

/*
                lock (locker)
                {
                    latestNotification.UnViewedNotifications++;
                    if (list.Count > 10)
                    {
                        list.RemoveRange(10, list.Count - 10);
                    }
                }
                latestNotification.Notifications = list;*/

                //accountService.UpdateLastestNotification(latestNotification, notificationMessage.ToAccountId);
            }
        }

        public void SendNotificationMessageGoverment(string accountid)
        {
            try
            {
                NotificationMessage notificationMessage = new NotificationMessage();
                var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("userId", accountid);

                var projection = Builders<BsonDocument>.Projection.Include("userId");

                projection = projection.Include("groupGovernmentID.value.birthCertificate.value");
                projection = projection.Include("groupGovernmentID.value.driverLicenseCard.value");
                projection = projection.Include("groupGovernmentID.value.healthCard.value");
                projection = projection.Include("groupGovernmentID.value.medicalBenefitCard.value");
                projection = projection.Include("groupGovernmentID.value.passportID.value");
                projection = projection.Include("groupGovernmentID.value.permanentResidenceCard.value");
                projection = projection.Include("groupGovernmentID.value.socialInsuranceCard.value");
                projection = projection.Include("groupGovernmentID.value.taxID.value");
                projection = projection.Include("groupGovernmentID.value.CustomGovernmentID.value");

                var vaultuser = vaultCollection.Find(filter).Project(projection).ToList().FirstOrDefault();
                var arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["birthCertificate"]["value"]
                    .AsBsonArray;

                // Vu   
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "birthCertificate", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["driverLicenseCard"]["value"].AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "driverLicenseCard", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["healthCard"]["value"].AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "healthCard", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["medicalBenefitCard"]["value"]
                    .AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "medicalBenefitCard", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["passportID"]["value"].AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "passportID", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["permanentResidenceCard"]["value"]
                    .AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "permanentResidenceCard",
                        accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["socialInsuranceCard"]["value"]
                    .AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "socialInsuranceCard", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["taxID"]["value"].AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "taxID", accountid);
                }

                arrayjsongorverment = vaultuser["groupGovernmentID"]["value"]["CustomGovernmentID"]["value"]
                    .AsBsonArray;
                if (arrayjsongorverment.Values.Count() > 0)
                {
                    SendNotificationMessageFromyjsongorverment(arrayjsongorverment, "CustomGovernmentID", accountid);
                }

                return;
            }
            catch
            {
            }
        }

        public bool SendNotificationMessageFromyjsongorverment(BsonArray arrayjsongorverment, string nameGovermentID,
            string accountid)
        {
            NotificationMessage notificationMessage = null;
            foreach (var govermon in arrayjsongorverment)
            {
                DateTime? dtexpirydate = null;
                try
                {
                    dtexpirydate = Convert.ToDateTime(govermon["expiryDate"].AsString);

                    if (dtexpirydate <= DateTime.Now)
                    {
                        notificationMessage = new NotificationMessage();
                        notificationMessage.Category = "vault";
                        notificationMessage.Type = "GovermentID Expired";
                        notificationMessage.Title = "Your " + this.ProperLabel(nameGovermentID) + " has expired";
                        //  notificationMessage.Title = "GovermentID Expiried";
                        notificationMessage.Id = ObjectId.GenerateNewId();
//                        notificationMessage.ObjId = notificationMessage.Id.ToString();
//                        notificationMessage.DateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
                        notificationMessage.ToAccountId = accountid;
                        repository.Add(notificationMessage);

                        var toAccount = accountService.GetByAccountId(accountid);

                        LatestNotification latestNotification;
                        if (toAccount.LastestNotification == null)
                        {
                            latestNotification = new LatestNotification();
                        }
                        else
                        {
                            latestNotification = toAccount.LastestNotification;
                        }

                        latestNotification.UnViewed = true;
                        List<NotificationMessage> list;
                        if (latestNotification.Notifications != null)
                            list = latestNotification.Notifications.ToList();
                        else
                            list = new List<NotificationMessage>();
                        list.Insert(0, notificationMessage);

                        //push notificaiton to user
                        var eventAggregator =
                            (IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                                typeof(IEventAggregator));
                        //Send notification to specific user
//                        eventAggregator.Publish(new ToUserConstrainedEvent(notificationMessage.ToAccountId, ""));
                        eventAggregator.Publish(new ToUserConstrainedEvent
                        {
                            AccountId = notificationMessage.ToAccountId,
                            RuntimeNotifyType = "Notify",
                            Message = new JavaScriptSerializer().Serialize(notificationMessage)
                        });


                        lock (locker)
                        {
                            latestNotification.UnViewedNotifications++;
                            if (list.Count > 10)
                            {
                                list.RemoveRange(10, list.Count - 10);
                            }
                        }

                        latestNotification.Notifications = list;

                        accountService.UpdateLastestNotification(latestNotification, notificationMessage.ToAccountId);
                    }
                }
                catch
                {
                }
            }

            return notificationMessage != null;
        }

        public IEnumerable<NotificationMessage> ViewNotification(string accountId)
        {
            var account = accountService.GetByAccountId(accountId);
            if (account.LastestNotification != null && account.LastestNotification.Notifications != null)
            {
                account.LastestNotification.UnViewed = false;
                account.LastestNotification.UnViewedNotifications = 0;
                accountService.UpdateLastestNotification(account.LastestNotification, accountId);
                return account.LastestNotification.Notifications;
            }

            return null;
        }

        public IEnumerable<NotificationMessage> GetNotificationList(string userId, int pageIndex = 1, int pageSize = 10)
        {
            return repository.Many(n => n.ToAccountId.Equals(userId), pageIndex, pageSize).AsEnumerable();
        }

        public IEnumerable<NotificationMessage> GetNotifications(string userId, int pageIndex = 1, int pageSize = 10)
        {
            return repository.Many(n => n.ToAccountId.Equals(userId), pageIndex, pageSize).AsEnumerable();
        }


        public List<NotificationMessage> GetNotificationByAccountId(string accountId, string notificationType = null,
            int start = 0, int take = 10)
        {
            var rs = new List<NotificationMessage>();
            var listtest = repository.Many(l => l.ToAccountId.Equals(accountId)).OrderByDescending(s => s.DateTime)
                .ToList();
            try
            {
                rs = listtest.Skip(start).Take(take).ToList();
                //if (!string.IsNullOrEmpty(notificationType))
                //{


                //}
                //else
                //{
                //    rs = repository.Many(l => l.ToAccountId.Equals(accountId)).OrderBy(s => s.DateTime).Skip(start).Take(take).ToList();

                //}
            }
            catch
            {
            }

            return rs;
        }

        // Son
        public List<UserNotification> GetNotifications(string accountId, string notificationType = null,
            int start = 0, int take = 10)
        {
            var rs = new List<UserNotification>();
            var notifications = repository.Many(l => l.ToAccountId.Equals(accountId)).OrderByDescending(s => s.DateTime)
                .Skip(start).Take(take).ToList();

            foreach (var n in notifications)
            {
                string id = "";
                try
                {
                    try
                    {
                        id = n.Id.ToString();
                    }
                    catch
                    {
                        id = "";
                    }

                    var notif = new UserNotification
                    {
                        Id = id,
                        Category = n.Category,
                        Type = n.Type,
                        DateTime = n.DateTime,
                        FromAccountId = n.FromAccountId,
                        FromUserDisplayName = n.FromUserDisplayName,
                        ToAccountId = n.ToAccountId,
                        ToUserDisplayName = n.ToUserDisplayName,
                        Title = n.Title,
                        Content = n.Content,
                        Options = n.Options,
                        PreserveBag = n.PreserveBag,
                        Payload = n.Payload,
                        PayloadStatus = "",
                        Read = n.Read
                    };
                    if (n.Type != "GovermentID Expired")
                    {
                        if (n.Type == EnumNotificationType.InviteFriend)
                        {
                            var result = new NetworkService().IsFriendByAccountId(accountId, n.FromAccountId);
                            if (result.Success)
                            {
                                if (result.Status == "found.normal" || result.Status == "found.trust")
                                    notif.PayloadStatus = "isFriend";
                                else if (result.Status == "found.pending")
                                    notif.PayloadStatus = "isPending";
                            }
                        }

                        var fromAccount = accountService.GetByAccountId(n.FromAccountId);
                        if (fromAccount == null) continue;
                        notif.FromProfile = new NotificationUserProfile
                        {
                            Id = fromAccount.Id.ToString(),
                            DisplayName = fromAccount.Profile.DisplayName,
                            Avatar = fromAccount.Profile.PhotoUrl
                        };
                    }

                    rs.Add(notif);
                }
                catch (Exception e)
                {
                    Log.Debug($"{e.Message}: {id} : {n.Type} : {n.DateTime} : {n.FromAccountId}");
                }
            }

            return rs;
        }

        public void MarkRead(string notifId)
        {
            repository.UpdateField(new NotificationMessage
            {
                Id = new ObjectId(notifId),
            }, "Read", true);
        }
    }
}