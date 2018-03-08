using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;

using System.Collections.Generic;


namespace GH.Core.BlueCode.BusinessLogic
{

    public interface ICampaignLogic
    {
        CampaignModel GetCampaignById(CampaignModel model);
        List<CampaignModel> GetCampaignList(CampaignModel model);
        int InsertCampaign(CampaignModel model);
        int UpdateCampaignById(CampaignModel model);
    }
}
