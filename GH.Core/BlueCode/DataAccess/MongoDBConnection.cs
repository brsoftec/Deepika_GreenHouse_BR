using GH.Core.Helpers;
using MongoDB.Driver;
using NLog;
using System.Configuration;
using MongoDB.Bson.Serialization.Conventions;

namespace GH.Core.BlueCode.DataAccess
{
    public class MongoDBConnection
    {
        private static IMongoClient _client;
        private static IMongoDatabase _db;
        static Logger log = LogManager.GetCurrentClassLogger();

        public MongoDBConnection()
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camelCase", pack,
                t => t.FullName.Contains("Models.InteractionCampaign") || t.FullName.Contains("Models.Interaction."));
        }

        public static IMongoDatabase Database
        {
            get
            {
                if (_db == null)
                {
                    log.Debug("Connect MongoDB ");
                    var connectionString = ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString;
                    var url = new MongoUrl(connectionString);
                    _client = MongoHelper.GetMongoClient(url);
                    _db = _client.GetDatabase(url.DatabaseName);
                }
                return _db;
            }
        }
    }
}