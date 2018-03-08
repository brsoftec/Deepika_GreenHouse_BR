using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Message;
using GH.Core.Services;
using GH.Core.SignalR.Events;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class PersonalMessageBusinessLogic : IMessageBusinessLogic
    {
        private MongoRepository<PersonalMessage> repository = new MongoRepository<PersonalMessage>();
        private AccountService accountService = new AccountService();

        private object locker = new object();
        public void SendMessage(PersonalMessage personalMessage)
        {
            //personalMessage.Id = ObjectId.GenerateNewId();
            ////personalMessage.DateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            //personalMessage.DateTime = DateTime.Now;
            //repository.Add(personalMessage);

            ////push notificaiton to user
            //var eventAggregator = (IEventAggregator)Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.GetService(typeof(IEventAggregator));
            ////Send notification to specific user
            //if(!personalMessage.IsGroup)
            //    eventAggregator.Publish(new MessageSingleConstrainedEvent(personalMessage.ToReceiverId, personalMessage.Content));
            //else eventAggregator.Publish(new MessageGroupConstrainedEvent(personalMessage.ToReceiverId, personalMessage.Content));

        }

        public IEnumerable<PersonalMessage> ViewPersonalMessage(string accountId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PersonalMessage> GetPersonalMessageList(string userId, int pageIndex = 1, int pageSize = 10)
        {
            return repository.Many(n => n.ToReceiverId.Equals(userId), pageIndex, pageSize).AsEnumerable();
        }

    }
}

