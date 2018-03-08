using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class NotificationContent
    {
        public NotificationContent() { }
        public NotificationContent(string title, string body, params string[] sendTo)
        {
            this.Title = title;
            this.Body = body;
            this.SendTo = sendTo;
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public string[] SendTo { get; set; }
    }
}