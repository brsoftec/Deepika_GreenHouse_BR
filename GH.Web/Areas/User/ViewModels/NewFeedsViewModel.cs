using GH.Core.BlueCode.Entity.ActivityLog;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.BlueCode.Entity.PostHandShake;

namespace GH.Web.Areas.User.ViewModels
{
    public class NewFeedsViewModel : TransactionalInformation
    {
        public NewFeedsViewModel()
        {
            ActivityLogList = new List<ActivityLog>();
        }
        public string CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string CampaignName { get; set; }
        public string BusinessUserobjectId { get; set; }
        public string BusinessId { get; set; }
        public string BusinessUserId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessImageUrl { get; set; }
        public string UserName { get; set; }

        public string Timetype { get; set; }
        public string PublicURL { get; set; }
        public string PublicURLEncode { get; set; }
        public bool AllowCreateQrCode { get; set; }


        public string termsAndConditionsFile { get; set; }

        public PostHandShake PostHandShake { set; get; }

        //event
        public string starttime { get; set; }
        public string startdate { get; set; }

        public string endtime { get; set; }
        public string enddate { get; set; }

        public string location { get; set; }
        public string theme { get; set; }

        public string usercodetype { get; set; }
        public string usercode { get; set; }
        public string usercodecurrentcy { get; set; }

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
        public string ResidenceStatus { get; set; }
        public string EstimatedReach { get; set; }
        public string Image { get; set; }
        public string TargetLink { get; set; }

        public string Verb { get; set; }
        public bool Following { get; set; }
        
        public string TargetNetwork { get; set; }
        public object Fields { get; set; }

        public int MembersOfBusinessNbr { get; set; }
        public List<Follower> MembersOfBusiness { get; set; }
        public List<ShortProfile> BusinessList2 { get; set; }
        public List<Follower> CampaignMembersList { get; set; }
        public List<string> FollowerIds { get; set; }
        public List<ActivityLog> ActivityLogList { get; set; }
        
        public int Participants { get; set; }

        public bool Register { get; set; }

    }

}
