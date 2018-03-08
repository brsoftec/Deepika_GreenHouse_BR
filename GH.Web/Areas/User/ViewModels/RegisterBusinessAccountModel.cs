using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class RegisterBusinessAccountModel
    {
        [Required]
        public SignUpCompanyDetails CompanyDetails { get; set; }
        [Required]
        public BusinessSignUpAccountInfo Account { get; set; }
        [Required]
        public SignUpAuthentication Authentication { get; set; }

        public SignUpBusinessAddtionalInfo AddtionalInfo { get; set; }
    }

    public class SignUpBusinessAddtionalInfo
    {
        public string Avatar { get; set; }
        public List<string> Workdays { get; set; }
        public DateTime? WorkHourFrom { get; set; }
        public DateTime? WorkHourTo { get; set; }
    }

    public class SignUpCompanyDetails
    {
        [Required, StringLength(256)]
        public string Industry { get; set; }
        [Required, StringLength(256)]
        public string CompanyName { get; set; }
        [Required, StringLength(256)]
        public string DisplayName { get; set; }
        [Required, StringLength(256)]
        public string Address { get; set; }
        public string Country { get; set; }
        [Required, StringLength(256)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Url)]
        public string Website { get; set; }
        public string Description { get; set; }
    }

    public class BusinessSignUpAccountInfo
    {
        [Required]
        public bool AgreeTermsAndCondition { get; set; }
        public bool LinkWithPersonal { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }
        [DisplayName("Phone number country code"), Required(ErrorMessage = "Country calling code is required")]
        public string PhoneNumberCountryCallingCode { get; set; }
        [Required]
        public string Password { get; set; }
        [DisplayName("Confirm password"), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class SignUpAuthentication
    {
        [Required(ErrorMessage = "Please enter your PIN")]
        public string PIN { get; set; }
        [Required(ErrorMessage = "Invalid request")]
        public string RequestId { get; set; }
    }

}