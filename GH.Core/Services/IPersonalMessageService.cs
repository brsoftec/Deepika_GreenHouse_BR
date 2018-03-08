using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GH.Core.BlueCode.Entity.Message;

namespace GH.Core.Services
{
    public interface IPersonalMessageService
    {
        void AddPersonalMessage(PersonalMessage personalMessage, bool sendFcm=false);
        Task<FuncResult> AddPersonalMessageAsync(PersonalMessage personalMessage, bool sendFcm=false);
        void NewConversation(Conversation conversaton);
        List<PersonalMessage> GetPrivateConversations(string userId);
        void AddGroupChat(Conversation conversation);
        List<Conversation> GetConversations(string userId);
        Conversation GetConversation(string conversationId);

        IEnumerable<Conversation> GeneratePrivateConversations(Account user, IEnumerable<Account> friends,
            IList<Conversation> existedConversations);

        bool DeletePerosnalMessage(string conversationId, string messageid, DateTime? datedelete);

        Task<FuncResult> MarkReadMessageAsync(string messageId, string userId);
        Task<FuncResult> MarkReadConversationAsync(string conversationId, string userId);

    }
}