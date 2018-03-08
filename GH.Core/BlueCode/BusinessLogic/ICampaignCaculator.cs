using GH.Core.BlueCode.Entity.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface ICampaignCaculator
    {
        CampaignUserFilterResult CalculateNumberOfUser(CampaignFilter campaignFilter);
    }
}
