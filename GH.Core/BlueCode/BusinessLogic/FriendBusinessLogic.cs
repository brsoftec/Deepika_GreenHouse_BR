using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Profile;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class FriendBusinessLogic: IFriendBusinessLogic
    {
        MongoRepository<Friend> repository = new MongoRepository<Friend>();
        public FriendBusinessLogic()
        {
        }

        public void AddFriend(string userId, string friendId, string groupName = "Nornal")
        {
            var profileBus = new ProfileBusinessLogic();
            var friendProfile = profileBus.GetProfileFromId(friendId);
            var friendCollection = MongoDBConnection.Database.GetCollection<Friend>("Friend");
            var friend = new Friend
            {
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                FriendId = friendId,
                FriendName = string.Format("{0} {1}", friendProfile.FirstName, friendProfile.LastName),
                Group = groupName
            };
            friendCollection.InsertOne(friend);
        }

        public IEnumerable<Friend> GetListOfFriend(string userId, string groupName = "All")
        {
            if (string.IsNullOrEmpty(groupName) || groupName.ToLower().Equals("all"))
                return repository.Many(f => f.UserId.Equals(userId)).AsEnumerable();

            return repository.Many(f => f.UserId.Equals(userId) && f.Group.Equals(groupName)).AsEnumerable();

        }

        public IEnumerable<Friend> GetFriendsForDelegation(string userId, string groupName = "All")
        {
            var trustedNetworkFriends = GetListOfFriend(userId, groupName);
            var profileBus = new ProfileBusinessLogic();
            var profile = profileBus.GetProfileFromId(userId);
            var delegatedFriendIds = profile.Delegations.Select(d => { return d.ToAccountId; });
            return trustedNetworkFriends.Where(f => !delegatedFriendIds.Contains(f.FriendId));
        }



    }
}
