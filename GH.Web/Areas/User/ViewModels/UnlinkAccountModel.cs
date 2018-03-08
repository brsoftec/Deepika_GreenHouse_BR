using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class UnlinkAccountModel
    {
        [Required]
        public SocialType Network { get; set; }
    }
}