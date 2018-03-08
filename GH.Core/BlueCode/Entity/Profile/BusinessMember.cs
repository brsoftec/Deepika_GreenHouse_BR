using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.Post;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Profile
{
    public class BusinessMember : IMongoDBEntity
    {
        [BsonElement("_id")]
        public ObjectId Id
        {
            get; set;
        }
        public string BusinessUserId { get; set; }
        public string BusinessName { get; set; }

        public IEnumerable<Follower> Members { get; set; }

    }
}
