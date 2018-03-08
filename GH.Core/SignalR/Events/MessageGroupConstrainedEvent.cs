namespace GH.Core.SignalR.Events
{
    public class MessageGroupConstrainedEvent : Event
    {
        public MessageGroupConstrainedEvent(string groupId, string message)
        {
            GroupId = groupId;
            Message = message;
        }
        public string Message { get; set; }

        public string GroupId { get; set; }
    }
}