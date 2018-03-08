using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;

namespace GH.Web.Areas.User.ViewModels
{
    public class MembersInBusinessViewModel
    {
        public long Total { get; set; }
        public List<MemberInBusiness> Members { get; set; }
    }

    public class MemberInBusiness
    {
        public ObjectId AccountId { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public List<ObjectId?> RoleIds { get; set; }

    }
 
}