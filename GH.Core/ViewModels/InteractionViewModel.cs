using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.ViewModels
{
    public class InteractionCriteria
    {
        public string gender { get; set; }
        public InteractionAgeCriteria age { get; set; }
        public InteractionLocationCriteria location { get; set; }
    }

    public class InteractionAgeCriteria
    {
        public string type { get; set; }
        public int min { get; set; }
        public int max { get; set; }
    }

    public class InteractionLocationCriteria
    {
        public string type { get; set; }
        public string country { get; set; }
        public string area { get; set; }
    }

    public class InteractionComment
    {
        public string type { get; set; }
        public string category { get; set; }
        public string creatorId { get; set; }
        public string text { get; set; }
    }


    //
    public class InteractionBusinessViewModel
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public bool Following { get; set; }
    }

    public class FeedBusinessViewModel
    {
        public string id { get; set; }
        public string accountId { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public bool following { get; set; }
    }

    public class InteractionParticipationViewModel
    {
        public string Actor { get; set; }
        public string ActorName { get; set; }
        public string DelegationId { get; set; }
        public string Participated { get; set; }
    }

    public class FeedParticipationViewModel
    {
        public string actor { get; set; }
        public string actorName { get; set; }
        public string delegationId { get; set; }
        public string participated { get; set; }
    }

    public class InteractionEventViewModel
    {
        public string fromDate { get; set; }
        public string fromTime { get; set; }

        public string toDate { get; set; }
        public string toTime { get; set; }

        public string location { get; set; }
        public string theme { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserFormField
    {
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public object Options { get; set; }
        public bool Optional { get; set; }
        public string Group { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Permission { get; set; }
    }     
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserFieldData
    {
        public string Source { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
    }   
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserDrfiField
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public object[] Options { get; set; }
        public UserFieldValue Value { get; set; }
    }      
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserField
    {
        public UserFormField Field { get; set; }
        public UserFieldData Data { get; set; }
    }      
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserDataField
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }     
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FieldUpdate
    {
        public string Path { get; set; }
        public bool Success { get; set; }
        public string Status { get; set; }
        [JsonIgnore]
        public DateTimeOffset Timestamp { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserFieldValue
    {
        public string Text { get; set; }
        public object[] List { get; set; }
        public string Json{ get; set; }
    }
    

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserFieldWithData
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public object[] Options { get; set; }
        public bool Optional { get; set; }
        public UserFieldValue Value { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FormFieldGroup
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class UserInteractionFormModel
    {
        public object fields { get; set; }
        public object groups { get; set; }
        public List<UserFieldData> userData { get; set; }
    }    
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserFormGroupWithData
    {
        public string Heading { get; set; }
        public List<UserFieldWithData> Fields { get; set; }
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ParticipationPostModel
    {
        public string InteractionId { get; set; }
        public string AccountId { get; set; }
        public string DelegationId { get; set; }
        public IList<UserFieldData> Fields { get; set; }
    }      
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UnparticipationPostModel
    {
        public string InteractionId { get; set; }
        public string DelegationId { get; set; }
    }   
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InteractionCore
    {
        public string Id { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string BusinessAccountId { get; set; }
        public string Name { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Type { get; set; }
        public string Description { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int Participants { get; set; }
    }   
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ParticipationRequest
    {
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class BusinessInteraction
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public DateTime Since { get; set; }
        public bool Indefinite { get; set; }
        public DateTime? Until { get; set; }
        
        //public UserInteractionFormModel form { get; set; }
        
        public List<UserFormField> Fields { get; set; }
        public IList<InteractionFieldGroup> Groups { get; set; }

    }
    public class UserInteractionModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public FeedBusinessViewModel business { get; set; }

        public string image { get; set; }
        public string targetUrl { get; set; }
        public string termsType { get; set; }
        public string termsUrl { get; set; }

        public bool paid { get; set; }
        public string price { get; set; }
        public string priceCurrency { get; set; }

        public string verb { get; set; }
        public string socialShare { get; set; }
        public DateTime? until { get; set; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public InteractionEventViewModel eventInfo { get; set; }

        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore)]
        public bool participated { get; set; }       
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore)]
        public UserParticipation participation { get; set; }       
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<FeedParticipationViewModel> participations { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public ParticipationRequest request { get; set; }

        public UserInteractionFormModel form { get; set; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<UserFormGroupWithData> formData { get; set; }
        

    }
    
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserHandshake
    {
        public string Id { get; set; }
        public string InteractionId { get; set; }
        public string AccountId { get; set; }
        public string BusinessId { get; set; }
        public string UserStatus { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Status { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string LastUpdated { get; set; }
        public UserInteractionModel Interaction { get; set; }
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserPersonalHandshake
    {
        public string Id { get; set; }
        public string Direction { get; set; }
        public string AccountId { get; set; }
        public string ToAccountId { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string ToName { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string ToEmail { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public EmbeddedProfile WithProfile { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Expires { get; set; }      
        public string UserStatus { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string LastUpdated { get; set; }
    }  
 
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PersonalHandshakePostModel
    {
        public string ToAccountId { get; set; }
        public string ToName { get; set; }
        public string ToEmail { get; set; }
        public string Description { get; set; }      
        public string Expires { get; set; }      
        public string NotifyFormat { get; set; }      
        public List<string> FieldPaths { get; set; }
    }
    
        
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserHandshakeRequest
    {
        public string Id { get; set; }
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string ToUcbId { get; set; }
        public EmbeddedProfile ToProfile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string CreatedAt { get; set; }      
        public string Status { get; set; }
    }   
        
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class HandshakeRequestPostModel
    {
        public string ToAccountId { get; set; }
        public string ToUcbId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }   


    public class FeedInteractionViewModel
    {
        public string id { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public FeedBusinessViewModel business { get; set; }

        public bool indefinite { get; set; }

        public string termsUrl { get; set; }

        public DateTime from { get; set; }
        public DateTime? until { get; set; }

        public string image { get; set; }
        public string targetUrl { get; set; }
        public string verb { get; set; }
        public string socialShare { get; set; }

        public bool paid { get; set; }
        public string price { get; set; }
        public string priceCurrency { get; set; }

        public InteractionEventViewModel eventInfo { get; set; }

        public bool participated { get; set; }    
        public int participants { get; set; }
        public bool expired { get; set; }

        public List<FeedParticipationViewModel> participations { get; set; }
    }

    public class InteractionViewModel
    {
        public InteractionViewModel()
        {
            Business = new InteractionBusinessViewModel();
            EventInfo = new InteractionEventViewModel();
            Participations = new List<InteractionParticipationViewModel>();
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }

        public InteractionBusinessViewModel Business { get; set; }

        public string Timetype { get; set; }

        public string TermsUrl { get; set; }
        public string SocialShare { get; set; }

        public InteractionEventViewModel EventInfo { get; set; }

        public DateTime From { get; set; }
        public DateTime? Until { get; set; }

        public string UserId { get; set; }
        public int MaxAge { get; set; }
        public int MinAge { get; set; }
        public string Gender { get; set; }
        public string LocationType { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public int SpendMoney { get; set; }

        public string ResidenceStatus { get; set; }
        public string EstimatedReach { get; set; }
        public string Image { get; set; }
        public string TargetLink { get; set; }
        public string Verb { get; set; }

        public bool paid { get; set; }
        public string price { get; set; }
        public string priceCurrency { get; set; }
        public string termsType { get; set; }
        public string termsUrl { get; set; }

        public object Fields { get; set; }
        public int Participants { get; set; }
        public bool Expired { get; set; }

        public List<InteractionParticipationViewModel> Participations { get; set; }
    }
}