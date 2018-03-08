using GH.Core.BlueCode.Entity.InformationVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    // Vu
    public class HandShakeViewModel
    {
        public string UserId { set; get; }
        public string DisplayName { set; get; }

        public List<FieldinformationVault> ListOfFields { get; set; }
        public List<FieldinformationVault> ListOfFieldsOld { get; set; }
        public string DateUpdateJson { set; get; }
    }
}



