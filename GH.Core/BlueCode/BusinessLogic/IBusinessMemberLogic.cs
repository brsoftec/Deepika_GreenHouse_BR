using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IBusinessMemberLogic
    {
        bool AddBusinessMember(Account userAcount, Account businessUserAccount, string dateadd="");
        bool RemoveBusinessMember(Account userAcount, Account businessUserAccount);
        IEnumerable<Follower> GetMembersOfBusiness(string businessUserId, string memberStatus = null);
        IEnumerable<ShortProfile> GetFollowingBusinesses(string userId);
    }
}
