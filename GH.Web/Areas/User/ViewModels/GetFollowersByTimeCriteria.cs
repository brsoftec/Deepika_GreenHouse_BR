using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class GetFollowersByTimeCriteria
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}