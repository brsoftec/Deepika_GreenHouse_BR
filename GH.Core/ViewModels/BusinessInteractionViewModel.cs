using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class BusinessInteractionViewModel
    {
        public BusinessInteractionViewModel()
        {
            criteria = new InteractionCriteria();
            eventInfo = new InteractionEventViewModel();
            comments = new List<InteractionComment>();
        }
        public string id { get; set; }
        public DateTime created { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public string image { get; set; }
        public string targetUrl { get; set; }
        public string termsType { get; set; }
        public string termsUrl { get; set; }

        public bool? paid { get; set; }
        public decimal price { get; set; }
        public string priceCurrency { get; set; }

        public bool? indefinite { get; set; }
        public DateTime from { get; set; }
        public DateTime? until { get; set; }

        public string verb { get; set; }
        public string target { get; set; }
        public InteractionCriteria criteria { get; set; }
        public string boost { get; set; }
        public string distribute { get; set; }

        public string socialShare { get; set; }
        public InteractionEventViewModel eventInfo { get; set; }

        public string notes { get; set; }
        public List<InteractionComment> comments { get; set; }

        public string fieldsJson { get; set; }
        public string groupsJson { get; set; }

        public string participants { get; set; }

    }
    public class BusinessInteractionPayload
    {
        public string id { get; set; }
        public string status { get; set; }
        public string json { get; set; }
    }

}