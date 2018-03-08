using GH.Core.BlueCode.DataAccess;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.BlueCode.Entity.Payment;
using GH.Core.Models;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class PaymentCardLogic
    {
        public string InsertPaymentCard(string businessUserId, string name, string cardtype,
            string cardnumber, string expiredmonth, string cardexpiredyear, bool isdefault, string cardsecuritycode, string idcreditcard)
        {
            string paymentcardid = BsonHelper.GenerateObjectIdString();
            try
            {
                var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");
                if (isdefault)
                {
                    var filter1 = Builders<BsonDocument>.Filter.Eq("isdefault", "1");
                    var update1 = Builders<BsonDocument>.Update.Set("isdefault", "0");
                    subcriptionCollection.UpdateOne(filter1, update1);
                }
                var subcription = new BsonDocument
            {
                {"_id",paymentcardid},
                {"userId",businessUserId },
                {"cardtype",cardtype },
                {"name",name},
                {"cardnumber",cardnumber},
                {"expiredmonth",expiredmonth},
                {"expiredyear",cardexpiredyear},
                {"isdefault",isdefault?"1":"0"},
                {"cardsecuritycode",cardsecuritycode},
                {"idcreditcard",idcreditcard},

            };
                subcriptionCollection.InsertOne(subcription);
            }
            catch (Exception ex) {


            }

            return paymentcardid;
        }

        public void UpdatePaymentCard(string paymentcardid,string name, string cardtype,
            string cardnumber, string expiredmonth, string cardexpiredyear, bool isdefault, string cardsecuritycode)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");
            if (isdefault)
            {
                var filter1 = Builders<BsonDocument>.Filter.Eq("isdefault", "1");
                var update1 = Builders<BsonDocument>.Update.Set("isdefault", "0");
                subcriptionCollection.UpdateOne(filter1, update1);
            }
            var filter = Builders<BsonDocument>.Filter.Eq("_id", paymentcardid);
            var update = Builders<BsonDocument>.Update.Set("isdefault", isdefault?"1":"0");
            subcriptionCollection.UpdateOne(filter, update);
           
        }
        public async Task<FuncResult> SetDefaultPaymentCardAsync(string paymentcardid)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", paymentcardid);
            var update = Builders<BsonDocument>.Update.Set("isdefault", "1");
            var result = await subcriptionCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount==0)
                return new ErrResult("method.update.not");
            update = Builders<BsonDocument>.Update.Set("isdefault", "0");
            await subcriptionCollection.UpdateManyAsync(!filter, update);
            return new OkResult("method.update.ok");
          
        }

        public List<PaymentCard> GetListPaymentCard(string userid)
        {
            var paymentCardCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");

            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            var totalList = paymentCardCollection.Find(criteria).ToEnumerable();

            var list = new List<PaymentCard>();
            foreach (var paymentcarditem in totalList)
            {
                PaymentCard paymentCard = new PaymentCard();
                paymentCard.cardnumber = paymentcarditem["cardnumber"].AsString;
                paymentCard.Id = paymentcarditem["_id"].AsString;
                paymentCard.cardtype = paymentcarditem["cardtype"].AsString;
                paymentCard.cardname = paymentcarditem["name"].AsString;
                paymentCard.expiredmonth = paymentcarditem["expiredmonth"].AsString;
                paymentCard.expiredyear = paymentcarditem["expiredyear"].AsString;
                paymentCard.isdefault = paymentcarditem["isdefault"].AsString == "1" ? true : false;
                paymentCard.cardsecuritycode = paymentcarditem["cardsecuritycode"].AsString;
                paymentCard.idcreditcard = paymentcarditem["idcreditcard"].AsString;
                list.Add(paymentCard);
            }
            return list;
        }
        public void DeletePaymentCard(string paymentcardid)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", paymentcardid);
            campaignCollection.FindOneAndDelete(filter);
        }

        public PaymentCard GetPaymentCardDefaultByUserId(string userid)
        {
            var list = GetListPaymentCard(userid);
            if (list.Count == 0) return null;
            var card = list.FirstOrDefault(x => x.isdefault);
            if (card == null) card = list[0];
            return card;
        }
        public PaymentCard GetPaymentCardById(string paymentcardid)
        {
            var paymentCardCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PaymentCard");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("_id", paymentcardid);
            var totalList = paymentCardCollection.Find(criteria).ToEnumerable();
            var list = new List<PaymentCard>();
            foreach (var paymentcarditem in totalList)
            {
                PaymentCard paymentCard = new PaymentCard();
                paymentCard.cardnumber = paymentcarditem["cardnumber"].AsString;
                paymentCard.Id = paymentcarditem["_id"].AsString;
                paymentCard.cardtype = paymentcarditem["cardtype"].AsString;
                paymentCard.cardname = paymentcarditem["name"].AsString;
                paymentCard.expiredmonth = paymentcarditem["expiredmonth"].AsString;
                paymentCard.expiredyear = paymentcarditem["expiredyear"].AsString;
                paymentCard.isdefault = paymentcarditem["isdefault"].AsString == "1" ? true : false;
                paymentCard.cardsecuritycode = paymentcarditem["cardsecuritycode"].AsString;
                paymentCard.idcreditcard = paymentcarditem["idcreditcard"].AsString;
                list.Add(paymentCard);
            }
            return list.FirstOrDefault();
        }
    }

}
