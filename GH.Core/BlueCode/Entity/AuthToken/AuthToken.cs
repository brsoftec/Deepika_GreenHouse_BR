using System;
using System.Web;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace GH.Core.BlueCode.Entity.AuthToken
{
    [BsonDiscriminator("AuthToken")]
    [BsonIgnoreExtraElements]
    public class AuthToken: IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Status { get; set; }
        public string AccountId { get; set; }
        public string AccessToken { get; set; }
        public DateTime Issued { get; set; }
        public DateTime Expires { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FcmToken { get; set; }       
        [JsonProperty(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public AuthClientInfo ClientInfo { get; set; }
    }

    public sealed class EnumStatusAuthToken
    {
        public static readonly string Active = "active";
        public static readonly string Expired = "expired";
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AuthClientInfo
    {
        public string ip { get; set; }
        public string host { get; set; }
        [JsonProperty("ua")] 
        public string ua { get; set; }
    }
}