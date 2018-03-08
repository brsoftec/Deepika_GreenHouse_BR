using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.InformationVault
{
    public class FieldinformationVault<T>: FieldinformationVault
    {
        public new  T modelarrays { set; get; }
    }
}