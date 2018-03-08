using GH.Core.Services;
using System;
using System.Collections;
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
using System.Net;
using System.Text.RegularExpressions;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.ViewModels;
using JavaPort;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [ApiAuthorize]
    [RoutePrefix("Api/Chat")]
    public class MessagingApiController : BaseApiController
    {
        private static readonly IAccountService AccountService = new AccountService();
        private static readonly IPersonalMessageService MessageService = new PersonalMessageService();
        private static readonly INetworkService NetworkService = new NetworkService();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        private static readonly Dictionary<string, string> LegacyEmojiMap = new Dictionary<string, string>
        {
            ["01"] = "\U0001f604",
            ["02"] = "\U0001f603",
            ["09"] = "\U0001f60C",
            ["10"] = "\U0001f60C",
            ["13"] = "\U0001f600",
            ["21"] = "\U0001f60A"
        };

        public MessagingApiController()
        {
        }

        [HttpGet, Route("Conversations")]
        public async Task<PersonalConversationsResult> GetConversationsAsync()
        {
            var ret = new PersonalConversationsResult() {userId = UserId, userName = Account.Profile.DisplayName};
            var listConversations = MessageService.GetConversations(UserId).ToList();
            var friends = NetworkService.GetFriends(AccountId);
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
                        fromMe = x.ToReceiverId != UserId,
                        isread = x.IsRead,
                        toReceiverId = x.ToReceiverId,
                        Datedeleted = x.DateDelete == DateTime.MinValue ? null : (DateTime?) x.DateDelete,
                        Isdeleted = x.IsDelete,
                        type = x.Type,
                        jsonFieldsdrfi = x.JsonFieldsdrfi
                    }).ToList();

                    foreach (var message in conversationModel.messages)
                    {
                        var rgx = new Regex(@"<img .*?src=""/.+?/img/emojis/(\d+).png"".*?>");
                        var msg = rgx.Replace(message.text, m =>
                        {
                            var code = m.Value.Trim(':');
                            if (!LegacyEmojiMap.TryGetValue(code, out var str))
                                return "\U0001f600";
                            return str;
                        });
                        rgx = new Regex(@":(\d+):");
                        msg = rgx.Replace(msg, m =>
                        {
                            var code = m.Value.Trim(':');
                            if (!LegacyEmojiMap.TryGetValue(code, out var str))
                                return "\U0001f600";
                            return str;
                        });
                        rgx = new Regex(@"<[a-zA-Z]+>(.*)</[a-zA-Z]+>");
                        msg = rgx.Replace(msg, @"$1");
                        rgx = new Regex(@"(&[a-zA-Z\d]{2,5};)");
                        msg = rgx.Replace(msg, " ");
                        message.text = msg;
                    }

                    conversationModel.unreadCount = conversationModel.messages
                        .Count(x => !x.isread && x.toReceiverId == UserId);
                    conversationModel.userIds = conversation.Users.Select(x => x.Id.ToString()).ToList();

                    conversationModel.users = new List<ConversationFrom>();
                    foreach (var userchat in conversation.Users)
                    {
                        ConversationFrom conversationFrom = new ConversationFrom();
                        var userdatabse = AccountService.GetById(ObjectId.Parse(userchat.Id));
                        conversationFrom.avatar = userdatabse.Profile.PhotoUrl;
                        conversationFrom.name = string.IsNullOrEmpty(userdatabse.Profile.DisplayName)
                            ? userdatabse.Profile.FirstName + " " + userdatabse.Profile.LastName
                            : userdatabse.Profile.DisplayName;
                        conversationFrom.id = userchat.Id;
                        conversationFrom.online = false;
                        conversationModel.users.Add(conversationFrom);
                    }

                    conversationModel.name = conversationModel.users.FirstOrDefault(x => x.id == friend.Id.ToString())
                        ?.name;
                    conversationModel.from = conversationModel.users.FirstOrDefault(x => x.id == friend.Id.ToString());
                    // conversationModel.messages = conversation
                }
                else
                {
                    conversationModel.isGroupChat = false;
                    conversationModel.messages = new List<ConversationMessage>();
                    conversationModel.id = ObjectId.GenerateNewId().ToString();
                    conversationModel.userIds = new List<string>();
                    conversationModel.userIds.Add(UserId);
                    conversationModel.userIds.Add(friend.Id.ToString());
                    conversationModel.users = new List<ConversationFrom>();
                    conversationModel.users.Add(new ConversationFrom
                    {
                        name = string.IsNullOrEmpty(Account.Profile.DisplayName)
                            ? Account.Profile.FirstName + " " + Account.Profile.LastName
                            : Account.Profile.DisplayName,
                        avatar = Account.Profile.PhotoUrl,
                        id = Account.Id.ToString(),
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
                    conversationModel.name =
                        conversationModel.users.FirstOrDefault(x => x.id == friend.Id.ToString())?.name;
                    conversationModel.from = conversationModel.users.FirstOrDefault(x => x.id == friend.Id.ToString());

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

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ConversationSummary
        {
            public string ConversationId { get; set; }
            public string FromAccountId { get; set; }
            public EmbeddedProfile FromProfile { get; set; }
            public int MessagesCount { get; set; }
            public int IncomingMessagesCount { get; set; }
            public int UnreadMessagesCount { get; set; }
            public int UnreadIncomingMessagesCount { get; set; }
            public int DeletedMessagesCount { get; set; }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ConversationsSummary
        {
            public List<ConversationSummary> Conversations;
        }

        [HttpGet, Route("Conversations/List")]
        public async Task<HttpResponseMessage> ListConversationsAsync()
        {
            var conversations = MessageService.GetConversations(UserId).ToList();
            var convs = new List<ConversationSummary>();
            foreach (var conversation in conversations)
            {
                var fromUser = conversation.Users.FirstOrDefault(u => u.Id != UserId);
                if (fromUser == null) continue;
                var fromAccount = new AccountService().GetById(fromUser.Id);
                if (fromAccount == null) continue;
                var conv = new ConversationSummary
                {
                    ConversationId = conversation.Id.ToString(),
                    FromAccountId = fromAccount.AccountId,
                    FromProfile = new EmbeddedProfile
                    {
                        Id = fromAccount.Id.ToString(),
                        DisplayName = fromAccount.Profile.DisplayName,
                        Avatar = fromAccount.Profile.PhotoUrl
                    },
                    MessagesCount = conversation.Messages.Count,
                    IncomingMessagesCount = conversation.Messages
                        .Count(x => x.ToReceiverId == UserId),
                    UnreadMessagesCount = conversation.Messages
                        .Count(x => !x.IsRead),
                    UnreadIncomingMessagesCount = conversation.Messages
                        .Count(x => !x.IsRead && x.ToReceiverId == UserId),
                    DeletedMessagesCount = conversation.Messages
                        .Count(x => x.IsDelete)
                };
                convs.Add(conv);
            }

            return Request.CreateSuccessResponse(convs, $"List {convs.Count} conversations");
        }

        [HttpGet, Route("Conversation")]
        public async Task<HttpResponseMessage> GetConversationAsync(string withUserId = "", string withAccountId = "")
        {
            Account user = null;
            if (string.IsNullOrEmpty(withUserId))
            {
                if (string.IsNullOrEmpty(withAccountId))
                    return Request.CreateApiErrorResponse("Missing user ID", HttpStatusCode.BadRequest,
                        "parameter.missing");
                user = AccountService.GetByAccountId(withAccountId);
                if (user == null)
                    return Request.CreateApiErrorResponse("Account not found", error: "account.notFound");
                withUserId = user.Id.ToString();
            }

            if (user == null)
            {
                if (!ObjectId.TryParse(withUserId, out var userId))
                    return Request.CreateApiErrorResponse("Invalid user", error: "userId.invalid");
                user = AccountService.GetById(userId);
                if (user == null)
                    return Request.CreateApiErrorResponse("Account not found", error: "account.notFound");
            }

            var listConversations = MessageService.GetConversations(UserId).ToList();

            var conversation =
                listConversations.FirstOrDefault(x => x.Users.Any(y => y.Id == withUserId));

            if (conversation == null)
            {
                return Request.CreateApiErrorResponse("Conversation not found", error: "chat.conversation.notFound");
            }

            var conversationModel = new PersonalConversation
            {
                from = new ConversationFrom
                {
                    name = user.Profile.DisplayName,
                    avatar = user.Profile.PhotoUrl,
                    id = withUserId,
                }
            };


            conversationModel.id = conversation.Id.ToString();

            conversationModel.messages = conversation.Messages.Select(x =>
            {
                var msg = new ConversationMessage
                {
                    conversationId = x.ConversationId,
                    messageid = x.Id.ToString(),
                    read = x.Read,
                    created = x.Created,
                    text = x.Text,
                    fromMe = x.ToReceiverId != UserId,
                    fromAccountId = x.FromAccountId,
                    isread = x.IsRead,
                    toReceiverId = x.ToReceiverId,
                    Datedeleted = x.DateDelete == DateTime.MinValue ? null : (DateTime?) x.DateDelete,
                    Isdeleted = x.IsDelete,
                    type = x.Type,
                    jsonFieldsdrfi = x.JsonFieldsdrfi
                };
                if (x.Type == "drfi")
                    msg.status = x.Status;
                else if (x.Type != null && x.Type.StartsWith("drfiresponse"))
                    msg.drfiRequestId = x.DrfiRequestId;
                return msg;
            }).ToList();

            var vaultFields = await VaultService.ListFieldsAsync();

            foreach (var message in conversationModel.messages)
            {
                try
                {
                    var rgx = new Regex(@"<img .*?src=""/.+?/img/emojis/(\d+).png"".*?>");
                    var msg = rgx.Replace(message.text, m =>
                    {
                        var code = m.Value.Trim(':');
                        if (!LegacyEmojiMap.TryGetValue(code, out var str))
                            return "\U0001f600";
                        return str;
                    });
                    rgx = new Regex(@":(\d+):");
                    msg = rgx.Replace(msg, m =>
                    {
                        var code = m.Value.Trim(':');
                        if (!LegacyEmojiMap.TryGetValue(code, out var str))
                            return "\U0001f600";
                        return str;
                    });
                    rgx = new Regex(@"<[a-zA-Z]+>(.*)</[a-zA-Z]+>");
                    msg = rgx.Replace(msg, @"$1");
                    rgx = new Regex(@"(&[a-zA-Z\d]{2,5};)");
                    msg = rgx.Replace(msg, " ");
                    message.text = msg;

                    var json = message.jsonFieldsdrfi;
                    List<FieldinformationVault> fields = null;
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                        }
                        catch
                        {
                        }
                    }

                    if (fields != null)
                    {
                        var drfiFields = new List<UserDrfiField>();
                        foreach (var field in fields)
                        {
                            var type = field.type;
                            string value = field.model;
                            var valueObj = value == null
                                ? new UserFieldValue
                                {
                                    Text = null,
                                    List = null,
                                    Json = null,
                                }
                                : new UserFieldValue();


                            var optionArray = new object[] { };
                            try
                            {
                                switch (field.options)
                                {
                                    case null:
                                        break;
                                    case JArray jarray:
                                        optionArray = jarray.ToArray<object>();
                                        break;
                                    case string[] optionArr:
                                        optionArray = optionArr.toArray<object>();
                                        break;
                                    case BsonArray bsonArray:
                                        optionArray = bsonArray.ToArray<object>();
                                        break;
                                    case List<BsonValue> optionArr:
                                        optionArray = optionArr.ToArray<object>();
                                        break;
                                    case string optionStr:
                                        if (string.IsNullOrEmpty(optionStr))
                                            optionArray = new object[] { };
                                        else if (optionStr == "[]")
                                            optionArray = new object[] { };
                                        else
                                            optionArray = new object[] {optionStr};
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug($"{field.jsPath}: {e.Message}");
                            }

                            if (optionArray.Length == 0)
                            {
                                var vField = vaultFields.FirstOrDefault(f => f.Path == field.jsPath);
                                if (vField != null)
                                {
                                    optionArray = vField.Options;
                                }
                            }

                            if (value != null)
                            {
                                try
                                {
                                    switch (type)
                                    {
                                        case "textbox"
                                            when optionArray.Length > 0 && optionArray[0].ToString() == "phone":
                                            var result = PhoneNumberHelper.GetFormattedPhone(value);
                                            valueObj.Text = result.Success
                                                ? ((FormattedPhone) result.Data).CountryCode + " " +
                                                  ((FormattedPhone) result.Data).PhoneNumber
                                                : value;
                                            break;
                                        case "date":
                                        case "datecombo":
                                            if (DateTime.TryParse(value, out var date))
                                                valueObj.Text = date.ToString("yyyy-MM-dd");
                                            break;
                                        case "smartinput":
                                        case "tagsinput":
                                            valueObj.List = value == "[]"
                                                ? new object[] { }
                                                : value.Split(',').ToArray<object>();
                                            break;

                                        case "location":
                                            valueObj.Json = JsonConvert.SerializeObject(new
                                            {
                                                country = field.model,
                                                city = field.unitModel
                                            });
                                            break;
                                        case "numinput":
                                            valueObj.Json = JsonConvert.SerializeObject(new
                                            {
                                                amount = field.model,
                                                unit = field.unitModel
                                            });
                                            break;
                                        default:
                                            if (value != null)
                                                valueObj.Text = value;
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug($"{field.jsPath}: {e.Message}");
                                }
                            }

                            var pField = new UserDrfiField
                            {
                                Path = field.jsPath,
                                Title = field.displayName,
                                Type = type,
                                Options = optionArray,
                                Value = valueObj
                            };
                            drfiFields.Add(pField);
                        }

                        if (drfiFields.Count > 0)
                            message.drfiFields = drfiFields;
                    }
                }
                catch (Exception e)
                {
                    Log.Debug($"{message.messageid}: {message.text}");
                }
            }

            conversationModel.userIds = conversation.Users.Select(x => x.Id.ToString()).ToList();

            conversationModel.unreadCount = conversationModel.messages
                .Count(x => !x.isread && x.toReceiverId == UserId);

            //conversationModel.users = new List<ConversationFrom>();

//            conversationModel.name = conversationModel.users.FirstOrDefault(x => x.id == withUserId)
//                ?.name;
//            conversationModel.from = conversationModel.users.FirstOrDefault(x => x.id == withUserId);

            return Request.CreateSuccessResponse(conversationModel, "Conversation details");
        }

        public bool CheckExistAccount(List<ConversationUser> users)
        {
            var rs = true;
            foreach (var item in users)
            {
                var id = new ObjectId(item.Id);
                var user = AccountService.GetById(id);
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
            return conversationModel;
        }

        [HttpPost, Route("Message/Add")]
        public async Task<HttpResponseMessage> AddMessageAsync(ConversationMessage conversationMessage)
        {
            if (conversationMessage == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(conversationMessage.conversationId))
                return Request.CreateApiErrorResponse("Missing conversation ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(conversationMessage.toReceiverId))
                return Request.CreateApiErrorResponse("Missing recipient ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(conversationMessage.text))
                return Request.CreateApiErrorResponse("Empty message", HttpStatusCode.BadRequest);

            if (!ObjectId.TryParse(conversationMessage.conversationId, out var conversationId))
                return Request.CreateApiErrorResponse("Invalid conversation");

            var conversationCollection = MongoDBConnection.Database.GetCollection<Conversation>("Conversation");
            var builder = Builders<Conversation>.Filter;
            var filter = builder.Eq("Id", conversationId);
            var conversation = await conversationCollection.Find(filter).FirstOrDefaultAsync();
            if (conversation == null)
                return Request.CreateApiErrorResponse("Invalid conversation");

            var displayName = Account.Profile.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = Account.Profile.FirstName + Account.Profile.LastName;
            }

            var rgx = new Regex(@":(\d+):");
            var msg = rgx.Replace(conversationMessage.text,
                @"<img class=""msg-emoji"" src=""/Content/img/emojis/$1.png"">");

            var personalMessage = new PersonalMessage
            {
                Id = ObjectId.GenerateNewId(),
                ConversationId = conversationMessage.conversationId,
                FromMe = true, //onversationMessage.fromMe ?? false,
                FromAccountId = AccountId,
                ToReceiverId = conversationMessage.toReceiverId,
                Text = msg,
                Read = conversationMessage.read,
                Type = conversationMessage.type,
                JsonFieldsdrfi = conversationMessage.jsonFieldsdrfi
            };

            if (!ObjectId.TryParse(conversationMessage.toReceiverId, out var receiverId))
                return Request.CreateApiErrorResponse("Invalid recipient");
            ;
            var receiver = AccountService.GetById(receiverId);
            if (receiver == null)
                return Request.CreateApiErrorResponse("Recipient not found");
            if (personalMessage.Type == "drfi")
            {
                var vaultFields = await VaultService.ListFieldsAsync();
                personalMessage.Text = "[DRFI Request]";
                var json = personalMessage.JsonFieldsdrfi;
                var fields = new List<FieldinformationVault>();
                var paths = conversationMessage.drfiPaths;
                if (paths != null && paths.Count > 0)
                {
                    foreach (var path in paths)
                    {
                        var vField = vaultFields.FirstOrDefault(f => f.Path == path);
                        var field = new FieldinformationVault
                        {
                            jsPath = path,
                            label = vField?.Title,
                            displayName = vField?.Title,
                            type = vField?.Type,
                            options = vField?.Options
                        };
                        fields.Add(field);
                    }

                    json = JsonConvert.SerializeObject(fields);
                }

                if (!string.IsNullOrEmpty(json))
                {
                    var list = new InfomationVaultBusinessLogic().getInformationvaultforDRFI(receiver.AccountId,
                        json);
                    personalMessage.JsonFieldsdrfi = JsonConvert.SerializeObject(list);
                }
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
                personalMessage.Text = "[DRFI Response]";
                var status = personalMessage.Type == "drfiresponseaccepted" ? "accepted" : "denied";
                title = displayName + " - DRFI Response";
                content = displayName + $" {status} your request for information.";
                var drfiRequestId = personalMessage.DrfiRequestId = conversationMessage.drfiRequestId;
                var result = MessagingService.SetMessageStatus(drfiRequestId, status);
            }

            await MessageService.AddPersonalMessageAsync(personalMessage);

/*            var msgData = new MessageDataViewModel();
            msgData.AccountId = AccountId;
            msgData.ConversationId = personalMessage.ConversationId;
            msgData.Messageid = personalMessage.Id.ToString();
            msgData.MessageType = personalMessage.Type;
            msgData.Message = personalMessage.Text;
            msgData.Type = "addmessage";
            msgData.DateDelete = personalMessage.DateDelete;
            msgData.Create = personalMessage.Created;
            msgData.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

            var privateMessageConstrainedEvent =
                new MessageSingleConstrainedEvent(receiver.AccountId, conversationMessage.text);
            privateMessageConstrainedEvent.MessageType = personalMessage.Type;

            privateMessageConstrainedEvent.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

            privateMessageConstrainedEvent.Messageid = personalMessage.Id.ToString();
            privateMessageConstrainedEvent.Type = "addmessage";
            privateMessageConstrainedEvent.ConversationId = personalMessage.ConversationId;
            var eventAggregator =
                (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                    typeof(Caliburn.Micro.IEventAggregator));

            eventAggregator.Publish(privateMessageConstrainedEvent);*/

            var fcmResult = await new AuthTokensLogic().PostToFcmAsync(receiver, Account, "message",
                $"Message from {Account.Profile.DisplayName}", content, personalMessage);
            if (fcmResult.Data != null)
                return Request.CreateSuccessResponse(new
                {
                    conversationId = personalMessage.ConversationId,
                    messageId = personalMessage.Id.ToString(),
                    fcmResponses = fcmResult.Data
                }, "Message created successfully");
            return Request.CreateSuccessResponse(new
            {
                conversationId = personalMessage.ConversationId,
                messageId = personalMessage.Id.ToString(),
            }, "Message created successfully");
        }

        [HttpPost, Route("Message/Delete")]
        public HttpResponseMessage DeletePersonalMessage(ConversationMessage conversationMessage)
        {
            if (conversationMessage == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(conversationMessage.messageid))
                return Request.CreateApiErrorResponse("Missing message ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(conversationMessage.toReceiverId))
                return Request.CreateApiErrorResponse("Missing recipient ID", HttpStatusCode.BadRequest);

            var result = new PersonalMessageService().DeletePerosnalMessage(conversationMessage.conversationId,
                conversationMessage.messageid, conversationMessage.Datedeleted);
            if (!result)
                return Request.CreateApiErrorResponse("Message not found");
            var accountReceiverId = AccountService.GetById(ObjectId.Parse(conversationMessage.toReceiverId));
            if (accountReceiverId != null)
            {
                var privateMessageConstrainedEvent =
                    new MessageSingleConstrainedEvent(accountReceiverId.AccountId, conversationMessage.text);
                privateMessageConstrainedEvent.Type = "deletemessage";
                privateMessageConstrainedEvent.Messageid = conversationMessage.messageid;
                privateMessageConstrainedEvent.ConversationId = conversationMessage.conversationId.ToString();
                privateMessageConstrainedEvent.DateDelete = conversationMessage.Datedeleted ?? DateTime.UtcNow;
                var eventAggregator =
                    (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                        typeof(Caliburn.Micro.IEventAggregator));
                eventAggregator.Publish(privateMessageConstrainedEvent);
            }

            return Request.CreateSuccessResponse(new
            {
                conversationId = conversationMessage.conversationId,
                messageId = conversationMessage.messageid
            }, "Message deleted successfully");
        }

        [HttpPost, Route("Message/MarkRead/{messageId}")]
        public async Task<HttpResponseMessage> MarkReadMessageAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                return Request.CreateApiErrorResponse("Missing message ID", HttpStatusCode.BadRequest);

            var result = await new PersonalMessageService().MarkReadMessageAsync(messageId, UserId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "message.invalid":
                        return Request.CreateApiErrorResponse("Invalid message ID", error: "message.invalid");
                    case "message.notFound":
                        return Request.CreateApiErrorResponse("Message not found", error: "message.notFound");
                    case "message.notReceiver":
                        return Request.CreateApiErrorResponse("Only receiver can mark read message",
                            error: "message.notReceiver");
                    case "message.update.not":
                        return Request.CreateApiErrorResponse("Message already read", error: "message.already.read");
                    default:
                        return Request.CreateApiErrorResponse("Error marking read messagge",
                            error: "message.markkread.error");
                }
            }

            return Request.CreateSuccessResponse(new
            {
                messageId,
                conversationId = result.Data,
                isRead = true
            }, "Message marked read");
        }
        
        [HttpPost, Route("Conversation/MarkRead/{conversationId}")]
        public async Task<HttpResponseMessage> MarkReadConversationAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                return Request.CreateApiErrorResponse("Missing conversation ID", HttpStatusCode.BadRequest);

            var result = await new PersonalMessageService().MarkReadConversationAsync(conversationId, UserId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "conversation.invalid":
                        return Request.CreateApiErrorResponse("Invalid conversation ID", error: "message.invalid");
                    case "conversation.notFound":
                        return Request.CreateApiErrorResponse("Conversation not found", error: "message.notFound");
                    case "conversation.notMember":
                        return Request.CreateApiErrorResponse("Only member of conversation can mark read messages",
                            error: "conversation.notMember");
                    case "conversation.update.not":
                        return Request.CreateApiErrorResponse("Messages already read", error: "conversation.already.read");
                    default:
                        return Request.CreateApiErrorResponse("Error marking read conversation",
                            error: "conversation.markkread.error");
                }
            }

            return Request.CreateSuccessResponse(new
            {
                conversationId,
                isRead = true,
                messagesMarkedRead = result.Data
            }, "Conversation messages marked read");
        }

        public class Modelupdateunread
        {
            public string conversationid { set; get; }
            public string conversationmessageid { set; get; }
        }

        [HttpPost, Route("updateunread")]
        public string updateunread(Modelupdateunread model)
        {
            new PersonalMessageService().updateUnreadConversation(model.conversationid, model.conversationmessageid,
                UserId);
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

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class DrfiAcceptModel
        {
            public string MessageId { get; set; }
            public IList<UserFieldData> Fields { get; set; }
        }

        [HttpPost, Route("Drfi/Accept")]
        public async Task<HttpResponseMessage> AcceptDrfiRequestAsync(DrfiAcceptModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            var messageId = model.MessageId;
            if (string.IsNullOrEmpty(messageId))
                return Request.CreateApiErrorResponse("Missing message ID", HttpStatusCode.BadRequest);
            var fields = model.Fields;
            if (fields == null || fields.Count == 0)
                return Request.CreateApiErrorResponse("Missing user data", HttpStatusCode.BadRequest);

            var result = MessagingService.GetMessage(messageId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "message.invalid":
                        return Request.CreateApiErrorResponse("Invalid message ID", error: "message.invalid");
                    case "message.notFound":
                        return Request.CreateApiErrorResponse("Message not found", error: "message.notFound");
                }

                return Request.CreateApiErrorResponse("Error getting message", error: "message.get.error");
            }

            var message = (PersonalMessage) result.Data;
            if (message.Type != "drfi")
                return Request.CreateApiErrorResponse("Message not DRFI request", error: "message.noDrfi");
            if (message.ToReceiverId != UserId)
                return Request.CreateApiErrorResponse("Only recipient allowed to accept DRFI request",
                    error: "drfi.recipient.invalid");
            if (message.Status == "accepted" || message.Status == "denied")
                return Request.CreateApiErrorResponse($"DRFI request already {message.Status}",
                    error: "drfi.already.completed");

            var fromAccountId = message.FromAccountId;
            var fromAccount = AccountService.GetByAccountId(fromAccountId);
            if (string.IsNullOrEmpty(fromAccountId))
                return Request.CreateApiErrorResponse("Error getting DRFI request", error: "drfi.sender.invalid");

            result = await MessagingService.AcceptDrfiMessageAsync(message, fields, Account, fromAccount);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "field.null":
                        return Request.CreateApiErrorResponse($"Missing value for field: {result.Data}",
                            error: "drfi.field.empty");
                    case "field.invalid":
                        return Request.CreateApiErrorResponse($"Value of invalid data type for field: {result.Data}",
                            error: "drfi.field.invalid");
                    case "field.invalid.format":
                        return Request.CreateApiErrorResponse($"Invalid format for field: {result.Data}",
                            error: "drfi.field.invalid.format");
                    case "field.invalid.value":
                        return Request.CreateApiErrorResponse($"Invalid value for field: {result.Data}",
                            error: "drfi.field.invalid.value");
                }

                return Request.CreateApiErrorResponse("Error accepting DRFI request", error: "drfi.accept.error");
            }

            return Request.CreateSuccessResponse(new
            {
                requestMessageId = messageId,
                responseMessageId = result.Data,
                status = "accepted"
            }, "DRFI request accepted");
        }

        [HttpPost, Route("Drfi/Deny")]
        public async Task<HttpResponseMessage> DenyDrfiRequest(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                return Request.CreateApiErrorResponse("Missing message ID", HttpStatusCode.BadRequest);

            var result = MessagingService.GetMessage(messageId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "message.invalid":
                        return Request.CreateApiErrorResponse("Invalid message ID", error: "message.invalid");
                    case "message.notFound":
                        return Request.CreateApiErrorResponse("Message not found", error: "message.notFound");
                }

                return Request.CreateApiErrorResponse("Error getting message", error: "message.get.error");
            }

            var message = (PersonalMessage) result.Data;
            if (message.Type != "drfi")
                return Request.CreateApiErrorResponse("Message not DRFI request", error: "message.noDrfi");
            if (message.ToReceiverId != UserId)
                return Request.CreateApiErrorResponse("Only recipient allowed to deny DRFI request",
                    error: "drfi.recipient.invalid");
            if (message.Status == "accepted" || message.Status == "denied")
                return Request.CreateApiErrorResponse($"DRFI request already {message.Status}",
                    error: "drfi.already.completed");

            var fromAccountId = message.FromAccountId;
            var fromAccount = AccountService.GetByAccountId(fromAccountId);
            if (string.IsNullOrEmpty(fromAccountId))
                return Request.CreateApiErrorResponse("Error getting DRFI request", error: "drfi.sender.invalid");

            var response = new PersonalMessage
            {
                Id = ObjectId.GenerateNewId(),
                Text = "[DRFI Response]",
                Type = $"drfiresponsedenied",
                FromAccountId = AccountId,
                ToReceiverId = fromAccount.Id.ToString(),
                ConversationId = message.ConversationId,
                DrfiRequestId = messageId
            };
            await MessageService.AddPersonalMessageAsync(response, true);

            result = MessagingService.SetMessageStatus(messageId, "denied");
            if (!result.Success)
            {
                return Request.CreateApiErrorResponse("Error denying DRFI request", error: "drfi.deny.error");
            }

            return Request.CreateSuccessResponse(new
            {
                requestMessageId = messageId,
                responseMessageId = response.Id.ToString(),
                status = "denied"
            }, "DRFI request denied");
        }
    }
}