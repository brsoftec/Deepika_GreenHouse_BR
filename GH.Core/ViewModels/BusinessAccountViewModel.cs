using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class BusinessAccountViewModel
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
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
       
        public string Birthdate { get; set; }

        public string Phone { get; set; }

        public string WebsiteURL { get; set; }

        public string StatusFriend { get; set; }

        public bool IsShowProfile { get; set; }
        public AccountNotificationSettings NotificationSettings { get; set; }
        public BusinessPrivacy BusinessPrivacies { get; set; }
        public bool BusinessAccountVerified { get; set; }

        public string StreetCompany { get; set; }
        public string CityCompany { get; set; }

        public string CountryCompany { get; set; }

        public string PhoneCompany { get; set; }
        public string EmailCompany { get; set; }
    }
}