using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class VerifyResetPasswordTokenModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string RequestId { get; set; }
        [Required]
        public string Token { get; set; }
    }
}