using System;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.BlueCode.Entity.Profile
{
    public class Region: IMongoDBEntity
    {
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

         
        public string Code { set; get; }


        public string Name { set; get; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Cities { get; set; }

    }
}
