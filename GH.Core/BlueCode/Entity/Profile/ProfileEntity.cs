using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Notification;
using MongoDB.Bson;
using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Profile
{
    public class UserProfile:IMongoDBEntity
    {
        public ObjectId Id
        {
            get; set;
        }
        public string AccountId { get; set; }
        public string UserName { set; get; }

        public string LastName { set; get; }

        public string FirstName { set; get; }

        public string Email { set; get; }

        public string Password { set; get; }

        public string VaultInformationId { set; get; }

        public string PhoneNumber { set; get; }

        public string CityName { set; get; }

        public string CountryName { set; get; }

        public string RegionName { set; get; }

        public string Age { set; get; }

        public string UserType { set; get; }

        public string Gender { get; set; }

   
        public IEnumerable<DelegationItemTemplate> Delegations { get; set; }
        public IEnumerable<Friend> Friends { get; set; }
        public LatestNotification LastestNotification { get; set; }

    }
}
