using GH.Core.BlueCode.Entity.InformationVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class InfoViewModel : TransactionalInformation
    {
      
       public string UserId { set; get; } 
        public InfoField infoField { set; get; }
    }
}