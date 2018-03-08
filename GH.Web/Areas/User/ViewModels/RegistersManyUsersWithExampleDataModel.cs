using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class RegistersManyUsersWithExampleDataModel
    {
        public string Firstname { set; get; }

        public string Lastname { set; get; }

        public int CountUsers { set; get; }
        public int NmberRandomKeyword { set; get; }

        public int numbercampaign { set; get; }

        public int countcampaignkeywords { set; get; }

        public int countdays { set; get; }
    }
}