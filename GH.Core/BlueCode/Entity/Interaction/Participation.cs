
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.BlueCode.Entity.Interaction
{
    public enum FieldSource
    {
        VaultCurrentValue,
        NewValue
     }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ParticipationField
    {
        [BsonElement("source")]
        public FieldSource Source { get; set; }
        [BsonElement("path")]
        public string Path { get; set; }
        [BsonElement("value")]
        public object Value { get; set; }
    }   
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserParticipationField
    {
        public string Source { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
    }
    [BsonIgnoreExtraElements]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Participation : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("interactionId")]
        public string InteractionId { get; set; }
        [BsonElement("accountId")]
        public string AccountId { get; set; }
        [BsonElement("businessId")]
        public string BusinessId { get; set; }
        [BsonElement("fields")]
        public IList<ParticipationField> Fields { get; set; } = new List<ParticipationField>();
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
        
    }    
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserParticipation
    {
        public string InteractionId { get; set; }
        public string AccountId { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime ParticipatedAt { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Status { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DelegationId { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DelegateeId { get; set; }

        public List<UserFieldData> UserData { get; set; } = new List<UserFieldData>();
        
    }        
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionParticipant
    {
        public string AccountId { get; set; }
        public string InteractionId { get; set; }
        public DateTime ParticipatedAt { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Status { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }      
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EmbeddedProfile profile { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<FieldinformationVault> Fields { get; set; }
        public List<UserFieldData> UserData { get; set; } = new List<UserFieldData>();
    }    
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionParticipation
    {
        public string InteractionId { get; set; }
        public DateTime ParticipatedAt { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Status { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }      
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<FieldinformationVault> Fields { get; set; }
        public List<UserFieldData> UserData { get; set; } = new List<UserFieldData>();
    }      
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Customer
    {
        public string AccountId { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EmbeddedProfile profile { get; set; }
        public List<InteractionParticipation> Participations { get; set; }
    }    
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ParticipationResult
    {
        public string InteractionId { get; set; }
        public string AccountId { get; set; }
        public IList<FieldUpdate> UpdatedFields { get; set; }
        
    }
}
