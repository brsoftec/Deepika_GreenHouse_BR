using System;
namespace GH.Core.SignalR.Events
{
    public class MessageSingleConstrainedEvent : Event
    {
        public MessageSingleConstrainedEvent(string accountId, string message)
        {
            AccountId = accountId;
            Message = message;
        }
        public string Message { get; set; }

        public string AccountId { get; set; }

        public string MessageType { set; get; }
        public string Messageid { get; set; }
        public string ConversationId { get; set; }
        public string jsonFieldsdrfi { set; get; }
        public DateTime DateDelete { set; get; }
       public string Type { set; get; }
    }
}