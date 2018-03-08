using System;

namespace GH.Core.SignalR.Events
{
    public class ToUserConstrainedEvent : Event
    {
        public ToUserConstrainedEvent()
        {


        }
        public ToUserConstrainedEvent(string accountId, string message)
        {
            AccountId = accountId;
            Message = message;
        }
        public string Message { get; set; }

        public string AccountId { get; set; }
        public string FromAccountId { get; set; }
        

        public string RuntimeNotifyType { get; set; }

    }
}