using System.Collections.Generic;
using System.Threading.Tasks;
using GH.Core.Interfaces;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.IRepositories
{
    public interface IDisabledUserRepository : IEntityRepository<DisabledUser>
    {
        IFindFluent<DisabledUser, DisabledUser> GetDisableUserByEmail(string email);
        IFindFluent<DisabledUser, DisabledUser> GetDisableUserByUserId(ObjectId userId);
        IFindFluent<DisabledUser, DisabledUser> GetAllDisableUser(FilterDefinition<DisabledUser> filter, int? skip, int? length);
        IFindFluent<DisabledUser, DisabledUser> GetTotal(FilterDefinition<DisabledUser> filter);
        Task<UpdateResult> EnableMany(IEnumerable<DisabledUser> disabledUsers);
    }
}