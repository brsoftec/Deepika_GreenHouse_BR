using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace GH.Core.BlueCode.Entity.Delegation
{
    public class DelegationGroupVault
    {
        public string name { set; get; }
        public bool read { set; get; }
        public bool write { set; get; }

        public string jsonpath { set; get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] jsonpaths { set; get; }
    }
}