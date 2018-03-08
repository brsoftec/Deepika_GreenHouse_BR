using System;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GH.Core.BlueCode.Entity.InformationVault;
using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Message
{
    [BsonIgnoreExtraElements]
    //[BsonDiscriminator("PersonalMessage")]
    public class PersonalMessage : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        //public string Type { get; set; }
        //public DateTime DateTime { get; set; }
        //public string FromAccountId { get; set; }
        //public string FromUserDisplayName { get; set; }
        //public string ToReceiverId { get; set; }
        //public string ToDisplayName { get; set; }
        //public bool IsGroup { get; set; }
        //public string Content { get; set; }
        //public Conversation Conversation { get; set; }

        public DateTime Created { get; set; }
        public string Text { get; set; }
        public DateTime Read { get; set; }
        public bool FromMe { get; set; }
        [BsonIgnoreIfNull]
        public string FromAccountId { get; set; }
        public string ToReceiverId { get; set; }
        //public bool isGroup { get; set; }
        public string ConversationId { get; set; }

        [BsonIgnoreIfNull]
        public string Status { get; set; }
        
        public bool IsRead { get; set; }

        public bool IsDelete { get; set; }

        public DateTime DateDelete { get; set; }
       
        [BsonIgnoreIfNull]
        public string Type { get; set; }
        [BsonIgnoreIfNull]
        public string DrfiRequestId { get; set; }
        public string JsonFieldsdrfi { set; get; }

    }

}
