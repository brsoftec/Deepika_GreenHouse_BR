using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.ProfilePrivacy
{
    [BsonDiscriminator("ProfilePrivacy")]
    public class ProfilePrivacy : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string AccountId { get; set; }
        public List<Privacy> ListField { get; set; }
    }

    public class Privacy
    {
        public string Field { get; set; }
        public string Role { get; set; }
    }
    public sealed class EnumPrivacy
    {
        public static readonly string CountryCity = "CountryCity";
        public static readonly string DOB = "DOB";
        public static readonly string PhotoUrl = "PhotoUrl";
        public static readonly string Phone = "Phone";
        public static readonly string Email = "Email";
        public static readonly string Profile = "Profile";
        public static readonly string RequestHandshake = "RequestHandshake";
    }
    public sealed class EnumPrivacyRole
    {
        public static readonly string Private = "private";
        public static readonly string Public = "public";
        public static readonly string On = "on";
        public static readonly string Off ="off";
    }

}

