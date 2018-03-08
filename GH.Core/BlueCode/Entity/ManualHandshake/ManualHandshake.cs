using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.ManualHandshake
{
    [BsonDiscriminator("ManualHandshake")]
    [BsonIgnoreExtraElements]
    public class ManualHandshake : IMongoDBEntity
    {
       
        public ManualHandshake()
        {
            fields = new List<FieldManualHandshake>();
            expiry = new Expiry();

        }
        [BsonId]
        public ObjectId Id { get; set; }
        public string accountId { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Expiry expiry { get; set; }
        public DateTime synced { get; set; }
        public string toAccountId { get; set; }
        public string toName { get; set; }
        public string toEmail { get; set; }
        public string comment { get; set; }
        public string notifyFormat { get; set; }
        public List<FieldManualHandshake> fields { get; set; }
    }
    public class FieldManualHandshake
    {
        public string label { get; set; }
        public string jsPath { get; set; }
        public bool selected { get; set; }
    }

    public class Expiry
    {
        public bool indefinite { get; set; }
        public DateTime date { get; set; }
    }
    public sealed class EnumManualHandshake
    {
        public static readonly string Active = "active";
        public static readonly string Pause = "paused";
        public static readonly string Blocked = "blocked";
        public static readonly string CurrentAddress = "Current Address";
        public static readonly string MailingAddress = "Mailing Address";
        public static readonly string MobileNumber = "Mobile Number";
        public static readonly string OfficeNumber = "Office Number";
        public static readonly string PersonalMail = "Personal Email";
        public static readonly string WorkMail = "Work Email";
        public static readonly string SendData = "values";


    }


}