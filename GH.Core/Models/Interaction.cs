using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionEventInfo
    {
        [BsonElement("startDate")]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime StartDate { get; set; }
        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endDate")]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime EndDate { get; set; }
        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }
        [BsonElement("theme")]
        public string Theme { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionCriteriaAge
    {
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("min")]
        public int Min { get; set; }
        [BsonElement("max")]
        public int Max{ get; set; }
    }   
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionCriteriaLocation
    {
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("country")]
        public string Country { get; set; }
        [BsonElement("area")]
        public string Area { get; set; }   
        [BsonIgnoreIfNull]
        [BsonElement("region")]
        public string Region { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("city")]
        public string City { get; set; }
    }    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    [BsonIgnoreExtraElements]
    public class InteractionCriteriaSpend
    {
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("effectiveDate")]
        public DateTime EffectiveDate { get; set; }
        [BsonElement("endDate")]
        public DateTime EndDate { get; set; }
    }   
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    [BsonIgnoreExtraElements]
    public class InteractionCriteria
    {
        [BsonElement("gender")]
        public string Gender { get; set; }
        [BsonElement("age")]
        public InteractionCriteriaAge Age { get; set; }
        [BsonElement("location")]
        public InteractionCriteriaLocation Location { get; set; }
        [BsonElement("spend")]
        public InteractionCriteriaSpend Spend { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    [BsonIgnoreExtraElements]
    public class InteractionField
    {
        [BsonElement("id")]
        public string FieldId { get; set; }
        [BsonElement("jsPath")]
        public string JsPath { get; set; }
        [BsonElement("path")]
        public string Path { get; set; }
        [BsonElement("label")]
        public string Label { get; set; }
        [BsonElement("displayName")]
        public string DisplayName { get; set; }
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("options")]
        public object Options { get; set; }
        [BsonElement("optional")]
        public bool Optional { get; set; }
        [BsonElement("choices")]
        [BsonIgnoreIfNull]
        public bool Choices { get; set; }
        [BsonElement("model")]
        [BsonIgnoreIfNull]
        public object Model { get; set; }
    }   
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionFieldGroup
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("displayName")]
        public string DisplayName { get; set; }
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionCampaign
    {
        [BsonId]
//        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonElement("campaign")]
        public Interaction Interaction { get; set; }
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Interaction
    {
        [BsonIgnore]
        public string Id { get; set; }
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("targetLink")]
        public string TargetLink { get; set; }
        [BsonElement("termsType")]
        public string TermsType { get; set; }
        [BsonElement("termsUrl")]
        public string TermsUrl { get; set; }
        
        [BsonElement("boost")]
        public string Boost { get; set; }
        [BsonElement("target")]
        public string Target { get; set; }
        [BsonElement("distribute")]
        public string Distribute { get; set; }
        [BsonElement("socialShare")]
        public string SocialShare { get; set; }
        [BsonElement("notes")]
        public string Notes { get; set; }
        
        [BsonElement("criteria")]
        public InteractionCriteria Criteria { get; set; }

        [BsonElement("eventInfo")]
        public InteractionEventInfo EventInfo { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("verb")]
        public string Verb { get; set; }

        [BsonElement("indefinite")]
        public bool Indefinite { get; set; }
        [BsonElement("paid")]
        public bool Paid { get; set; }
        [BsonElement("price")]
        public string Price { get; set; }
        [BsonElement("priceCurrency")]
        public string PriceCurrency { get; set; }

        [BsonElement("fields")]
        public IEnumerable<InteractionField> Fields { get; set; }
        [BsonElement("groups")]
        public IEnumerable<InteractionFieldGroup> Groups { get; set; }
        
        [BsonExtraElements]
        [BsonElement("extraElements")]
        public BsonDocument ExtraElements { get; set; }
    }
}