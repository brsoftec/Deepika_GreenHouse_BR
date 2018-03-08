using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.ViewModels
{
    public class SharePostViewModel
    {
        public string SocialPostId { get; set; }
        public string PostMongoId { get; set; }

        public bool IsShareFacebook { get; set; }

        public bool IsShareTwitter { get; set; }

        public bool IsShareGreenHouse { get; set; }
        public string Message { get; set; }
    }
}