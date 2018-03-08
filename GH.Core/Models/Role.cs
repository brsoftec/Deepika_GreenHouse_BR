using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class Role
    {
        public const string ROLE_ADMIN = "Administrator";
        public const string ROLE_REVIEWER = "Reviewer";
        public const string ROLE_EDITOR = "Editor";

        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}