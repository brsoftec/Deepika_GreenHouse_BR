using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.ViewModels
{
    public class SearchSocialPostCriteria
    {
        public string Keyword { get; set; }

        public bool Regit { get; set; }
        public bool Facebook { get; set; }
        public bool Twitter { get; set; }
        
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int? Start { get; set; }
        public int? Length { get; set; }
    }
}