using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class PhoneViewModel
    {
        public string CodeCountry { get; set; }
        public string PhoneNumber { get; set; }
    }   
    public class FormattedPhone
    {
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}