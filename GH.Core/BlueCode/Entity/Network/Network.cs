using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Common;

namespace GH.Core.BlueCode.Entity.Network
{
    public class Network : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public ObjectId NetworkOwner { get; set; }
        public string NetworkOwnerAccountId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CampaignId { get; set; }
        public IEnumerable<FriendNetwork> Friends { get; set; }

    }
}
