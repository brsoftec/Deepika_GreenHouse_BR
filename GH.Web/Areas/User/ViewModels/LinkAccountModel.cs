using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class LinkAccountModel
    {
        [Required]
        public SocialType Network { get; set; }
        [Required]
        public string SocialAccountId { get; set; }
        [Required]
        public string AccessToken { get; set; }
        public string SecretAccessToken { get; set; }
        public string TwitterName { get; set; }
    }
}