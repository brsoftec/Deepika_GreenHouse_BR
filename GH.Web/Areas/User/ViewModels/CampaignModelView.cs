using GH.Core.BlueCode.Entity.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class CampaignModelView : TransactionalInformation
    {
        public CampaignModelView()
        {
            CampaignFilter = new CampaignFilter();
        }
        public string CampaignType { set; get; }
        public string CampaignStatus { set; get; }
        public object CampaignTemplateAdvertising { set; get; }

        public string StrCampaignAdvertising { set; get; }

        public string UserId { set; get; }
        public string BusinessUserId { set; get; }
        public bool Isdraff { set; get; }
        public CampaignFilter CampaignFilter { set; get; }
        public CampaignUserFilterResult CampaignUserFilterResult { set; get; }
        public List<CampaignListItem> Listitems;
        public List<NewFeedsViewModel> NewFeedsItemsList { get; set; }
    }
}