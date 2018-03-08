using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class FieldPrivacyViewModel
    {
        public string AccountId { get; set; }
        public string Field { get; set; }
        public string Role { get; set; }
    }
}