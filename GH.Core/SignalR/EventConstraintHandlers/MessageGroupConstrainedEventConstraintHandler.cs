using GH.Core.SignalR.Constraints;
using GH.Core.SignalR.Events;
using SignalR.EventAggregatorProxy.Constraint;

namespace GH.Core.SignalR.EventConstraintHandlers
{
    public class MessageGroupConstrainedEventConstraintHandler : EventConstraintHandler<MessageGroupConstrainedEvent, MessageGroupConstrainedEventConstraint>
    {
        public override bool Allow(MessageGroupConstrainedEvent message, ConstraintContext context, MessageGroupConstrainedEventConstraint constraint)
        {
            return message.GroupId == constraint.GroupId;
        }
    }
}