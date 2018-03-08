using GH.Core.BlueCode.Entity.ActivityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.BlueCode.Entity.Profile;
namespace GH.Web.Areas.User.ViewModels
{
    public class LocationModelView : TransactionalInformation
    {
        public LocationModelView()
        {
            Countries = new List<Country>();
            Cities = new List<City>();

        }
        public string CountryCode {
            set;get;
        }
        public string CountryName
        {
            set; get;
        }
        
        public List<Country> Countries { set; get; }

        public List<City> Cities { set; get; }


    }
}