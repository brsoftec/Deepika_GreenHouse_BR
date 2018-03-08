using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using GH.Core.BlueCode.Entity.Message;
using Microsoft.AspNet.Identity;
using GH.Web.Areas.User.ViewModels;
using MongoDB.Bson;
using GH.Core.SignalR.Events;
using GH.Core.BlueCode.Entity.InformationVault;
using Newtonsoft.Json;
using GH.Core.BlueCode.BusinessLogic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Configuration;
using System.Web;
using System.Text.RegularExpressions;
using GH.Core.Models;
using GH.Core.ViewModels;
using Newtonsoft.Json.Linq;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("Api/PersonalMessages")]
    public class PersonalMessageController : ApiController
    {
        IAccountService _accountService;
        private IPersonalMessageService personalMessageService;
        private INetworkService networkService;


        public PersonalMessageController()
        {
            _accountService = new AccountService();
            personalMessageService = new PersonalMessageService();
            networkService = new NetworkService();
        }

        [HttpGet, Route("userId")]
        public string GetUserId()
        {
            return User.Identity.GetUserId();
        }

        [HttpGet, Route("newobjectid")]
        public string NewObjectId()
        {
            return ObjectId.GenerateNewId().ToString();
        }

        [HttpGet, Route("conversations")]
        public PersonalConversationsResult GetPrivateConversations()
        {
            //Find all friends

            var accountId = User.Identity.GetUserId();
            var user = _accountService.GetByAccountId(accountId);
            var userId = user.Id.ToString();
            var ret = new PersonalConversationsResult() {userId = userId, userName = user.Profile.DisplayName};
            var listConversations = personalMessageService.GetConversations(userId).ToList();
            var friends = networkService.GetFriends(accountId);
            var listresponseconversions = new List<PersonalConversation>();
            foreach (var friend in friends)
            {
                var conversation =
                    listConversations.FirstOrDefault(x => x.Users.Any(y => y.Id == friend.Id.ToString()));
                var conversationModel = new PersonalConversation();
                if (conversation != null)
                {
                    conversationModel.id = conversation.Id.ToString();
                    conversationModel.isGroupChat = conversation.IsGroupChat;

                    //conversationModel.
                    conversationModel.messages = conversation.Messages.Select(x => new ConversationMessage
                    {
                        conversationId = x.ConversationId,
                        messageid = x.Id.ToString(),
                        read = x.Read,
                        created = x.Created,
                        text = x.Text,
                        fromMe = !(x.ToReceiverId == userId),
                        isread = x.IsRead,
                        toReceiverId = x.ToReceiverId,
                        Datedeleted = x.DateDelete,
                        Isdeleted = x.IsDelete,
                        type = x.Type,
                        jsonFieldsdrfi = x.JsonFieldsdrfi
                    }).ToList();
                    conversationModel.unreadCount = conversationModel.messages
                        .Where(x => !x.isread && x.toReceiverId == user.Id.ToString()).Count();
                    conversationModel.userIds = conversation.Users.Select(x => x.Id.ToString()).ToList();

                    conversationModel.users = new List<ConversationFrom>();
                    foreach (var userchat in conversation.Users)
                    {
                        ConversationFrom conversationFrom = new ConversationFrom();
                        var userdatabse = _accountService.GetById(ObjectId.Parse(userchat.Id));
                        conversationFrom.avatar = userdatabse.Profile.PhotoUrl;
                        conversationFrom.name = string.IsNullOrEmpty(userdatabse.Profile.DisplayName)
                            ? userdatabse.Profile.FirstName + " " + userdatabse.Profile.LastName
                            : userdatabse.Profile.DisplayName;
                        conversationFrom.id = userchat.Id;
                        conversationFrom.online = false;
                        conversationModel.users.Add(conversationFrom);
                    }

                    conversationModel.name = conversationModel.users.Where(x => x.id == friend.Id.ToString())
                        .FirstOrDefault().name;
                    conversationModel.from = conversationModel.users.Where(x => x.id == friend.Id.ToString())
                        .FirstOrDefault();
                    // conversationModel.messages = conversation
                }
                else
                {
                    conversationModel.isGroupChat = false;
                    conversationModel.messages = new List<ConversationMessage>();
                    conversationModel.id = ObjectId.GenerateNewId().ToString();
                    conversationModel.userIds = new List<string>();
                    conversationModel.userIds.Add(userId);
                    conversationModel.userIds.Add(friend.Id.ToString());
                    conversationModel.users = new List<ConversationFrom>();
                    conversationModel.users.Add(new ConversationFrom
                    {
                        name = string.IsNullOrEmpty(user.Profile.DisplayName)
                            ? user.Profile.FirstName + " " + user.Profile.LastName
                            : user.Profile.DisplayName,
                        avatar = user.Profile.PhotoUrl,
                        id = user.Id.ToString(),
                        online = false
                    });
                    conversationModel.users.Add(new ConversationFrom
                    {
                        name = string.IsNullOrEmpty(friend.Profile.DisplayName)
                            ? friend.Profile.FirstName + " " + friend.Profile.LastName
                            : friend.Profile.DisplayName,
                        avatar = friend.Profile.PhotoUrl,
                        id = friend.Id.ToString(),
                        online = false
                    });
                    conversationModel.name = conversationModel.users.Where(x => x.id == friend.Id.ToString())
                        .FirstOrDefault().name;
                    conversationModel.from = conversationModel.users.Where(x => x.id == friend.Id.ToString())
                        .FirstOrDefault();

                    new PersonalMessageService().NewConversation(new Conversation
                    {
                        Id = ObjectId.Parse(conversationModel.id),
                        IsGroupChat = false,
                        Messages = new List<PersonalMessage>(),
                        Users = conversationModel.users.Select(x => new ConversationUser
                        {
                            Avatar = x.avatar,
                            Id = x.id,
                            Name = x.name
                        }).ToList()
                    });
                }

                listresponseconversions.Add(conversationModel);
            }

            ret.conversations = listresponseconversions;
            return ret;
        }

        #region Process FireBase

        private async Task PushToFireBaseAsync(string tokenDevide, string title, string content,
            MessageDataViewModel msgData)
        {
            if (!string.IsNullOrEmpty(content))
                content = Regex.Replace(content, @"<[^>]+>|&nbsp;", "").Trim();

            const string END_POINT = "https://fcm.googleapis.com/fcm/send";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                    ConfigurationManager.AppSettings["fcmkey"]);
                var response = await client.PostAsJsonAsync(END_POINT, new
                {
                    to = tokenDevide,
                    notification = new
                    {
                        title = title,
                        body = content,

                        sound = "default",
                        click_action = "fcm.ACTION.notification"
                    },
                    data = new
                    {
                        title = "Regit.Today",
                        body = msgData,
                        click_action = "fcm.ACTION.HELLO",
                        remote = true
                    },
                    priority = "high"
                });
                response.EnsureSuccessStatusCode();
            }
        }


        #endregion

        public bool CheckExistAccount(List<ConversationUser> users)
        {
            var rs = true;
            foreach (var item in users)
            {
                var id = new ObjectId(item.Id);
                var user = _accountService.GetById(id);
                if (user == null)
                    rs = false;
            }

            return rs;
        }

        private static PersonalConversation GetPersonalConversation(Conversation conversation, string userId)
        {
            var conversationModel = new PersonalConversation();

            conversationModel.id = conversation.Id.ToString();
            conversationModel.isGroupChat = conversation.IsGroupChat;
            conversationModel.name = conversation.Name;

            conversationModel.messages = conversation.Messages.Select(x => new ConversationMessage()
            {
                conversationId = conversation.Id.ToString(),
                fromMe = x.FromMe,
                created = x.Created,
                text = x.Text,
                toReceiverId = x.ToReceiverId
            }).ToList();
            conversationModel.owner = conversation.Owner;
            conversationModel.users = conversation.Users.Select(x => new ConversationFrom()
            {
                avatar = x.Avatar,
                id = x.Id,
                name = x.Name,
            }).ToList();

            if (!conversation.IsGroupChat)
            {
                var fromUser = conversation.Users.First(x => x.Id != userId);
                conversationModel.@from = new ConversationFrom
                {
                    avatar = fromUser.Avatar,
                    name = fromUser.Name,
                    id = fromUser.Id,
                };
                conversationModel.name = conversation.IsGroupChat ? conversation.Name : fromUser.Name;
            }

            return conversationModel;
        }

        private static PersonalConversation GetPersonalConversation(string user1, string userId2)
        {
            var conversationModel = new PersonalConversation();


            //conversationModel.id = conversation.Id.ToString();
            //conversationModel.isGroupChat = conversation.IsGroupChat;
            //conversationModel.name = conversation.Name;

            //conversationModel.messages = conversation.Messages.Select(x => new ConversationMessage()
            //{
            //    conversationId = conversation.Id.ToString(),
            //    fromMe = x.FromMe,
            //    created = x.Created,
            //    text = x.Text,
            //    toReceiverId = x.ToReceiverId
            //}).ToList();
            //conversationModel.owner = conversation.Owner;
            //conversationModel.users = conversation.Users.Select(x => new ConversationFrom()
            //{
            //    avatar = x.Avatar,
            //    id = x.Id,
            //    name = x.Name,
            //}).ToList();

            //if (!conversation.IsGroupChat)
            //{
            //    var fromUser = conversation.Users.First(x => x.Id != userId);
            //    conversationModel.@from = new ConversationFrom
            //    {
            //        avatar = fromUser.Avatar,
            //        name = fromUser.Name,
            //        id = fromUser.Id,
            //    };
            //    conversationModel.name = conversation.IsGroupChat ? conversation.Name : fromUser.Name;
            //}
            return conversationModel;
        }

        [HttpPost, Route("addmessages")]
        public async Task<string> AddPersonalMessage(ConversationMessage conversationMessage)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserAccountId);
            var accountId = account.AccountId;
            var displayName = account.Profile.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = account.Profile.FirstName + account.Profile.LastName;
            }

            var personalMessage = new PersonalMessage
            {
                Id = ObjectId.GenerateNewId(),
                ConversationId = conversationMessage.conversationId,
                Created = DateTime.Now,
                FromMe = conversationMessage.fromMe.HasValue ? conversationMessage.fromMe.Value : false,
                Text = conversationMessage.text,
                Read = conversationMessage.read,
                FromAccountId = accountId,
                ToReceiverId = conversationMessage.toReceiverId,
                IsRead = false,
                Type = conversationMessage.type,
                JsonFieldsdrfi = conversationMessage.jsonFieldsdrfi
            };
            var toAccount = _accountService.GetById(ObjectId.Parse(conversationMessage.toReceiverId));
            if (personalMessage.Type == "drfi" && !string.IsNullOrEmpty(personalMessage.JsonFieldsdrfi))
            {
                var list = new InfomationVaultBusinessLogic().getInformationvaultforDRFI(toAccount.AccountId,
                    conversationMessage.jsonFieldsdrfi);
                personalMessage.JsonFieldsdrfi = JsonConvert.SerializeObject(list);
            }

            var title = displayName;
            var content = personalMessage.Text;
            if (personalMessage.Type == "drfi")
            {
                title = displayName + " - DRFI Request";
                content = displayName + " is requesting your information.";
                personalMessage.Status = "sent";
            }
            else if (personalMessage.Type != null && personalMessage.Type.StartsWith("drfiresponse"))
            {
                var status = personalMessage.Type == "drfiresponseaccepted" ? "accepted" : "denied";
                title = displayName + " - DRFI Response";
                content = displayName + $" {status} your request for information.";
                var drfiRequestId = personalMessage.DrfiRequestId = conversationMessage.drfiRequestId;
                var result = MessagingService.SetMessageStatus(drfiRequestId, status);
            }
            
            await personalMessageService.AddPersonalMessageAsync(personalMessage);

/*
            var msgData = new MessageDataViewModel();
            msgData.AccountId = accountId;
            msgData.ConversationId = personalMessage.ConversationId;
            msgData.Messageid = personalMessage.Id.ToString();
            msgData.MessageType = personalMessage.Type;
            msgData.Message = personalMessage.Text;
            msgData.Type = "addmessage";
            msgData.DateDelete = personalMessage.DateDelete;
            msgData.Create = personalMessage.Created;
            msgData.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

            var privateMessageConstrainedEvent =
                new MessageSingleConstrainedEvent(toAccount.AccountId, conversationMessage.text);
            privateMessageConstrainedEvent.MessageType = personalMessage.Type;

            privateMessageConstrainedEvent.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

            privateMessageConstrainedEvent.Messageid = personalMessage.Id.ToString();
            privateMessageConstrainedEvent.Type = "addmessage";
            privateMessageConstrainedEvent.ConversationId = personalMessage.ConversationId.ToString();
            var eventAggregator =
                (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                    typeof(Caliburn.Micro.IEventAggregator));

            eventAggregator.Publish(privateMessageConstrainedEvent); */

            var fcmResult = await new AuthTokensLogic().PostToFcmAsync(toAccount, account, "message", 
                $"Message from {account.Profile.DisplayName}", content, personalMessage);


/*            var tokenDeviceLogic = new ManageTokenDeviceBusinessLogic();
            var tokenDevide = tokenDeviceLogic.GetListManageTokenDeviceByAccountId(accountReceiverId.AccountId);
            if (tokenDevide == null) throw new Exception($"Not Found With UserId: {accountReceiverId.AccountId}");
            foreach(var token in tokenDevide)
            {
               Task.Factory.StartNew(() => PushToFireBaseAsync(token.TokenDevice, title, content, msgData)).Wait();
            }*/

            return personalMessage.Id.ToString();
        }

        [HttpPost, Route("deletemessages")]
        public void DeletePersonalMessage(ConversationMessage conversationMessage)
        {
            new PersonalMessageService().DeletePerosnalMessage(conversationMessage.conversationId,
                conversationMessage.messageid, conversationMessage.Datedeleted);
            var accountReceiverId = _accountService.GetById(ObjectId.Parse(conversationMessage.toReceiverId));
            var privateMessageConstrainedEvent =
                new MessageSingleConstrainedEvent(accountReceiverId.AccountId, conversationMessage.text);
            privateMessageConstrainedEvent.Type = "deletemessage";
            privateMessageConstrainedEvent.Messageid = conversationMessage.messageid;
            privateMessageConstrainedEvent.ConversationId = conversationMessage.conversationId.ToString();
            privateMessageConstrainedEvent.DateDelete = conversationMessage.Datedeleted ?? DateTime.MinValue;
            var eventAggregator =
                (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                    typeof(Caliburn.Micro.IEventAggregator));
            eventAggregator.Publish(privateMessageConstrainedEvent);
        }

        public class Modelupdateunread
        {
            public string conversationid { set; get; }
            public string conversationmessageid { set; get; }
        }

        [HttpPost, Route("updateunread")]
        public string updateunread(Modelupdateunread model)
        {
            var accountId = User.Identity.GetUserId();
            var user = _accountService.GetByAccountId(accountId);
            new PersonalMessageService().updateUnreadConversation(model.conversationid, model.conversationmessageid,
                user.Id.ToString());
            return "";
        }

        public class ModelFieldvaults
        {
            public string jsonfieldvaults { set; get; }
        }

        [HttpPost, Route("GetInformationvaultforDRFI")]
        public List<FieldinformationVault> GetInformationvaultforDRFI(ModelFieldvaults model)
        {
            return new InfomationVaultBusinessLogic().getInformationvaultforDRFI(User.Identity.GetUserId(),
                model.jsonfieldvaults);
        }

        [HttpPost, Route("addgroupchat")]
        public string AddGroupChat(PersonalConversation personalConversation)
        {
            var accountId = User.Identity.GetUserId();
            var user = _accountService.GetByAccountId(accountId);
            var conversation = new Conversation()
            {
                Id = ObjectId.GenerateNewId(),
                IsGroupChat = personalConversation.isGroupChat,
                Name = personalConversation.name,
                Owner = user.Profile.DisplayName,
                //Messages = personalConversation.messages
                //Users = personalConversation.users
            };
            if (personalConversation.messages != null && personalConversation.messages.Count > 0)
            {
                conversation.Messages = personalConversation.messages.Select(x => new PersonalMessage()
                {
                    Id = ObjectId.GenerateNewId(),
                    ConversationId = conversation.Id.ToString(),
                    Created = DateTime.Now,
                    Text = x.text,
                }).ToList();
            }

            if (personalConversation.userIds != null && personalConversation.userIds.Count > 0)
            {
                var ids = personalConversation.userIds.Select(x => new ObjectId(x)).ToList();
                var users = _accountService.GetByListId(ids);
                conversation.Users = users.Select(u => new ConversationUser()
                {
                    Avatar = u.Profile.PhotoUrl,
                    Id = u.Id.ToString(),
                    Name = u.Profile.DisplayName,
                    Online = true
                }).ToList();
            }

            if (conversation.Users != null && conversation.Users.All(x => x.Id != user.Id.ToString()))
            {
                conversation.Users.Add(new ConversationUser()
                {
                    Avatar = user.Profile.PhotoUrl,
                    Id = user.Id.ToString(),
                    Name = user.Profile.DisplayName,
                    Online = true,
                    IsOwner = true
                });
            }

            personalMessageService.AddGroupChat(conversation);
            return conversation.Id.ToString();
        }
    }
}