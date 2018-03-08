using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Caliburn.Micro;
using IEventAggregator = SignalR.EventAggregatorProxy.EventAggregation.IEventAggregator;

namespace GH.Core.SignalR.EventProxy
{
    public class EventAggregatorProxy : IEventAggregator, IHandle<GH.Core.SignalR.Events.Event>
    {
        private Action<object> handler;

        public EventAggregatorProxy(Caliburn.Micro.IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Subscribe(Action<object> handler)
        {
            this.handler = handler;
        }

        public void Handle(GH.Core.SignalR.Events.Event message)
        {
            if (handler != null) //Events can come in before the subsriber is hooked up
                handler(message);
        }
    }
}