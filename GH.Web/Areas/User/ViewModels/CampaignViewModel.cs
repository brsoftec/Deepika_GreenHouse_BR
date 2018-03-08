using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Campaign;
using MongoDB.Bson;

namespace GH.Web.Areas.User.ViewModels
{

   

    public class CampaignViewModel : TransactionalInformation
    {

        public class ExportAllHandshakeModel
        {
            public string UserId { set; get; }
            public string DisplayName { set; get; }

            public List<FieldinformationVault> ListOfFields { get; set; }
            public string groupsJson { get; set; }
            public List<FieldinformationVault> ListOfFieldsOld { get; set; }
            public string DateUpdateJson { set; get; }

        }


        public CampaignViewModel()
        {
            CampaignFilter = new CampaignFilter();
            ListRoles = new List<Role>();
            Isviewedit = true;
            Isviewapproved = true;
        }      

        public string StrEvent { set; get; }
        public string campaignpublicid { set; get; }
        public object EventTemplate { set; get; }
        public string UserId { set; get; }
        public string BusinessUserId { set; get; }
        public string CampaignId { set; get; }
        public string CampaignType { set; get; }
        public string CampaignName { set; get; }
        public string CampaignDescription { set; get; }
        public string CampaignStatus { set; get; }
        public object CampaignTemplateAdvertising { set; get; }
        public object Campaign { set; get; }
        public string StrCampaignAdvertising { set; get; }
        public bool IsActive { set; get; }
        public bool Isdraff { set; get; }

        public bool Istemplate { set; get; }

        public string Urlpublic { set; get; }

        public CampaignFilter CampaignFilter { set; get; }
        public CampaignUserFilterResult CampaignUserFilterResult { set; get; }
        public List<CampaignListItem> Listitems;
        public List<string> BusinessIdList { get; set; }
        public List<string> BusinessCampaignIdList { get; set; }
        public List<string> CampaignIdInPostList { get; set; }
        public List<NewFeedsViewModel> NewFeedsItemsList { get; set; }
        public List<FieldinformationVault> ListOfFields { get; set; }
        public List<FieldinformationVault> ListOfFieldsOld { get; set; }
        public List<ExportAllHandshakeModel> ExportAllHandshakeModels { set; get; }
        


        public string keyword { set; get; }
        public List<Role> ListRoles { get; set; }
        public bool Isviewedit { get; set; }
        public bool Isviewapproved { get; set; }

        public string handshakeupadte { set; get; }
        
        public class FieldGroup
        {
            public string name { set; get; }
            public string displayName { set; get; }
        }
        
        public List<FieldGroup> Groups { set; get; }
    }
}