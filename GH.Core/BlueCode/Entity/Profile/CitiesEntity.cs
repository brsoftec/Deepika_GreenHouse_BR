using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GH.Core.BlueCode.Entity.Common;

namespace GH.Core.BlueCode.Entity.Profile
{
    public class City: IMongoDBEntity
    {
        [BsonIgnore]
        public ObjectId Id
        {
            get; set;
        }

        [BsonElement("_id")]
        public ObjectId ID
        {
            get; set;
        }

        public string CountryCode { set; get; }

        public string RegionCode { set; get; }

        public string Name { set; get; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}

