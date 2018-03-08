using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.EventAggregatorProxy.Constraint;
using GH.Core.SignalR.Events;
using GH.Core.SignalR.Constraints;

namespace GH.Core.SignalR.EventConstraintHandlers
{
    public class ToUserConstrainedEventConstraintHandler : EventConstraintHandler<ToUserConstrainedEvent, ToUserConstrainedEventConstraint>
    {
        public override bool Allow(ToUserConstrainedEvent message, ConstraintContext context, ToUserConstrainedEventConstraint constraint)
        {
            return message.AccountId == constraint.AccountId;
        }
    }
}