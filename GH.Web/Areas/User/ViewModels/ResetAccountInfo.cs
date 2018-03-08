using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ResetPasswordAccountInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string EncodedPhoneNumber { get; set; }
    }
}