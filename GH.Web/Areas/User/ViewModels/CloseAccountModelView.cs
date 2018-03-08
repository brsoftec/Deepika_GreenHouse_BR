using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class StatusAccountModelView
    {
        public string Status { set; get; }
        public string Email { set; get; }
        public DateTime EffectiveDate { get; set; }
      
        public DateTime? Until { get; set; }
        public string Reason { get; set; }
    }
}