using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Search
{
    public class UserSearchResult
    {
        public string Userid { set; get; }
        public string UserAcccountid { set; get; }

        public string PhotoUrl { set; get; }

        public string DisplayName { set; get; }

        public string Email { set; get; }
        public string Status { set; get; }
        public string Description { set; get; }

        public string FirstName { set; get; }

        public string LastName { set; get; }

        public string StatusFriend { set; get; }
    }
}