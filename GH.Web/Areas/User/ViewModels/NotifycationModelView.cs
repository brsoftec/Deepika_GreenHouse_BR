using GH.Core.BlueCode.Entity.Notification;
using System.Collections.Generic;

namespace GH.Web.Areas.User.ViewModels
{
    public class NotifycationModelView : TransactionalInformation
    {
        public NotifycationModelView()
        {
            Listitems = new List<NotificationMessage>();
        }
       
        public IList<NotificationMessage> Listitems { set; get; }
        
        public string UserId { set; get; }
     


    }   

}