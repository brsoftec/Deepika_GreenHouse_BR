using System.ComponentModel.DataAnnotations;
using GH.Lang;

namespace GH.Web.Areas.User.ViewModels
{
    public class ChangeEmailModel
    {
        [Required(ErrorMessageResourceType = typeof(Regit), ErrorMessageResourceName = "Email_Error_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Regit),ErrorMessageResourceName = "Email_Error_Invalid")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string VerifiedToken { get; set; }
    }
}