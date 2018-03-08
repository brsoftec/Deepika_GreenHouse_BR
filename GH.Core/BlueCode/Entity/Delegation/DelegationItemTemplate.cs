using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.BlueCode.Entity.Delegation
{
    public class DelegationItemTemplate
    {
        public DelegationItemTemplate() {
            GroupVaultsPermission = new List<DelegationGroupVault>();
        }
        public string DelegationId { get; set; }
        public string FromAccountId { get; set; }
        public string FromUserDisplayName { get; set; }
        public string ToAccountId { get; set; }
        public string ToUserDisplayName { get; set; }
        public string InvitedEmail { get; set; }
        public string Image { get; set; }
        public string Direction { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string DelegationRole { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiredDate { get; set; }

        public List<DelegationGroupVault> GroupVaultsPermission { set; get; }
    }
}
