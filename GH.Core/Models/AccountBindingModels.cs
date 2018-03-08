using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using GH.Core.ViewModels;
using Newtonsoft.Json.Serialization;

namespace GH.Core.Models
{
    // Models used as parameters to AccountController actions.

    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }

    public class ChangePasswordBindingModel
    {
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessage = "Invalid request")]
        public string VerifiedToken { get; set; }
    }

    public class RegisterBindingModel
    {
        [Required]
        public RegistrationInfo Account { get; set; }
        [Required]
        public RegisterAuthenticationInfo Authentication { get; set; }
        //public RegisterSecurirtyQuestions SecurityQuestions { get; set; }
        public IList<AccessSecurityAnswer> SecurityQuestionsAnswers;
        public string OutsiteId { get; set; }
        public string InviteId { get; set; }
    }

    public class RegisterExternalBindingModel
    {
        [Required]
        public ExternalRegistrationInfo Account { get; set; }
        [Required]
        public RegisterAuthenticationInfo Authentication { get; set; }
        public RegisterSecurirtyQuestions SecurityQuestions { get; set; }
    }

    public class RegisterAuthenticationInfo
    {
       // [Required(ErrorMessage = "Invalid request")]
        public string RequestId { get; set; }
       // [Required(ErrorMessage = "Invalid request")]
        public string PIN { get; set; }
    }

    public class RegisterSecurirtyQuestions
    {

        public AnswerSecurityQuestionViewModel Question1 { get; set; }

        public AnswerSecurityQuestionViewModel Question2 { get; set; }

        public AnswerSecurityQuestionViewModel Question3 { get; set; }
    }
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AccessSecurityAnswer
    {
        public string Code { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class RegistrationInfo
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(32, ErrorMessage = "The password must be at least 8 characters, maximum 32 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(64, ErrorMessage = "The first name's maximum length is 64 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(64, ErrorMessage = "The last name's maximum length is 64 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Country calling code is required")]
        public string PhoneNumberCountryCallingCode { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string AboutMe { get; set; }
        public string Avatar { get; set; }
        public string FileName { get; set; }
        public DateTime? Birthday { get; set; }
      
    }

    public class ExternalRegistrationInfo
    {
        public string Email { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(64, ErrorMessage = "The first name's maximum length is 64 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(64, ErrorMessage = "The last name's maximum length is 64 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Country calling code is required")]
        public string PhoneNumberCountryCallingCode { get; set; }

        [StringLength(128, ErrorMessage = "The display name's maximum length is 128 characters")]
        public string DisplayName { get; set; }

        public string Avatar { get; set; }

        public bool Skip { get; set; }

        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string AboutMe { get; set; }
        public string FileName { get; set; }
        public DateTime? Birthday { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

  
}
