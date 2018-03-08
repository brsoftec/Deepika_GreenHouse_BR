using System.Threading.Tasks;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.IServices
{
    public interface IDisabledUserService
    {
        DisabledUser DisabledUser(DisabledUser disabledUser);
        bool IsDisabled(ObjectId userId);
        bool IsExisted(ObjectId userId);
        bool IsDisabled(string email);
        DisabledUser GetDisabledUserById(string id);
        DisabledUser GetDisabledUserByEmail(string email);
        Task<UpdateResult> EnableUsers(EnableParameters parameter);
        UpdateResult EnableUsers(ObjectId userId);
        DisableUserResult GetAllDisableUser(DisableUserParamter disableUserParamter);
    }
}
