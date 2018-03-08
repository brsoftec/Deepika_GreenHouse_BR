using GH.Core.BlueCode.Entity.Common;
using static GH.Core.BlueCode.Entity.Campaign.Campaign;

namespace GH.Core.ViewModels
{
    public class CampaignDto : TransactionalInformation
    {

        public CampaignDto()
        {
            campaign = new CampaignContent();
        }

        public string Id { get; set; }
        public string userId { get; set; }
        public string status { get; set; }
        public CampaignContent campaign { get; set; }

    }
}