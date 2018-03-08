using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ChangePhoneNumberModel
    {
        [Required(ErrorMessage = "Phone number is required")]
        [MinLength(10, ErrorMessage = "Invalid phone number")]
        public string NewPhoneNumber { get; set; }
        [Compare("NewPhoneNumber", ErrorMessage = "Confirm phone number do not match with new phone number")]
        public string ConfirmNewPhoneNumber { get; set; }
       
        public string NewPhonePIN { get; set; }
       
        public string NewPhoneRequestId { get; set; }
        public string VerifiedToken { get; set; }
    }
}