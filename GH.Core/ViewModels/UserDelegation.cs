using System;
using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Delegation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.ViewModels
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class DelegationProfile
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserDelegation
    {
        public UserDelegation() {
            Permissions = new List<DelegationGroupVault>();
        }
        public string DelegationId { get; set; }
        public string DelegationRole { get; set; }
        public string Direction { get; set; }
        public string FromAccountId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DelegationProfile FromProfile { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DelegationProfile ToProfile { get; set; }
        public string ToAccountId { get; set; }

        public string Status { get; set; }
        public string Begins { get; set; }
        public string Expires { get; set; }

        public List<DelegationGroupVault> Permissions { set; get; }
    }
}