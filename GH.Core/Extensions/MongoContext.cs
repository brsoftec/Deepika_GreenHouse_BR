using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using GH.Core.Helpers;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Reflection;
using GH.Core.BlueCode.Entity.Message;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;

namespace GH.Core.Extensions
{
    public static class MongoContext
    {
        public static GreenHouseMongoDatabase Db
        {
            get
            {
                return new GreenHouseMongoDatabase();
            }
        }
    }


    public class GreenHouseMongoDatabase
    {
        public const string GREENHOUSE_DB_CONNECTION_NAME = "MongoConnection";
        public const string GREENHOUSE_DB_CONFIG_DBNAME = "GreenHouseDbName";
        public const string SOCIAL_POST_COLLECTION = "SocialPost";
        public const string ACCOUNT_COLLECTION = "Account";
        public const string SECURITY_QUESTION_COLLECTION = "SecurityQuestion";
        public const string FRIEND_INVITATION_COLLECTION = "FriendInvitation";
    
        public const string NETWORK_COLLECTION = "Network";
        public const string UCB_COLLECTION = "UserCreatedBusiness";
        public const string DRAFPOST_COLLECTION = "BusinessPost";
        public const string ROLE_COLLECTION = "Role";
        public const string JOINING_BUSINESS_INVITATION_COLLECTION = "JoiningBusinessInvitation";
        public const string NOTIFICATION_COLLECTION = "Notification";
        public const string VERIFY_TOKEN_COLLECTION = "VerifyToken";
        public const string COUNTRY = "Country";
        public const string CITY = "City";
        public const string REGION = "Region";
      
        public const string GROUP_CHAT = "Conversation";
        public static readonly string[] COLLECTIONS = new string[]
        {
            "SocialPost", "Account", "SecurityQuestion", "FriendInvitation", "Network", "Role", "BusinessPost",
            "JoiningBusinessInvitation", "Notification", "Country", "City", "Region", "VerifyToken", "Conversation"
        };

        public IMongoDatabase Db { get; private set; }
        public IMongoCollection<SocialPost> SocialPosts { get; private set; }
        public IMongoCollection<Account> Accounts { get; private set; }
        public IMongoCollection<SecurityQuestion> SecurityQuestions { get; private set; }
        public IMongoCollection<FriendInvitation> FriendInvitations { get; private set; }
        public IMongoCollection<Network> Networks { get; private set; }
        public IMongoCollection<Role> Roles { get; private set; }
        public IMongoCollection<Resource> Resources { get; private set; }
        public IMongoCollection<BusinessPost> BusinessPosts { get; private set; }
        public IMongoCollection<JoiningBusinessInvitation> JoiningBusinessInvitations { get; private set; }
        public IMongoCollection<Notification> Notifications { get; private set; }
        public IMongoCollection<UserCreatedBusiness> UserCreatedBusinesses { get; private set; }
        public IMongoCollection<Country> Countries { get; private set; }
        public IMongoCollection<City> Cities { get; private set; }
        public IMongoCollection<Region> Regions { get; private set; }
        public IMongoCollection<VerifyToken> VerifyTokens { get; private set; }
        public IMongoCollection<Conversation> GroupChats { get; private set; }

        public GreenHouseMongoDatabase()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[GREENHOUSE_DB_CONNECTION_NAME].ConnectionString;
            var url = new MongoUrl(connectionString);
            var client = MongoHelper.GetMongoClient(url);
            Db = client.GetDatabase(url.DatabaseName);
            SocialPosts = Db.GetCollection<SocialPost>(SOCIAL_POST_COLLECTION);
            Accounts = Db.GetCollection<Account>(ACCOUNT_COLLECTION);
            SecurityQuestions = Db.GetCollection<SecurityQuestion>(SECURITY_QUESTION_COLLECTION);
            FriendInvitations = Db.GetCollection<FriendInvitation>(FRIEND_INVITATION_COLLECTION);
            Networks = Db.GetCollection<Network>(NETWORK_COLLECTION);
            Roles = Db.GetCollection<Role>(ROLE_COLLECTION);
            BusinessPosts = Db.GetCollection<BusinessPost>(DRAFPOST_COLLECTION);
            UserCreatedBusinesses = Db.GetCollection<UserCreatedBusiness>(UCB_COLLECTION);
            JoiningBusinessInvitations = Db.GetCollection<JoiningBusinessInvitation>(JOINING_BUSINESS_INVITATION_COLLECTION);
            Notifications = Db.GetCollection<Notification>(NOTIFICATION_COLLECTION);
            Countries = Db.GetCollection<Country>(COUNTRY);
            Cities = Db.GetCollection<City>(CITY);
            Regions = Db.GetCollection<Region>(REGION);
            VerifyTokens = Db.GetCollection<VerifyToken>(VERIFY_TOKEN_COLLECTION);
         
            GroupChats = Db.GetCollection<Conversation>(GROUP_CHAT);

        }

        public static void UpdateDatabase()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[GREENHOUSE_DB_CONNECTION_NAME].ConnectionString;
            var url = new MongoUrl(connectionString);
            var client = MongoHelper.GetMongoClient(url);
            var db = client.GetDatabase(url.DatabaseName);

            var allCollections = db.ListCollections().ToList().Select(c => c.GetElement("name").Value.ToString());

            var insertCollections = COLLECTIONS.Where(c => !allCollections.Contains(c));
            foreach (var collection in insertCollections)
            {
                db.CreateCollection(collection);
            }

            var dbContext = new GreenHouseMongoDatabase();
            var existRoles = dbContext.Roles.Find(new BsonDocument()).ToList();
            if (!existRoles.Any(r => r.Name == Role.ROLE_ADMIN))
            {
                dbContext.Roles.InsertOne(new Role { Name = Role.ROLE_ADMIN });
            }
            if (!existRoles.Any(r => r.Name == Role.ROLE_REVIEWER))
            {
                dbContext.Roles.InsertOne(new Role { Name = Role.ROLE_REVIEWER });
            }
            if (!existRoles.Any(r => r.Name == Role.ROLE_EDITOR))
            {
                dbContext.Roles.InsertOne(new Role { Name = Role.ROLE_EDITOR });
            }

            //Update location
            dbContext.Countries.DeleteMany(new BsonDocument());
            dbContext.Cities.DeleteMany(new BsonDocument());
            dbContext.Regions.DeleteMany(new BsonDocument());
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var countryFile = string.Empty;
            if (HttpContext.Current != null)
            {
                countryFile = HttpContext.Current.Server.MapPath("Datas\\Country.json");
            }
            else
            {
                countryFile = Path.Combine(appDir, @"..\..\..\GH.Web\Home\Datas\\Country.json");
            }
            var countries = new List<Country>();
            using (StreamReader r = new StreamReader(countryFile))
            {
                string json = r.ReadToEnd();
                List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(json);
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.name.ToString()))
                        continue;
                    countries.Add(new Country { Code = item.code, Code3 = item.code3, Name = item.name, NumCode = item.numcode, PhoneCode = item.phonecode });
                }
                dbContext.Countries.InsertMany(countries);
            }

            var cities = new List<City>();
            var cityFile = string.Empty;
            if (HttpContext.Current != null)
            {
                cityFile = HttpContext.Current.Server.MapPath("Datas\\City.json");
            }
            else
            {
                cityFile = Path.Combine(appDir, @"..\..\..\GH.Web\Home\Datas\\City.json");
            }
            using (StreamReader r = new StreamReader(cityFile))
            {
                string json = r.ReadToEnd();
                List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(json);
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.name.ToString()))
                        continue;
                    cities.Add(new City { CountryCode = item.countryCode, RegionCode = item.regionCode, Name = item.name, Latitude = item.latitude, Longitude = item.longitude });
                }
                dbContext.Cities.InsertMany(cities);
            }

            var regions = new List<Region>();
            var regionFile = string.Empty;
            if (HttpContext.Current != null)
            {
                regionFile = HttpContext.Current.Server.MapPath(@"..\..\..\GH.Web\Home\Datas\\Region.json");
            }
            else
            {
                regionFile = Path.Combine(appDir, @"Datas\\Region.json");
            }
            using (StreamReader r = new StreamReader(countryFile))
            {
                string json = r.ReadToEnd();
                List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(json);
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.name.ToString()))
                        continue;
                    string cityCount = item.cities != null ? item.cities.ToString() : "0";
                    regions.Add(new Region { CountryCode = item.countryCode, Code = item.code, Name = item.name, Latitude = item.latitude, Longitude = item.longitude, Cities = cityCount.ParseInt() });
                }
                dbContext.Regions.InsertMany(regions);
            }

            dbContext.Accounts.UpdateMany(a => a.PictureAlbum == null, Builders<Account>.Update.Set(a => a.PictureAlbum, new List<string>()));
            dbContext.Accounts.UpdateMany(new BsonDocument(), Builders<Account>.Update.Unset("CompanyDetails.Address"));
            dbContext.Accounts.UpdateMany(a => a.Followees == null, Builders<Account>.Update.Set(a => a.Followees, new List<Follow>()));
            dbContext.Accounts.UpdateMany(a => a.Followers == null, Builders<Account>.Update.Set(a => a.Followers, new List<Follow>()));
            dbContext.Accounts.UpdateMany(a => !a.BusinessAccountVerified, Builders<Account>.Update.Set(a => a.BusinessAccountVerified, false));
        }

    }
}