using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class ShareResult
    {
        public List<string> Errors { get; set; }

        /// <summary>
        /// The share post
        /// </summary>
        public SocialPost SharePost { get; set; }

        /// <summary>
        /// The post is shared
        /// </summary>
        public SocialPost PostShared { get; set; }

        public ShareResult()
        {
            this.Errors = new List<string>();
        }
    }
}