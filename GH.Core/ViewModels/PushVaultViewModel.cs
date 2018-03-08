using GH.Core.BlueCode.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SRFIViewModel : TransactionalInformation
    {
        public SRFIViewModel()
        {
            ListCampaign = new List<CampaignVM>();
        }

        public string BusId { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
    
         public List<CampaignVM> ListCampaign { get; set; }
    }
   

    public class CampaignVM
        {
        public CampaignVM()
        {
            Fields = new List<FieldViewModel>();
        }
        public string UserId { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
        public string Description { set; get; }
        public string CampaignId { set; get; }
      
        public string Status { set; get; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Verb { get; set; }
        public DateTime? Created { get; set; }
        public string Comment { get; set; }
        public string TermsUrl { get; set; }
        public List<FieldViewModel> Fields { get; set; }

        }

    public class PushVaultViewModel
    {
        public PushVaultViewModel()
        {
            Fields = new List<FieldViewModel>();
        }
        public string Description { set; get; }
        public string CampaignId { set; get; }
        public string Status { set; get; }
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public List<FieldViewModel> Fields { get; set; }

    }

    public class FieldViewModel
    {
        public string displayName { get; set; }
        public string displayName2 { get; set; }
        public string id { get; set; }
        public string jsPath { get; set; }
        public string label { get; set; }
        public bool optional { get; set; }
        public bool optional2 { get; set; }
        public string type { get; set; }

        public List<string> options { get; set; }

    }

    public class BusinessPushVaulViewModel
    {
        public BusinessPushVaulViewModel()
        {
            ListPushToVault = new List<PushVaultViewModel>();
        }
        public List<PushVaultViewModel> ListPushToVault { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
       
    
    }

}