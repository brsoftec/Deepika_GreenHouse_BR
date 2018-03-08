using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using System;
using Newtonsoft.Json;

namespace GH.Web.Areas.User.ViewModels
{
    public class UserCreatedBusinessModel
    {
        public string id { get; set; }

        public string name { get; set; }
        public string industry { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string avatar { get; set; }
        public string description { get; set; }
        public string status { get; set; } = "pending";
        [JsonIgnore]
        public bool isVerified { get; set; }
        [JsonIgnore]
        public string CreatorRole { get; set; } = "user";
        [JsonIgnore]
        public string CreatorId { get; set; }
        [JsonIgnore]
        public DateTime Created { get; set; }

        public UserCreatedBusinessModel()
        {
        }

        public UserCreatedBusinessModel(UserCreatedBusiness ucb)
        {
            id = ucb.Id.ToString();
            name = ucb.Name;
            industry = ucb.Industry;
            country = ucb.Country;
            city = ucb.City;
            address = ucb.Address;
            phone = ucb.PhoneNumber;
            email = ucb.Email;
            website = ucb.Website;
            avatar = ucb.Avatar;
            description = ucb.Description;
            status = ucb.Status;
            isVerified = ucb.Status.Equals("approved");
            CreatorRole = ucb.CreatorRole;
            CreatorId = ucb.CreatorId;
            Created = ucb.Created;
        }
    }
    
            
    public class UcbClaimViewModel
    {
        public string id { get; set; }
        public string ucbId { get; set; }
        public string ucbName { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string message{ get; set; }
    }

}