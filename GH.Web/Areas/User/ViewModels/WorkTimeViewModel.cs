using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class WorkTimeViewModel
    {
        public DateTime? WorkHourFrom { get; set; }
        public DateTime? WorkHourTo { get; set; }
        public List<string> Workdays { get; set; }
    }
}