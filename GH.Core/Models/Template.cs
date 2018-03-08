using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public string Title { get; set; }

        public string Body { get; set; }

        public string OpenPlaceHolder { get; set; }
        public string ClosePlaceHolder { get; set; }
        public NotificationTemplateType Type { get; set; }
    }

    public enum NotificationTemplateType
    {
        Email = 0,
        SMS = 1
    }
}