
using GH.Core.BlueCode.Entity.ProfilePrivacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IProfilePrivacyBusinessLogic
    {
        string InsertProfilePrivacy(ProfilePrivacy profilePrivacy);
        Privacy InsertFieldPrivacy(string accountId, Privacy field);
        ProfilePrivacy GetProfilePrivacyById(string id);
        ProfilePrivacy GetProfilePrivacyByAccountId(string accountId);
        Privacy GetRequestHandshakePrivacy(string accountId, string field);
        Privacy GetRoleFieldPrivacy(string accountId, string field);
        Privacy UpdateRequestHandshakePrivacy(string accountId, string field, string role = null);
        Privacy UpdateFieldPrivacy(string accountId, Privacy field);
        string UpdateProfilePrivacy(ProfilePrivacy profilePrivacy);
        void DeleteProfilePrivacyById(string id);
    }
}