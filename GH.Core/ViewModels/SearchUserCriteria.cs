using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SearchUserCriteria
    {
        public AccountType? AccountType { get; set; }
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }

        public int? Start { get; set; }
        public int? Length { get; set; }
    }
}