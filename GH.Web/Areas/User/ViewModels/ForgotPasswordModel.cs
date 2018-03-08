using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required]
        public ResetPasswordVerifyOption Option { get; set; }
        [Required]
        public VerifySecurityQuestionsModel VerifyInfo { get; set; }
    }

    public enum ResetPasswordVerifyOption
    {
        EMAIL, SMS
    }
}