using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Common;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GH.Core.Services;
using System.Configuration;
using GH.Core.Models;
using MongoDB.Bson.Serialization;
using System.Text.RegularExpressions;
using GH.Core.BlueCode.Entity.Payment;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class BillingLogic
    {
        public string InsertBilling(string businessUserId, string subcriptionid, string transactionid,
            string productname,
            string status, DateTime toStart, string price, string method = "", string promoCode = "", int numberMonthExpired = 1)
        {
            string billingid = BsonHelper.GenerateObjectIdString();
            try
            {
                var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
                var subcription = new BsonDocument
                {
                    {"_id", billingid},
                    {"userId", businessUserId},
                    {"DateStart", toStart},
                    {"DateEnd", toStart.AddMonths(numberMonthExpired)},
                    {"subcriptionid", subcriptionid},
                    {"productname", productname},
                    {"transactionid", transactionid},
                    {"status", status},
                    {"price", price},
                    {"isCurrent", "1"},
                    {"isPending", toStart > DateTime.Now},
                    {"methodpayment", method},
                    {"promocode", promoCode}
                };
                billingCollection.InsertOne(subcription);
            }
            catch (Exception ex) { }

            return subcriptionid;
        }

        public bool isUsePromocodeBus(string promocode, string userid)
        {
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("promocode", promocode);
            return billingCollection.Find(criteria).Count() > 0;
        }

        public long CountReUsePromoCode(string promocode)
        {
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var filter = Builders<BsonDocument>.Filter.Eq("promocode", promocode);
            return billingCollection.Find(filter).Count();
        }

        public void UpdateBillingCurrent(string billingId)
        {
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", billingId);
            var update = Builders<BsonDocument>.Update.Set("isCurrent", "0");
            billingCollection.UpdateOne(filter, update);
        }

        public void UpdateBillingPending(string billingId, decimal price, string method)
        {
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", billingId);
            var builder = Builders<BsonDocument>.Update;
            var update = builder.Set("isPending", false);
            billingCollection.UpdateOne(filter, update);
            update = builder.Set("amount", price.ToString());
            billingCollection.UpdateOne(filter, update);
            update = builder.Set("method", method);
            billingCollection.UpdateOne(filter, update);
        }

        public List<Billing> GetListBillingFromUserId(string userid)
        {
            var list = new List<Billing>();
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("userId", userid);// & builder.Eq("isCurrent", "1");
            var totalList = billingCollection.Find(filter).SortByDescending(x => x["DateStart"]).ToEnumerable();
            //var subcription= new SubcriptionLogic().GetSubcriptionByUserId(userid);
            //var paymentplan = new SubcriptionLogic().GetPlanFromName(subcription["subcription"]["CurrentPlan"].AsString);
            foreach (var item in totalList)
            {
                list.Add(new Billing
                {
                    Id = item["_id"].AsString,
                    datestart = item["DateStart"].ToLocalTime(),
                    dateend = item["DateEnd"].ToLocalTime(),
                    productname = item["productname"].AsString,
                    transactionid = item["transactionid"].AsString,
                    status = item["status"].AsString,
                    amount = item["price"].AsString,
                    methodpayment = BsonHelper.GetvaluestringFromObjectOnelevel(item, "methodpayment"),
                    promocode = BsonHelper.GetvaluestringFromObjectOnelevel(item, "promocode"),
                    isCurrent = BsonHelper.GetvaluestringFromObjectOnelevel(item, "isCurrent") == "1",
                    isPending = BsonHelper.GetvaluestringFromObjectOnelevel(item, "isPending") == "true",
//                    isPending = item.GetValue("isPending",false).AsBoolean
                });
            }

            return list;
        }

        public Billing GetCurrentBilling(string userid)
        {
            Billing currentbill = null;
            var list = this.GetListBillingFromUserId(userid);
            if (list != null && list.Count > 0)
            {
                currentbill = list.FirstOrDefault();
            }

            return currentbill;
        }

        public List<Billing> GetListBillingByEndDate()
        {
            var list = new List<Billing>();
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var filter = Builders<BsonDocument>.Filter;


            var totalList = billingCollection.Find(
                filter.Eq("isCurrent", "1") &
                 (filter.Lt("DateEnd", DateTime.UtcNow.AddDays(3))
                  & filter.Gt("DateEnd", DateTime.UtcNow.AddDays(-2)) |
                  filter.Lt("DateEnd", DateTime.UtcNow.AddDays(11))
                  & filter.Gt("DateEnd", DateTime.UtcNow.AddDays(9))))
                .SortByDescending(x => x["DateEnd"]).ToEnumerable();
            foreach (var item in totalList)
            {
                try
                {
                    list.Add(new Billing
                    {
                        UserId = item["userId"].AsString,
                        Id = item["_id"].AsString,
                        datestart = item["DateStart"].ToLocalTime(),
                        dateend = item["DateEnd"].ToLocalTime(),
                        productname = item["productname"].AsString,
                        transactionid = item["transactionid"].AsString,
                        status = item["status"].AsString,
                        amount = item["price"].AsString,
                        methodpayment = BsonHelper.GetvaluestringFromObjectOnelevel(item, "methodpayment"),
                        promocode = BsonHelper.GetvaluestringFromObjectOnelevel(item, "promocode")
                    });
                }
                catch
                {
                    
                }
            }

            return list;
        }
    }
}