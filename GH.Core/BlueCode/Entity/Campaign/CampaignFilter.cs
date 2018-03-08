
using System.Collections;
using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Campaign
{
    public class CampaignFilter
    {
        public CampaignFilter()
        {

        }
        public CampaignFilter(int minAge, int maxAge, string gender, string country, string region, string city, bool flash, string targetNetwork)
        {
            this.MinAge = minAge;
            this.MaxAge = MaxAge;
            this.Gender = gender;
            this.Country = country;
            this.Region = region;
            this.City = city;
            this.Flash = flash;
            this.TargetNetwork = targetNetwork;
        }

        public string BusinessUserId { set; get; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string TargetNetwork { get; set; }
        public bool Flash { get; set; }
        public decimal Money { get; set; }

        public List<string> ListKeywork { get; set; }



    }
}
