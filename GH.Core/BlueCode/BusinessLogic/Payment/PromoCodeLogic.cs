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
    public class PromoCodeLogic
    {
        public string InsertPromoCode(string promocode,string type,string numberReUse,string numberMonthExpired)
        {
            string subcriptionid = BsonHelper.GenerateObjectIdString();
            try
            {
                var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PromoCode");
                var subcription = new BsonDocument
            {
                {"Code",promocode },
                {"IsActive","1" },
                {"IsUse","0"},
                {"Type",type},
                {"NumberReUse",numberReUse },
                {"NumberMonthExpired",numberMonthExpired }

            };
                subcriptionCollection.InsertOne(subcription);
            }
            catch (Exception ex) { }

            return subcriptionid;
        }

        public PromoCode GetPromoCodeByCode(string code)
        {
            BsonDocument pc = null;
            var promoCodeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PromoCode");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("Code", code);
            criteria= criteria & filter.Eq("IsActive", "1");
            pc = promoCodeCollection.Find(criteria).FirstOrDefault();
            

            if(pc!=null)
            return new PromoCode {
                IsActive= pc["IsActive"].ToBoolean(),
                Code = pc["Code"].ToString(),
                Type = pc["Type"].ToString(),
                IsUse = pc["IsUse"].ToBoolean(),
                NumberMonthExpired= ConvertHelper.ConvertToInt( BsonHelper.GetvaluestringFromObjectOnelevel(pc, "NumberMonthExpired")),
                NumberReUse = ConvertHelper.ConvertToInt(BsonHelper.GetvaluestringFromObjectOnelevel(pc, "NumberReUse"))
            };
            else
                return null;
        }    
        public void UpdatePromocode(string code)
        {
            var promoCodeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PromoCode");
            var filter = Builders<BsonDocument>.Filter.Eq("Code", code);
            var update = Builders<BsonDocument>.Update.Set("IsUse", "1");
            promoCodeCollection.UpdateOne(filter, update);
        }
       
    }

}
