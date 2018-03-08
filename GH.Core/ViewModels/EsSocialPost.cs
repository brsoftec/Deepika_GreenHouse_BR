using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class EsSocialPost
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatorId { get; set; }
        public SocialType SocialNetwork { get; set; }
        public PostPrivacy PostPrivacy { get; set; }
        public string GroupId { get; set; }
    }
}