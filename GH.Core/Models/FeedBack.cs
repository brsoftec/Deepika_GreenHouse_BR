using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GH.Core.Models
{
    public class FeedBack
    {    
            [BsonId]
            public ObjectId Id { get; set; }
            public string UserId { get; set; }
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