
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class VaultField
    {
        [BsonId]
        public string Id { get; set; }
        [BsonElement("title")]
        public string Title { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("type")]
        public string Type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [BsonIgnoreIfNull]
        [BsonElement("options")]
        public object[] Options{ get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [BsonIgnoreIfNull]
        [BsonElement("rules")]
        public string Rules { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [BsonIgnoreIfDefault]
        [BsonElement("branch")]
        public bool Branch { get; set; }
    }
       
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserVaultField
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object[] Options{ get; set; }
    }
    
}