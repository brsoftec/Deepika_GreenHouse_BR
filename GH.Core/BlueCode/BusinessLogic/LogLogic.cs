using GH.Core.BlueCode.DataAccess;
using GH.Util;
using MongoDB.Bson;
using System;

namespace GH.Core.BlueCode
{
    public class LogLogic
    {
        public static void Insert(string businessUserId, string description, string module)
        {
            string billingid = BsonHelper.GenerateObjectIdString();
            try
            {
                var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Log");
                var subcription = new BsonDocument
            {
                {"_id",billingid},
                {"userId",businessUserId },
                {"description",description },
                {"DateEnd",DateTime.Now.AddDays(30)},
                {"module", module }
               
            };
                billingCollection.InsertOne(subcription);
            }
            catch (Exception ex) { }

        }
    }
}