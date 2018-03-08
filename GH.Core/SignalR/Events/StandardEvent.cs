using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.SignalR.Events
{
    public class StandardEvent : Event, IMessageEvent<string>
    {
        public string Message { get; set; }

        public StandardEvent(string message)
        {
            Message = message;
        }
    }
}