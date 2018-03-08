using System;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.PostHandShake;
namespace GH.Core.BlueCode.Entity.Campaign
{
    public class CampaignListItem
    {
        public CampaignListItem()
        {
            postHandShake = new PostHandShake.PostHandShake();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public string starttime { get; set; }
        public string startdate { get; set; }
        public bool AllowCreateQrCode { set; get; }
        public string PublicURL { set; get; }
        public string endtime { get; set; }
        public string enddate { get; set; }

        public string location { get; set; }
        public string theme { get; set; }

        public string usercodetype { get; set; }

        public string usercode { get; set; }
        public string usercodecurrentcy { get; set; }
        public string termsAndConditionsFile { get; set; }
        public string CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string CampaignName { get; set; }
        public string BusinessId { get; set; }
        public string BusinessUserId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessAvatar { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public int MaxAge { get; set; }
        public int MinAge { get; set; }
        public string Gender { get; set; }
        public string LocationType { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public int SpendMoney { get; set; }
        public DateTime SpendEffectDate { get; set; }
        public DateTime SpendEndDate { get; set; }

        public string TimeType { get; set; }
        public string ResidenceStatus { get; set; }
        public string EstimatedReach { get; set; }
        public string Image { get; set; }
        public string TargetLink { get; set; }
        public string Verb { get; set; }
        public string SocialShare { get; set; }
        public string TargetNetwork { get; set; }
        public BsonArray Fields { get; set; }

        public PostHandShake.PostHandShake postHandShake { set; get; }
        public string EnDate { get; set; }
        public int Participants { get; set; }
        public int Viewers { get; set; }
        public int Target { get; set; }

        public string Boost { get; set; }
        public int Cost { set; get; }
        
        public bool Following { get; set; }
        

        public int CountUserInvitedHandshake { set; get; }


        public int indexoder { set; get; }
    }
}
