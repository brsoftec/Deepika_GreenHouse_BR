using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.SignalR.Events
{
    public interface IMessageEvent<TMessage>
    {
        TMessage Message { get; set; }
    }
}