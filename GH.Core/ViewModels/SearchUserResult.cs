using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SearchUserResult
    {
        public long Total { get; set; }

        public List<Account> Accounts { get; set; }
    }
}