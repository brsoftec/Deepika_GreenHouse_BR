using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class FacebookPageResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string access_token { get; set; }
    }
}