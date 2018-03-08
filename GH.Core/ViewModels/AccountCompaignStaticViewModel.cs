using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class AccountCompaignStaticViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? Todate { get; set; }
    }
}











