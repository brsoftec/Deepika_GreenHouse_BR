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
    public class Country: IMongoDBEntity
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

        public string Code { set; get; }


        public string Code3 { get; set; }


        public string Name { set; get; }



        public string NumCode { get; set; }


        public string PhoneCode { get; set; }
    }
}

