using System;
using MongoDB.Bson;

namespace GH.Core.BlueCode.Entity.Event
{
    public class EventListItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public string username { get; set; }

        public string type { get; set; }

        public string starttime { get; set; }
        public string startdate { get; set; }

        public string endtime { get; set; }
        public string enddate { get; set; }

        public string location { get; set; }
        public string theme { get; set; }

        public string timetype { get; set; }
        public string note { get; set; }

        public bool syncgoogle { get; set; }

    }
}
