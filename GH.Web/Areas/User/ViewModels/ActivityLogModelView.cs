using GH.Core.BlueCode.Entity.ActivityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ActivityLogModelView : TransactionalInformation
    {
        public ActivityLogModelView()
        {
            Listitems = new List<ActivityLog>();
        }

        public IList<ActivityLog> Listitems { set; get; }

        public string UserId { set; get; }



    }
}