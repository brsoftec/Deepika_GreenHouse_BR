using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class SpendCampaign
    {
        public string Type { get; set; }
        public string Money { get; set; }
        public string Currentcy { get; set; }
        public string EffectiveDate { get; set; }
        public string EndDate { get; set; }
       
    }
}