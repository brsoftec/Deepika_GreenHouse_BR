
using GH.Core.Models;

namespace GH.Web.Areas.User.ViewModels
{
    public class HomeViewModel
    {
        public string Status { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        public string CountryDetected { get; set; }
        public string CountryCodeDetected { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public bool NoSecurityQuestions { get; set; }
        public AccountViewPreferences ViewPreferences { get; set; }
        public bool AccountIncomplete { get; set; }
    }
}