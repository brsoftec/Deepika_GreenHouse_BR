using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.ManageTokenDevice
{
    [BsonDiscriminator("ManageTokenDevice")]
    [BsonIgnoreExtraElements]
    public class ManageTokenDevice : IMongoDBEntity
    {
            public ObjectId Id { get; set; }
            public string AccountId { get; set; }
            public string TokenDevice { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Status { get; set; }
        
    }
    public sealed class EnumStatusTokenDevice
    {
        public static readonly string Online = "Online";
        public static readonly string Offline = "Offline";
    }

    }