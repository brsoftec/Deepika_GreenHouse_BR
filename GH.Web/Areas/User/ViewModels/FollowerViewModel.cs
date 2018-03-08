using System.Collections.Generic;
using System.Linq;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;

namespace GH.Web.Areas.User.ViewModels
{
    public class FollowerViewModel : TransactionalInformation
    {
        public string UserId { set; get; }
        public string BusinessUserId { set; get; }
        public string CampaignId { get; set; }
        public string CampaignType { set; get; }
        public string CampaignName { set; get; }
        public List<Follower> FollowerList { get; set; }
        public List<ShortProfile> BusinessList { get; set; }
        public List<BusinessProfileViewModel> BusinessProfileList { get; set; }
        public List<ActivityLog> ActivityLogList { get; set; }
        public List<Follower> FollowerListByGender { get; set; }
        
        public List<IGrouping<string, Follower>> FollowerListByCountry { get; set; }
        public List<IGrouping<string, Follower>> FollowerListByCity { get; set; }
    }

}