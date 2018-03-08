using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class BaseObject
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime CreatedOn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime ModifiedOn { get; set; }
        public ObjectId CreatedBy { get; set; }
        public ObjectId ModifiedBy { get; set; }
    }
}