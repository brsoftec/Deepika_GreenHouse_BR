using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class AccountRegistrationTimeStatResult
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TimePeriod Period { get; set; }
        public int Count { get; set; }
        public int CountBusiness { get; set; }
        public int CountPersonal { get; set; }
    }

    public enum TimePeriod
    {
        Day,
        Week,
        Month,
        Year
    }
}