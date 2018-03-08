using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Payment
{
    [BsonIgnoreExtraElements]
    public class Billing
    {
        public string UserId { set; get; }
       public string Id { set; get; }
       public string productname { set; get; }
        public string promocode { set; get; }
        public string transactionid { set; get; }

       public DateTime datestart { set; get; }

       public DateTime dateend { set; get; }

       public string status  { set; get; }

        public string amount { set; get; }

        public string methodpayment { set; get; }
        
        public bool isCurrent { get; set; }
        
        [BsonIgnoreIfNull]
        public bool isPending { get; set; }

    }
}