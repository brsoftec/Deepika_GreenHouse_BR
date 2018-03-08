using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class FieldHandShakeViewModel
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string OldValue { set; get; }
        public string NewValue { set; get; }
        public DateTime UpdateDate { set; get; }
        public bool IsChange { set; get; }
   
    }
}