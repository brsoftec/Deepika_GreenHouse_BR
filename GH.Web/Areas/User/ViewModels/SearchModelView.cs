using GH.Core.BlueCode.Entity.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{

    public class SearchModelView : TransactionalInformation
    {
        public SearchModelView()
        {
            results = new List<UserSearchResult>();
        }
        public string keyword { set; get; }

        public bool isbus { set; get; }
        public List<UserSearchResult> results { set; get; }
    }
}