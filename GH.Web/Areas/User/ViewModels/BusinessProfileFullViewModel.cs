using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class BusinessProfileFullViewModel
    {
      
        public string Id { get; set; }
        public string BusId { get; set; }
        public string DisplayName { get; set; }
        public List<string> PictureAlbum { get; set; }
        public string Avatar { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string StreetCompany { get; set; }
        public string CityCompany { get; set; }

        public string CountryCompany { get; set; }
  
        public string PhoneCompany { get; set; }
        public string EmailCompany { get; set; }
        public string Website { get; set; }
        public int NumberOfFollowers { get; set; }
        public bool Followed { get; set; }
        public bool CanFollow { get; set; }

        public AccountType AccountType { set; get; }
     
        public BusinessPrivacy BusinessPrivacies { get; set; }
        public DateTime? WorkHourFrom { get; set; }
        public DateTime? WorkHourTo { get; set; }
        public List<string> Workdays { get; set; }
    }
}