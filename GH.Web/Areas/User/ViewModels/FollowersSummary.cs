using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class FollowersSummary
    {
        public int NumberOfFollowers { get; set; }
        public bool Followed { get; set; }
    }
}