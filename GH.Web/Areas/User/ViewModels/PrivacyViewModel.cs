using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class PrivacyViewModel
    {
        public bool FieldEnable { get; set; }
        public string FieldRole { get; set; }
        public string FieldName { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public string AccountType { get; set; }

    }
}