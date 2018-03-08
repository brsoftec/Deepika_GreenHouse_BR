using System;
using GH.Core.BlueCode.Entity.InformationVault;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.BlueCode.Entity.Post
{
    [BsonIgnoreExtraElements]
    public class Follower
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string CityId { get; set; }
        public DateTime? ParticipatedAt { get; set; }
        public string FollowedDate { get; set; }
        public string UnFollowedDate { get; set; }
        public string Status { get; set; }
        public string DelegationId { set; get; }
        public string DelegateeId { set; get; }
        public string Comment { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<FieldinformationVault> fields { get; set; }
        public DateTime[] PushDates { get; set; }
    }
  
    
    [BsonIgnoreExtraElements]
    public class PostHandshake
    {
        [BsonId]
        public string Id { get; set; }
        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonElement("busid")]
        public string BusId { get; set; }       
        [BsonElement("campaignid")]
        public string CampaignId { get; set; }      
        public string Status { get; set; }
        [BsonElement("timestamp")]
        public DateTime? Timestamp { get; set; }
        public DateTime[] PushDates { get; set; }
        [BsonElement("participatedAt")]
        public DateTime? ParticipatedAt { get; set; }
        [BsonElement("isaccecpt")]
        public string IsAccept { get; set; }       
        [BsonElement("isjoin")]
        public string IsJoin { get; set; }       
        [BsonElement("ischange")]
        public string IsChange { get; set; }       
        [BsonElement("comment")]
        public string Comment { get; set; }
        [BsonElement("dateupdatejson")]
        public string DateJson { get; set; }
        [BsonElement("lastSync")]
        public DateTime LastSync { get; set; }
        [BsonElement("dateTerminate")]
        public DateTime DateTerminate { get; set; }
        [BsonElement("jsondata")]        
        public string FieldsJson { get; set; }
        [BsonElement("jsondataold")]
        public string FieldsJsonOld { get; set; }
/*        [BsonIgnoreIfNull]
        [BsonElement("fields")]        
        public List<FieldinformationVault> Fields { get; set; }*/
        

    }
}
