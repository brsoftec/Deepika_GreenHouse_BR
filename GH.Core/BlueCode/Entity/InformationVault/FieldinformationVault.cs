using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.BlueCode.Entity.InformationVault
{
    public class FieldinformationVault
    {
        public string jsPath { set; get; }
        public string label { set; get; }
        public bool optional { set; get; }
        public bool choices { set; get; }
        public bool qa { set; get; }
        public string type { set; get; }
        public string model { set; get; }
        public string unitModel { set; get; } 
        public object options { set; get; }
        public string id  { set; get; }
        public string displayName { set; get; }
        public string value { set; get; }
        public string membership { set; get; }

        [BsonIgnoreIfNull]
        public object modelarrays { set; get; }

        public string pathddocument { set; get; }

        public string pathfile { set; get; }
        public string modelarraysstr { set; get; }

    }
}