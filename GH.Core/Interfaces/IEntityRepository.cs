using MongoDB.Driver;

namespace GH.Core.Interfaces
{
    public interface IEntityRepository<T> where T : IMongoEntity
    {
        void Create(T entity);

        DeleteResult Delete(string id);

        T GetById(string id);

        UpdateResult Update(T entity);
    }
}
