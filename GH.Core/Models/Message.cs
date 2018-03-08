
using System;
using System.Collections.Generic;
using GH.Core.ViewModels;
using Newtonsoft.Json;

namespace GH.Core.Models
{
     
    public class ConversationMessage
    {
        public ConversationMessage()
        {
            //fieldsdrfi = new List<FieldinformationVault>();
        }
        public string messageid { set; get; }
        public DateTime created { get; set; }
        public string text { get; set; }
        [JsonIgnore]
        public DateTime read { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? fromMe { get; set; }
        public string fromAccountId { get; set; }
        public string toReceiverId { get; set; }
        [JsonIgnore]
        public bool isGroup { get; set; }
        public string conversationId { get; set; }

        public bool isread { get; set; }

        public bool Isdeleted { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Datedeleted { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string type { set; get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string drfiRequestId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<UserDrfiField> drfiFields { set; get; }       
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> drfiPaths { set; get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string jsonFieldsdrfi { set; get; }        
     
    
    }
}