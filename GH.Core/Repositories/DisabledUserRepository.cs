using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.IRepositories;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.Repositories
{
    public class DisabledUserRepository : EntityRepository<DisabledUser>, IDisabledUserRepository
    {
        public override UpdateResult Update(DisabledUser entity)
        {
           return MongoConnectionHandler.MongoCollection.UpdateOne(
                Builders<DisabledUser>.Filter.Eq(t => t.Id, entity.Id),
                Builders<DisabledUser>.Update.Set(a => a.IsEnabled, entity.IsEnabled)
                    .Set(a => a.ModifiedOn, entity.ModifiedOn).Set(a => a.ModifiedBy, entity.ModifiedBy));
        }

        public IFindFluent<DisabledUser, DisabledUser> GetDisableUserByEmail(string email)
        {
            return
                MongoConnectionHandler.MongoCollection.Find(x => x.User.Profile.Email == email && !x.IsEnabled).Limit(1)
                    .Sort(Builders<DisabledUser>.Sort.Descending("ModifiedOn"));
        }

        public IFindFluent<DisabledUser, DisabledUser> GetDisableUserByUserId(ObjectId userId)
        {
            return
                MongoConnectionHandler.MongoCollection.Find(x => x.UserId == userId && !x.IsEnabled).Limit(1)
                    .Sort(Builders<DisabledUser>.Sort.Descending("ModifiedOn"));
        }

        public IFindFluent<DisabledUser, DisabledUser> GetAllDisableUser(FilterDefinition<DisabledUser> filter,
            int? skip, int? length)
        {
            return MongoConnectionHandler.MongoCollection.Find(filter).Skip(skip).Limit(length);
        }

        public IFindFluent<DisabledUser, DisabledUser> GetTotal(FilterDefinition<DisabledUser> filter)
        {
            return MongoConnectionHandler.MongoCollection.Find(filter);
        }

        public Task<UpdateResult> EnableMany(IEnumerable<DisabledUser> disabledUsers)
        {
            var builders = Builders<DisabledUser>.Filter;
            var filter = builders.In(x => x.Id, disabledUsers.Select(x => x.Id));
            var result = MongoConnectionHandler.MongoCollection.UpdateManyAsync(filter,
                Builders<DisabledUser>.Update.Set(a => a.IsEnabled, true).CurrentDate(x => x.ModifiedOn));
            return result;
        }
    }
}