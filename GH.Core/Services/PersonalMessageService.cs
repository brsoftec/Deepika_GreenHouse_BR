using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Message;
using GH.Core.Exceptions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Extensions;
using GH.Core.Helpers;
using GH.Core.SignalR.Events;
using GH.Core.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GH.Core.Services
{
    public class PersonalMessageService : IPersonalMessageService
    {
        IMongoCollection<Account> _accountCollection;

        //private IMongoCollection<PersonalMessage> _messageCollection;
        private IMongoCollection<Conversation> _conversationCollection;

        public PersonalMessageService()
        {
            var db = MongoContext.Db;
            //_messageCollection = db.PersonalMessages;
            _conversationCollection = db.GroupChats;
        }

        public bool DeletePerosnalMessage(string conversationId, string messageid, DateTime? datedelete)
        {
            Conversation conversation;
            try
            {
                conversation =
                    _conversationCollection.Find(x => x.Id == ObjectId.Parse(conversationId)).FirstOrDefault();
            }
            catch
            {
                return false;
            }

            if (conversation == null) return false;

            var messages = conversation.Messages;

            var message = messages.FirstOrDefault(m => m.Id.ToString().Equals(messageid));
            if (message == null) return false;
            message.DateDelete = datedelete ?? DateTime.UtcNow;
            message.IsDelete = true;
            message.Text = "Message deleted";

            var update = Builders<Conversation>.Update.Set(a => a.Messages, messages);
            _conversationCollection.UpdateOne(x => x.Id == ObjectId.Parse(conversationId), update);
            return true;
        }
        public async Task<FuncResult> MarkReadMessageAsync(string messageId, string userId)
        {
            ObjectId objId;
            try
            {
                objId = ObjectId.Parse(messageId);
            }
            catch
            {
                return new ErrResult("message.invalid");
            }

            var builderPost = Builders<Conversation>.Filter;
            var builderFollower = Builders<PersonalMessage>.Filter;
            var filterFollower = builderFollower.Eq("Id", objId);
            var filterPost = builderPost.ElemMatch("Messages", filterFollower);
            var conversation = await _conversationCollection.Find(filterPost).FirstOrDefaultAsync();
            if (conversation == null) return new ErrResult("message.notFound");

            var message = conversation.Messages.FirstOrDefault(m => m.Id == objId);
            if (message?.ToReceiverId != userId)
                return new ErrResult("message.notReceiver");

            var update = Builders<Conversation>.Update.Set("Messages.$.IsRead", true);
            var result = await _conversationCollection.UpdateOneAsync(filterPost, update);
            if (!result.IsAcknowledged)
                return new ErrResult("message.update.error");
            if (result.ModifiedCount == 0)
                return new ErrResult("message.update.not");

            return new OkResult("message.markread.ok", conversation.Id);
        }
        public async Task<FuncResult> MarkReadConversationAsync(string conversationId, string userId)
        {
            ObjectId objId;
            try
            {
                objId = ObjectId.Parse(conversationId);
            }
            catch
            {
                return new ErrResult("conversation.invalid");
            }

            var builder = Builders<Conversation>.Filter;
            var filter = builder.Eq("Id", objId);

            var conversation = await _conversationCollection.Find(filter).FirstOrDefaultAsync();
            if (conversation == null) return new ErrResult("conversation.notFound");

            if (conversation.Users.All(u => u.Id != userId))
                return new ErrResult("conversation.notMember");
            
            var builderMessage = Builders<PersonalMessage>.Filter;
            var filterMessage = builder.ElemMatch("Messages", 
                builderMessage.Eq("ToReceiverId", userId) & builderMessage.Eq("IsRead", false));

            UpdateResult result;
            long count = 0;
            do
            {
                var update = Builders<Conversation>.Update.Set("Messages.$.IsRead", true);
                result = await _conversationCollection.UpdateOneAsync(filterMessage, update);
                if (!result.IsAcknowledged)
                    return new ErrResult("conversation.update.error");
                count += result.ModifiedCount;
            } while (result.ModifiedCount > 0);

            return new OkResult("conversation.markread.ok", count);
        }

        public void AddPersonalMessage(PersonalMessage personalMessage, bool sendFcm = false)
        {
            if (personalMessage == null)
            {
                throw new CustomException("Personal message can't be NULL");
            }

            personalMessage.Created = DateTime.UtcNow;
            personalMessage.IsRead = false;

            var conId = new ObjectId(personalMessage.ConversationId);
            var conversation =
                _conversationCollection.Find(x => x.Id == conId).FirstOrDefault();
            if (conversation != null)
            {
                var messages = conversation.Messages;
                messages.Add(personalMessage);
                var update = Builders<Conversation>.Update.Set(a => a.Messages, messages);
                _conversationCollection.UpdateOne(x => x.Id == conId, update);

                var accountService = new AccountService();
                var toAccount = accountService.GetById(personalMessage.ToReceiverId);

                var msgData = new MessageDataViewModel();
                msgData.AccountId = personalMessage.FromAccountId;
                msgData.ConversationId = personalMessage.ConversationId;
                msgData.Messageid = personalMessage.Id.ToString();
                msgData.MessageType = personalMessage.Type;
                msgData.Message = personalMessage.Text;
                msgData.Type = "addmessage";
                msgData.DateDelete = personalMessage.DateDelete;
                msgData.Create = personalMessage.Created;
                msgData.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

                var privateMessageConstrainedEvent =
                    new MessageSingleConstrainedEvent(toAccount.AccountId, personalMessage.Text);
                privateMessageConstrainedEvent.MessageType = personalMessage.Type;

                privateMessageConstrainedEvent.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

                privateMessageConstrainedEvent.Messageid = personalMessage.Id.ToString();
                privateMessageConstrainedEvent.Type = "addmessage";
                privateMessageConstrainedEvent.ConversationId = personalMessage.ConversationId;
                var eventAggregator =
                    (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                        typeof(Caliburn.Micro.IEventAggregator));

                eventAggregator.Publish(privateMessageConstrainedEvent);

                var account = accountService.GetByAccountId(personalMessage.FromAccountId);

                if (sendFcm)
                {
                    Task.Factory.StartNew(() => new AuthTokensLogic().PostToFcmAsync(toAccount, account, "message",
                        $"Message from {account.Profile.DisplayName}", personalMessage.Text, personalMessage)).Wait();
                }
            }
        }

        public async Task<FuncResult> AddPersonalMessageAsync(PersonalMessage personalMessage, bool sendFcm = false)
        {
            if (personalMessage == null)
            {
                return new ErrResult("message.null");
            }

            personalMessage.Created = DateTime.UtcNow;
            personalMessage.IsRead = false;

            var conId = new ObjectId(personalMessage.ConversationId);
            var conversation =
                _conversationCollection.Find(x => x.Id == conId).FirstOrDefault();
            if (conversation != null)
            {
                var messages = conversation.Messages;
                messages.Add(personalMessage);
                var update = Builders<Conversation>.Update.Set(a => a.Messages, messages);
                _conversationCollection.UpdateOne(x => x.Id == conId, update);

                var accountService = new AccountService();
                var toAccount = accountService.GetById(personalMessage.ToReceiverId);

                var msgData = new MessageDataViewModel();
                msgData.AccountId = personalMessage.FromAccountId;
                msgData.ConversationId = personalMessage.ConversationId;
                msgData.Messageid = personalMessage.Id.ToString();
                msgData.MessageType = personalMessage.Type;
                msgData.Message = personalMessage.Text;
                msgData.Type = "addmessage";
                msgData.DateDelete = personalMessage.DateDelete;
                msgData.Create = personalMessage.Created;
                msgData.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

                var privateMessageConstrainedEvent =
                    new MessageSingleConstrainedEvent(toAccount.AccountId, personalMessage.Text);
                privateMessageConstrainedEvent.MessageType = personalMessage.Type;

                privateMessageConstrainedEvent.jsonFieldsdrfi = personalMessage.JsonFieldsdrfi;

                privateMessageConstrainedEvent.Messageid = personalMessage.Id.ToString();
                privateMessageConstrainedEvent.Type = "addmessage";
                privateMessageConstrainedEvent.ConversationId = personalMessage.ConversationId;
                var eventAggregator =
                    (Caliburn.Micro.IEventAggregator) Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(
                        typeof(Caliburn.Micro.IEventAggregator));

                eventAggregator.Publish(privateMessageConstrainedEvent);

                var account = accountService.GetByAccountId(personalMessage.FromAccountId);

                if (sendFcm)
                {

                    await new AuthTokensLogic().PostToFcmAsync(toAccount, account, "message",
                        $"Message from {account.Profile.DisplayName}", personalMessage.Text, personalMessage);
                }
            }

            return new OkResult("message.ok");
        }

        public void updateUnreadConversation(string conversationId, string conversationmessageId, string userid)
        {
            var conversation =
                _conversationCollection.Find(x => x.Id == ObjectId.Parse(conversationId)).FirstOrDefault();
            if (conversation != null)
            {
                var messages = conversation.Messages;
                for (int i = 0; i < messages.Count; i++)
                {
                    if (messages[i].ToReceiverId == userid)
                    {
                        if (!string.IsNullOrEmpty(conversationmessageId))
                        {
                            if (messages[i].Id.ToString() == conversationmessageId)
                                messages[i].IsRead = true;
                        }
                        else
                        {
                            messages[i].IsRead = true;
                        }
                    }
                }

                var update = Builders<Conversation>.Update.Set(a => a.Messages, messages);
                _conversationCollection.UpdateOne(x => x.Id == ObjectId.Parse(conversationId), update);
            }
        }

        public Conversation GetConversation(string conversationId)
        {
            return _conversationCollection.Find(x => x.Id.ToString() == conversationId).Single();
        }


        public void NewConversation(Conversation conversaton)
        {
            if (conversaton == null) throw new CustomException("Personal message can't be NULL");
            _conversationCollection.InsertOne(conversaton);
        }

        public void AddGroupChat(Conversation conversation)
        {
            if (conversation == null)
                throw new CustomException("Group chat can't be NULL");
            _conversationCollection.InsertOne(conversation);
        }

        public List<PersonalMessage> GetPrivateConversations(string userId)
        {
            //var messages = _messageCollection.Find(x=>!x.IsGroup &&(x.FromAccountId==userId || x.ToReceiverId==userId) ).ToList();
            //return messages;
            throw new NotImplementedException();
        }

        public List<Conversation> GetConversations(string userId)
        {
            var listusers = _conversationCollection.Find(x => x.Users.Any(u => u.Id == userId)).ToList();
            return listusers;
        }

        public IEnumerable<Conversation> GeneratePrivateConversations(Account user, IEnumerable<Account> friends,
            IList<Conversation> existedConversations)
        {
            //var result = new HashSet<Conversation>();
            var conversations = new List<Conversation>();
            foreach (var friend in friends)
            {
                var friendId = friend.Id.ToString();
                if (!existedConversations.Any(x => !x.IsGroupChat && x.Users.All(u => u.Id != friendId)))
                {
                    var id = ObjectId.GenerateNewId();
                    var conversation = new Conversation()
                    {
                        Id = id,
                        IsGroupChat = false,
                        Users = new List<ConversationUser>()
                        {
                            new ConversationUser()
                            {
                                Avatar = user.Profile.PhotoUrl,
                                Id = user.Id.ToString(),
                                Name = user.Profile.DisplayName,
                                IsOwner = true
                            },
                            new ConversationUser()
                            {
                                Avatar = friend.Profile.PhotoUrl,
                                Id = friend.Id.ToString(),
                                Name = friend.Profile.DisplayName,
                            }
                        },
                        Messages = new List<PersonalMessage>()
                        {
                            new PersonalMessage()
                            {
                                ConversationId = id.ToString(),
                                Created = DateTime.Now,
                                FromMe = false,
                                Id = ObjectId.GenerateNewId(),
                                Read = DateTime.Now,
                                Text = @"Let's chat!",
                                ToReceiverId = user.Id.ToString()
                            }
                        }
                    };
                    conversations.Add(conversation);
                }
            }

            if (conversations.Count > 0) _conversationCollection.InsertMany(conversations);
            return conversations;
        }
    }
}