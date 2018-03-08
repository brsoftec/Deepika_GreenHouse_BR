using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ShortProfileViewModel
    {
        public string userId { get; set; }
        public string avatarUrl { get; set; }
        public string displayName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string accountType { get; set; }
       
    }
}