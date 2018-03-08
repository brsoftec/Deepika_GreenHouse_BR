using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace GH.Core.BlueCode.Entity.Campaign
{
    public class CampaignItemForHomeFeed
    {
        public string Id { get; set; }
        public string BusinessUserId { get; set; }
        public string BusinessName { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public string CampaignName { get; set; }
        public string CampaignType { get; set; }
        
        public string Description { get; set; }
        public int MaxAge { get; set; }
        public int MinAge { get; set; }
        public string Gender { get; set; }
        public string LocationType { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string TargetNetwork { get; set; }
        //event
        public string termsAndConditionsFile { set; get; }
        public string starttime { get; set; }
        public string startdate { get; set; }


        public string endtime { get; set; }
        public string enddate { get; set; }

        public string location { get; set; }
        public string theme { get; set; }

        public string usercodetype { get; set; }

        public string usercode { get; set; }
        public string usercodecurrentcy { get; set; }

        public string TimeType { set; get; }
        public string PublicURL { get; set; }
        public string SocialShare { get; set; }

        public bool AllowCreateQrCode { get; set; }
        public string Verb { get; set; }


        public int SpendMoney { get; set; }
        public DateTime SpendEffectDate { get; set; }
        public DateTime SpendEndDate { get; set; }
        public string ResidenceStatus { get; set; }
        public string EstimatedReach { get; set; }
        public string Image { get; set; }
        public string TargetLink { get; set; }

        public object Fields { get; set; }
        public List<string> FollowerIds { get; set; }
    }
}
