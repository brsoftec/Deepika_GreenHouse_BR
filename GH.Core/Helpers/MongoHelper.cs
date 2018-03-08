using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Web;

namespace GH.Core.Helpers
{
    public class MongoHelper
    {   
        public static MongoClient GetMongoClient(MongoUrl mongoUrl)
        {
            //var connectionString = ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString;
            //MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            //settings.SslSettings =
            //  new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            //var client = new MongoClient(settings);
            var client = new MongoClient(mongoUrl);
            return client;
        }

    }
}