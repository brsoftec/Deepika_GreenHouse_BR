using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class CheckDuplicatedEmailModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }
        public bool IgnoreCurrent { get; set; }
        public bool IgnoreCurrentBusiness { get; set; }
    }
}