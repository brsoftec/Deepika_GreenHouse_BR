using System.Collections.Generic;
using GH.Core.Models;

namespace GH.Core.ViewModels
{
    public class AccountViewModel
    {
       public AccountViewModel()
        {
            ActivityLogSettings = new AccountActivityLogSettings();
            AnswerSercurityQuestions = new AnswerSecurityQuestionModel();
            StatusAccount = new StatusAccount();
            SercurityQuestions = new List<SecurityQuestion>();
        }
        public bool IsAdmin { set; get; }

        public string Id { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Region { get; set; }
        public string ZipPostalCode { get; set; }
        public string Description { get; set; }
        public string AccountType { get; set; }
        public string Birthdate { get; set; }

        public string Phone { get; set; }

        public string WebsiteURL { get; set; }

        public string StatusFriend { get; set; }

        public bool IsShowProfile { get; set; }
        public AccountNotificationSettings NotificationSettings { get; set; }
        public BusinessPrivacy BusinessPrivacies { get; set; }
        public bool BusinessAccountVerified { get; set; }
        public AccountActivityLogSettings ActivityLogSettings { get; set; }
        public StatusAccount StatusAccount { get; set; }
       
        public List<SecurityQuestion> SercurityQuestions { get; set; }
        public AnswerSecurityQuestionModel AnswerSercurityQuestions { get; set; }
    }
    public class StatusAccount
    {
        public string Status { set; get; }
        public string Email { set; get; }
        public string EffectiveDate { get; set; }

        public string Until { get; set; }
        public string Reason { get; set; }
    }

    public class AccountSettingsViewModel
    {
    }

}