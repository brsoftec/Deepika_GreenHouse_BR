using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GH.Core.BlueCode.DataAccess
{
    public interface IRepository<T> : IDisposable where T : IMongoDBEntity
    {
        IMongoCollection<BsonDocument> GetCollection(string name);
        void Add(T item);
        void AddAsync(T item);
        void Add(IEnumerable<T> items);
        void AddAsync(IEnumerable<T> items);
        void Delete(ObjectId id);
        void DeleteAsync(ObjectId id);
        void Delete(T item);
        void DeleteAsync(T item);
        void Delete(IEnumerable<T> items);
        void DeleteAsync(IEnumerable<T> items);
        void Delete(Expression<Func<T, bool>> expression);
        void DeleteAsync(Expression<Func<T, bool>> expression);
        void Update(T item);
        void UpdateAsync(T item);
        void Update(IEnumerable<T> item);
        void UpdateAsync(IEnumerable<T> item);
        T Single(ObjectId id);
        T Single(Expression<Func<T, bool>> expression);
        IQueryable<T> Many(Expression<Func<T, bool>> expression);
        IQueryable<T> Many(Expression<Func<T, bool>> expression, int page, int pageSize);

    }
}
