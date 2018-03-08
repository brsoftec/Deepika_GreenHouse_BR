
using GH.Core.BlueCode.Entity.InformationVault;
using System.Collections.Generic;


namespace GH.Web.Areas.User.ViewModels
{
    public class VaultInformationModelView : TransactionalInformation
    {
        public object VaultInformation { set; get; }
        public string StrVaultInformation { set; get; }
        public string VaultInformationId { set; get;}
        public string BusinessUserId { get; set; }
        public string CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string UserId { set; get; }
        public string DelegationId { set; get; }
        public string DelegateeId { set; get; }
        public string StrEvent { set; get; }
        public List<FieldinformationVault> Listvaults { set; get; }
        public string usernotifycationid { set; get; }
        public string usernotifycationemail { set; get; }
        public string status { set; get; }
        public string pushtype { set; get; }
        public bool isEdit{ set; get; }
    }
   
}