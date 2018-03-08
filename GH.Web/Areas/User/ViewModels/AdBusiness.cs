using GH.Core.BlueCode.Entity.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class AdBusiness
    {
        public AdBusiness()
    {
            AdBusinesUsers = new List<AdBusinessUser>();
            results = new List<UserSearchResult>();
    }
      
        public List<AdBusinessUser> AdBusinesUsers { set; get; }
        public int TotalUser { set; get; }
        public List<UserSearchResult> results { set; get; }



    }
public class PrivacyProfile
{

    public string PhotoUrl { set; get; }
    public string PictureAlbum { set; get; }

    public string Address { set; get; }

    public string Phone { set; get; }

    public string Email { set; get; }

    public string Website { set; get; }

    public string WorkTime { set; get; }

    public string Profile { set; get; }


}
    public class AdBusinessUser
    {
        public AdBusinessUser()
        {
            PrivacyProfile = new PrivacyProfile();
        }
        public string id { set; get; }
        public string AccountId { set; get; }

        public string desc { set; get; }

        public string displayName { set; get; }

        public string email { set; get; }

        public PrivacyProfile PrivacyProfile { set; get; }
        public string avatar { set; get; }

        public bool following { set; get; }
       

    }
}