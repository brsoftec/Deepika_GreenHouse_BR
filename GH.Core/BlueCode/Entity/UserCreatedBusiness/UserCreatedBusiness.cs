using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace GH.Core.BlueCode.Entity.UserCreatedBusiness
{
    public enum UcbType
    {
        Pending, Approved, Rejected
    }

    [BsonDiscriminator("UserCreatedBusiness")]
    [BsonIgnoreExtraElements]
    public class UserCreatedBusiness : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime Created { get; set; }
        public string CreatorRole { get; set; }
        public string CreatorId { get; set; }
        public string Status { get; set; }
        
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Avatar { get; set; }
        public string Description { get; set; }

    }
}