using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class FriendsInNetworkViewModel
    {
        public List<FriendInNetworkInfo> Friends { get; set; }
        public int Total { get; set; }
    }

    public class FriendInNetworkInfo
    {
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
    

    }
    public class FriendsInNetworkFullViewModel
    {
        public List<FriendInNetworkFullInfo> Friends { get; set; }
        public int Total { get; set; }
    }
    public class FriendInNetworkFullInfo
    {
        public string NetworkId { get; set; }
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Relationship { get; set; }
        public string UserId { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

        public string BirthDay { get; set; }
    }

    public class EmergencyFullInfo
    {
        public string NetworkId { get; set; }
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Relationship { get; set; }
        public string UserId { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

        public string BirthDay { get; set; }
    }
}