using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class TwitterFeed
    {
        public string UserID
        {
            get;
            set;
        }

        public string CreatedTime { get; set; }

        public string Content
        {
            get;
            set;
        }

        public string ImagePath { get; set; }
    }
}