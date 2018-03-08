using GH.Core.BlueCode.Entity.Profile;
using System.Collections.Generic;

namespace GH.Core.BlueCode.BusinessLogic
{
   public  interface IProfileBusinessLogic
    {
        void Add(UserProfile entity, string invitedDelegationId = null);
        void Update(UserProfile profile);
        IList<UserProfile> GetAll();

        IList<UserProfile> GetPagingAll(int page, int pageSize,out int totalrow);

        UserProfile GetProfileFromId(string id);

        UserProfile VerifyProfile(string username, string password);

        string sendMessagePin(string userid);

      
    }


}
