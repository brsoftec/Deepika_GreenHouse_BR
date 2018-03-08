using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class InviteViewModel
    {
        public string FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string ToUserId { get; set; }
        public string ToDisplayName { get; set; }
        public string CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string Comment { get; set; }
        public string InviteType { get; set; }
        public List<string> ListEmailOutSite { get; set; }
        public List<string> ListEmailInSite { get; set; }
        public List<string> ListEmail { get; set; }
         
    }
    public class RegisViewModel
    {
        public RegisViewModel()
        {
            Fields = new List<FieldinformationVault>();
        }
        public string CampaignId { get; set; }
        public string Comment { get; set; }
        public string ToUserId { get; set; }
        public string NotificationId { get; set; }
     
        public string TermsUrl { get; set; }
        public List<FieldinformationVault> Fields { get; set; }

    }
}