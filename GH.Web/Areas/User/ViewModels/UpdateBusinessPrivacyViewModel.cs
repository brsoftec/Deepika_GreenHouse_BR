using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;

namespace GH.Web.Areas.User.ViewModels
{
    public class UpdateBusinessPrivacyViewModel
    {
        public AccountPrivacy Privacy { get; set; }
        public bool AllowComment { get; set; }
        public string BAId { get; set; }
    }
}