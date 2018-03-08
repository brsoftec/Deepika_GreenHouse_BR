using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Configuration;
using System.Linq.Expressions;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.Helpers;

namespace GH.Core.BlueCode.DataAccess
{
    public class MongoRepository<T> : IRepository<T> where T : IMongoDBEntity
    {
        private IMongoClient _client;
        private IMongoDatabase _db;

        public MongoRepository()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString;
            var url = new MongoUrl(connectionString);
            _client = MongoHelper.GetMongoClient(url);
            _db = _client.GetDatabase(url.DatabaseName);
        }
        public IMongoCollection<BsonDocument> GetCollection(string name)
        {
            return _db.GetCollection<BsonDocument>(name);
        }
        public void Add(T item)
        {
            _db.GetCollection<T>(typeof(T).Name).InsertOne(item);
        }
        public void AddAsync(T item)
        {
            _db.GetCollection<T>(typeof(T).Name).InsertOneAsync(item);
        }

        public void Add(IEnumerable<T> items)
        {
            _db.GetCollection<T>(typeof(T).Name).InsertMany(items);
        }

        public void AddAsync(IEnumerable<T> items)
        {
            _db.GetCollection<T>(typeof(T).Name).InsertManyAsync(items);
        }

        public void Delete(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _db.GetCollection<T>(typeof(T).Name).DeleteOne(filter);
        }
        public void DeleteAsync(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _db.GetCollection<T>(typeof(T).Name).DeleteOneAsync(filter);
        }

        public void Delete(T item)
        {
            _db.GetCollection<T>(typeof(T).Name).DeleteOne(item.ToBsonDocument());
        }
        public void DeleteAsync(T item)
        {
            _db.GetCollection<T>(typeof(T).Name).DeleteOneAsync(item.ToBsonDocument());
        }
        public void Delete(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Delete(item);
            }
        }
        public void DeleteAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                DeleteAsync(item);
            }
        }
        public void Delete(Expression<Func<T, bool>> expression)
        {
            _db.GetCollection<T>(typeof(T).Name).DeleteMany(expression);
        }

        public void DeleteAsync(Expression<Func<T, bool>> expression)
        {
            _db.GetCollection<T>(typeof(T).Name).DeleteManyAsync(expression);
        }

        public void Update(T item)
        {
            var filter = Builders<T>.Filter.Eq("_id", item.Id);
            _db.GetCollection<T>(typeof(T).Name).ReplaceOne(filter, item);
        }      
        public void UpdateField(T item, string fieldName, dynamic value)
        {
            var filter = Builders<T>.Filter.Eq("_id", item.Id);
            var update = Builders<T>.Update
                .Set(fieldName, value);
                //.CurrentDate("lastModified");
            _db.GetCollection<T>(typeof(T).Name).UpdateOne(filter, update);
        }
        public void UpdateAsync(T item)
        {
            var filter = Builders<T>.Filter.Eq("_id", item.Id);
            _db.GetCollection<T>(typeof(T).Name).ReplaceOneAsync(filter, item);
        }
        public void Update(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }
        public void UpdateAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                UpdateAsync(item);
            }
        }
        public T Single(Expression<Func<T, bool>> expression)
        {
            return Many(expression).SingleOrDefault<T>(expression);
        }
        public T Single(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return _db.GetCollection<T>(typeof(T).Name).Find(filter).SingleOrDefault();
        }
        public IQueryable<T> Many(Expression<Func<T, bool>> expression)
        {
            return _db.GetCollection<T>(typeof(T).Name).AsQueryable().Where(expression);
        }

        public IQueryable<T> Many(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return PagingExtensions.Page(Many(expression), page, pageSize);
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
