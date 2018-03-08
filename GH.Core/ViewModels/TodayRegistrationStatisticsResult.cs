using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TodayRegistrationStatisticsResult
    {
        public int Count { get; set; }
        public int DifferenceWithYesterdayOfPersonal { get; set; }
        public int DifferenceWithYesterdayOfBusiness { get; set; }
    }
}