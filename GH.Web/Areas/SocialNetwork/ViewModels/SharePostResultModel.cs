using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.ViewModels
{
    public class SharePostResultModel
    {
        public List<object> postStatus { get; set; }
        public SocialPostViewModel SharePost { get; set; }
        public int TotalShares { get; set; }
    }
}