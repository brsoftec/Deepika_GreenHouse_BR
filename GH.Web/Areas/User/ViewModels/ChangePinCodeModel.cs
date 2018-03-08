using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ChangePinCodeModel
    {
        [Required(ErrorMessage = "Pin code is required")]
        public string NewPinCode { get; set; }
        [Compare("NewPinCode", ErrorMessage = "Confirm pin code do not match with new pin code")]
        public string ConfirmNewPinCode { get; set; }
       
        public string VerifiedToken { get; set; }
    }
}