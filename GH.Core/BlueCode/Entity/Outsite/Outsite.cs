using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace GH.Core.BlueCode.Entity.Outsite
{

    [BsonDiscriminator("Outsite")]
    [BsonIgnoreExtraElements]
    public class Outsite : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public string Option { get; set; }
        public string[] ListEmail { get; set; }
        public bool SendMe { get; set; }
        public string FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string CompnentId { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }

        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
    }
}