using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.ViewModels
{
    public class PostNewFeedResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SocialPostViewModel Post { get; set; }
        public SocialType Social { get; set; }
    }
}