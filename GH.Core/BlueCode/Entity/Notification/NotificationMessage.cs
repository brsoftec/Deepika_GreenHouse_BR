using System.Dynamic;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.BlueCode.Entity.Notification
{

    [BsonDiscriminator("Notification")]
    [BsonIgnoreExtraElements]
    public class NotificationMessage : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Category { get; set; }
        public string Type { get; set; }
        public string DateTime { get; set; }
        public string FromAccountId { get; set; }
        public string FromUserDisplayName { get; set; }
        [BsonIgnoreIfNull]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EmbeddedProfile FromProfile { get; set; }
        public string ToAccountId { get; set; }
        public string ToUserDisplayName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Options { get; set; }
        public object PreserveBag { get; set; }
        public string PayloadStatus { get; set; }
        public object Payload{ get; set; }
        public bool Read { get; set; }
//        public bool BlockDetail { get; set; }
    }
    
        
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class NotificationUserProfile
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
    }
    
    public class UserNotification
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DateTime { get; set; }
        public string FromAccountId { get; set; }
        public string FromUserDisplayName { get; set; }
        public NotificationUserProfile FromProfile { get; set; }
        public string ToAccountId { get; set; }
        public string ToUserDisplayName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Options { get; set; }
        public object PreserveBag { get; set; }
        public object Payload{ get; set; }
        public object PayloadStatus { get; set; }
        public bool Read { get; set; }
    }
}