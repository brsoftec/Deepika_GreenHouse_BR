using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class ChangeViewModel
    {
       public ChangeViewModel()
        {
            Values = new List<ChangeValue>();
        }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime DateChange { get; set; }
        public string BodyEmail { get; set; }
        public List<ChangeValue> Values { get; set; }

    }
    public class ChangeValue
    {
        public string Name { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}