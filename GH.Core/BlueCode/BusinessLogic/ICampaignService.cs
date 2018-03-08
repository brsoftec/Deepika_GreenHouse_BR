using GH.Core.BlueCode.Entity.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface ICampaignService
    {
        Campaign GetCampaignById(string id);
        List<Campaign> GetListByType( string type = null);
        List<Campaign> GetListByUserId(string userId = null);
        List<Campaign> GetListByStatus(string userId = null);
        List<Campaign> GetCampaignActive(string userId = null, string type = null);
        string InsertCampaign(Campaign campaignEntity);
        string DeleteCampaign(string id);
       
        string Update(Campaign campaignEntity);
    }
}