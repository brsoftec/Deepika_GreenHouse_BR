using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson.Serialization.Attributes;
using GH.Core.BlueCode.Entity.InformationVault;

namespace GH.Core.BlueCode.Entity.Post
{
    [BsonIgnoreExtraElements]
    public class Post : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string PostType { get; set; }
        public string PostedUserId { get; set; }
        public string PostedUserName { get; set; }
        public string CampaignId { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<Follower> Followers { get; set; }

    }   

}
