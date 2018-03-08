using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.ViewModels;
using MongoDB.Bson;

using System.Collections.Generic;


namespace GH.Core.BlueCode.BusinessLogic
{

    public interface ICampaignBusinessLogic
    {
        BsonDocument GetCampaignRegistrationTemplate();
        BsonDocument GetCampaignAdvertisingTemplate();
        string InsertCampaign(string userId, BsonDocument campaign);
     
        BsonDocument GetVaultTreeForRegistration();
        DataList<CampaignListItem> GetCampaignByBusinessUser(string businessUserId, string campaignType = "All",
            string campaignStatus = "All", bool withoutDraft = false, int pageIndex = 1, int pageSize = 10, bool withoutTemplate = false, 
            bool withoutSRFI = false, string keyword = "", bool isnotcasebbusid = false, bool withoutPustToVault = false,bool withoutHandshake = false, List<string> busids = null, bool isbothPushtovaultandhandshake = false);
        string GetCampaignSRFIId(string businessUserId);
        void UpdateCampaignStatus(string campaignId, string campaignStatus);
        List<CampaignItemForHomeFeed> GetActiveCampaignsForUser(string userId,string campaignpublicurl = "", string campaignType = "All", 
            bool withoutSRFI = false, string keysearch = "",bool withoutPustToVault=true, bool withoutHandshake = true);

        List<CampaignItemForHomeFeed> GetInteractionFeedFromBusiness(string accountId, string businessAccountId);
        List<CampaignRegistrationFormField> GetUserInformationForCampaign(string userId, string campaignId);
        BsonDocument GetCampaignById(string campaignId);
        void SaveCampaign(string campaignId, string campaignJson);
        void DeleteCampaign(string campaignId);
        void RemoveCampaign(string campaignId);
        void SetBoostAdvertising(string campaignId);
        BsonDocument GetCampaignSRFITemplate();
        CampaignItemForHomeFeed GetCampaignInfor(string campaignId);
        BsonDocument GetCampaignEventTemplate();
        BsonDocument GetFormTemplate();
        BsonDocument GetCampaignTemplate(string name);
        bool CheckEndDateCampaign(string campaignId);
        void UpdateCampaign(string campaignId, BsonDocument campaignContent);
        List<PushVaultViewModel> GetAllPushVaultByUser(string accountId);
        List<PushVaultViewModel> GetAllPushVaultActiveByUser(string accountId);
        List<FieldinformationVault> UpdateCampaignCustom(string campaignId, string userId, List<FieldinformationVault> listFieldInfo);
        List<CampaignVM> GetCampaignForBusinessByType(string accountId, string campaignType);
        string UserIdByCampaignId(string campaignId);
        CampaignVM CampaignById(string campaignId);
        List<CampaignVM> GetCampaignActiveForBusinessByType(string accountId, string campaignType);
    }
}
