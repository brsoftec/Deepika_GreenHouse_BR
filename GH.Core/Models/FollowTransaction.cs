using System;
using GH.Core.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class FollowTransaction 
    {
        public ObjectId FromUser { get; set; }
        public ObjectId ToUser { get; set; }
        public FollowType Type { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime Date { get; set; }
    }
}