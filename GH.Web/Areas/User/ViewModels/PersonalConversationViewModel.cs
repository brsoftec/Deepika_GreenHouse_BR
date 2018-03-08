using GH.Core.BlueCode.Entity.InformationVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace GH.Web.Areas.User.ViewModels
{
    public class PersonalConversationsResult 
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public List<PersonalConversation> conversations { get; set; } 
    }

    public class PersonalConversation
    {
        //public PersonalConversation()
        //{
        //    messages=new List<ConversationMessage>();
        //    users=new List<ConversationFrom>();
        //}
        public string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string name { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool isGroupChat { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string owner { get; set; }
        public ConversationFrom from { get; set; }
        public List<string> userIds { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ConversationFrom> users { get; set; }
        public List<ConversationMessage> messages { get; set; }

        public int unreadCount { set; get; }
    }

    public class ConversationFrom
    {
        public string id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool online { get; set; }
    }

   
}