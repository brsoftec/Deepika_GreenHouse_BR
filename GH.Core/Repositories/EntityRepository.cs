using GH.Core.Handler;
using GH.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GH.Core.Repositories
{
    public abstract class EntityRepository<T> : IEntityRepository<T> where T : IMongoEntity
    {
        protected readonly MongoConnectionHandler<T> MongoConnectionHandler;

        protected EntityRepository()
        {
            MongoConnectionHandler = new MongoConnectionHandler<T>();
        }

        public virtual void Create(T entity)
        {
            //// Save the entity with safe mode (WriteConcern.Acknowledged)
            MongoConnectionHandler.MongoCollection.InsertOne(entity);
        }

        public virtual DeleteResult Delete(string id)
        {
            return MongoConnectionHandler.MongoCollection.DeleteOne(Builders<T>.Filter.Eq(e => e.Id, new ObjectId(id)));
        }

        public virtual T GetById(string id)
        {
            var entityQuery = Builders<T>.Filter.Eq(e => e.Id, new ObjectId(id));
            return MongoConnectionHandler.MongoCollection.Find(entityQuery).FirstOrDefault();
        }

        public abstract UpdateResult Update(T entity);
    }

  

}