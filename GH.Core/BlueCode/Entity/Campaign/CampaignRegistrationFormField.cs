using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Campaign
{
    public class CampaignRegistrationFormField
    {
        public string BusinessUserId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string ControlType { get; set; }
        public List<string> Values { get; set; }
        public string UserInfor { get; set; }
    }
}
