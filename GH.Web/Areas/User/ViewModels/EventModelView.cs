using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class EventModelView : TransactionalInformation
    {
        public EventModelView()
        {
            Listitems = new List<EventListItem>();
        }
        public string EventType { set; get; }
       
        public string StrEvent { set; get; }

        public object EventTemplate { set; get; }

        public string UserId { set; get; }

        public string EventId { set; get; }

        public List<EventListItem> Listitems;
       
    }
}