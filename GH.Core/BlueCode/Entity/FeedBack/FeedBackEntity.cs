using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.FeedBack
{
    [BsonDiscriminator("FeedBack")]
    public class FeedBackEntity : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string UserIP { get; set; }
        //UserLocal
        public string UserLocal { get; set; }
        public string Device { get; set; }
        public string Name { get; set; }

        public DateTime DateCreate { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public string Component { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string FeedBackURL { get; set; }
    }
}