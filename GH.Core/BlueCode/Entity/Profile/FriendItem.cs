using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Common;

namespace GH.Core.BlueCode.Entity.Profile
{
    public class Friend : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }
        public string FriendName { get; set; }
        public string Image { get; set; }
        public string Group { get; set; }
    }
}
