using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.BlueCode.Entity.Message
{
    [BsonDiscriminator("Conversation")]
    public class Conversation : IMongoDBEntity
    {
        public Conversation()
        {
            Users = new List<ConversationUser>();
            Messages = new List<PersonalMessage>();
        }
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        
        public bool IsGroupChat { get; set; }
        public string Owner { get; set; }
        public List<ConversationUser> Users { get; set; }

        public List<PersonalMessage> Messages { get; set; }
        public string TokenDevice { get; set; }
    }

    public class ConversationUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public bool Online { get; set; }
        public bool IsOwner { get; set; }
        public string AccountId { get; set; }
    }

}