using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.InformationVault;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Core.BlueCode.Entity.Network
{
    public class FriendNetwork: IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
