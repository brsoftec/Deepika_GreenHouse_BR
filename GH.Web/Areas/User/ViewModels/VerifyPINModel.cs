using System.ComponentModel.DataAnnotations;
using GH.Lang;

namespace GH.Web.Areas.User.ViewModels
{
    public class VerifyPINModel
    {
        [Required]
        public string RequestId { get; set; }
        [Required(ErrorMessageResourceType = typeof(Regit), ErrorMessageResourceName = "PIN_Error_Required")]
        public string PIN { get; set; }
        public string StaticPIN { get; set; }
        public string PhoneNumber { get; set; }
    }
}