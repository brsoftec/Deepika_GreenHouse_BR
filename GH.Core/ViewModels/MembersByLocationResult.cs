using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class MembersByLocationResult
    {
        public string Country { get; set; }
        public string City { get; set; }
        public int Count { get; set; }
    }

    public enum LocationForReport
    {
        Country, City
    }
}