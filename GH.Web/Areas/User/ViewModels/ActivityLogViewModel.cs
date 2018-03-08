using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ActivityLogViewModel
    {
       
        public string ActivityId { get; set; }
        public string DateTime { get; set; }
        public string ActivityType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromUserLink { get; set; }
        public string ToUserId { get; set; }
        public string ToUserEmail { get; set; }
        public string ToUserName { get; set; }
        public string ToUserProfileLink { get; set; }
        public string TargetOjectId { get; set; }
        public string TargetObjectName { get; set; }
        public string TargetObjectLink { get; set; }
    }
}