using System.Configuration;
using GH.Core.Interfaces;
using MongoDB.Driver;
using GH.Core.Helpers;

namespace GH.Core.Handler
{
    public class MongoConnectionHandler<T> where T : IMongoEntity
    {
        public const string GREENHOUSE_DB_CONNECTION_NAME = "MongoConnection";
        public const string GREENHOUSE_DB_CONFIG_DBNAME = "GreenHouseDbName";

        public IMongoCollection<T> MongoCollection { get; private set; }
        public IMongoDatabase Db { get; private set; }

        public MongoConnectionHandler()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[GREENHOUSE_DB_CONNECTION_NAME].ConnectionString;
            var url = new MongoUrl(connectionString);
            var mongoClient = MongoHelper.GetMongoClient(url);
            Db = mongoClient.GetDatabase(ConfigurationManager.AppSettings[GREENHOUSE_DB_CONFIG_DBNAME]);
            
            MongoCollection = Db.GetCollection<T>(typeof(T).Name + "s");
        }
    }
}