using GH.Core.BlueCode.Entity.Profile;
using System.Collections.Generic;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IFriendBusinessLogic
    {
        void AddFriend(string userId, string friendId, string groupName = "Nornal");
        IEnumerable<Friend> GetListOfFriend(string userId, string groupName = "All");
        IEnumerable<Friend> GetFriendsForDelegation(string userId, string groupName = "All");
    }
}
