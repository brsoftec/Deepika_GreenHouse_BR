using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{   
    public class RegistrationByGenderResult
    {
        public string Gender { get; set; }
        public int Count { get; set; }

        public List<RegistrationByGenderAge> CountByAges { get; set; }
    }

    public class RegistrationByGenderAge
    {
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public int Count { get; set; }
    }
}