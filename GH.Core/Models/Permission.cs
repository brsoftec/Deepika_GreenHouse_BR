using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;

namespace GH.Core.Models
{
    public class Permission
    {
        public bool IsFriend { get; set; }
        public IList<ObjectId> ListGroupBelong { get; set; }
    }
}