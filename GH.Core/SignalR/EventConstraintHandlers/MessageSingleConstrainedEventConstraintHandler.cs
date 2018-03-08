using GH.Core.SignalR.Constraints;
using GH.Core.SignalR.Events;
using SignalR.EventAggregatorProxy.Constraint;

namespace GH.Core.SignalR.EventConstraintHandlers
{
    public class MessageSingleConstrainedEventConstraintHandler : EventConstraintHandler<MessageSingleConstrainedEvent, MessageSingleConstrainedEventConstraint>
    {
        public override bool Allow(MessageSingleConstrainedEvent message, ConstraintContext context, MessageSingleConstrainedEventConstraint constraint)
        {
            return message.AccountId == constraint.AccountId;
        }
    }
}