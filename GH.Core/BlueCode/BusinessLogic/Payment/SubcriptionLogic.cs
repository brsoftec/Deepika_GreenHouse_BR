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
    public class SubcriptionLogic
    {
        private CacheContainer cacheContainer = CacheManager.CreateCacheContainer(typeof(SubcriptionLogic).Name);
        public BsonDocument GetSubcriptionTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "Subscription");
            var paymentPlanTemplate = settingCollection.Find(filter).FirstOrDefault();
            var data = paymentPlanTemplate["value"].AsBsonDocument;
            return data.ToBsonDocument();
        }
        public BsonDocument GetSubcriptionPlanTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "PaymentPlan");
            var paymentPlanTemplate = settingCollection.Find(filter).FirstOrDefault();
            var data = paymentPlanTemplate["value"].AsBsonDocument;
            return data.ToBsonDocument();
        }
        public PaymentPlanDetail GetPlanFromName(string name)
        {
            List<PaymentPlanDetail> paymentPlanDetails = cacheContainer.Get<List<PaymentPlanDetail>>(Util.Common.PaymentPlanKey);
            if (paymentPlanDetails == null)
            {
                paymentPlanDetails = new List<PaymentPlanDetail>();
                var data = GetSubcriptionPlanTemplate()["Data"].AsBsonArray;
                foreach (var item in data)
                {
                    paymentPlanDetails.Add(new PaymentPlanDetail
                    {
                        PlanName = item["name"].AsString,
                        Price = ConvertHelper.ConvertToInt(item["price"].AsString),
                        InteractionActives = item["quota"]["communication"].AsString,
                        SyncRelationships = item["quota"]["syncRelationships"].AsString,
                        BusinessUsers = item["quota"]["businessUsers"].AsString
                    });
                }
                cacheContainer.Add(Util.Common.PaymentPlanKey, paymentPlanDetails, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }

            if (paymentPlanDetails != null)
            {
                return paymentPlanDetails.Where(x => x.PlanName == name).FirstOrDefault();
            }

            return null;
        }
        public string InsertSubcription(string businessUserId)
        {
            string subcriptionid = BsonHelper.GenerateObjectIdString();
            try
            {
                var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
                var subcription = new BsonDocument
            {
                {"_id",subcriptionid},
                {"userId",businessUserId },
                {"DateStartTrial",DateTime.Now.ToString() },
                {"Upgrade",""},
                {"subcription", GetSubcriptionTemplate() },
                {"IsFirstPay", "1" },

            };
                subcriptionCollection.InsertOne(subcription);
            }
            catch (Exception ex) { }

            return subcriptionid;
        }
        public void UpdateSubcriptionFirstTime(string subcriptionId)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", subcriptionId);
            var update = Builders<BsonDocument>.Update.Set("IsFirstPay", "0");
            subcriptionCollection.UpdateOne(filter, update);
        }       
        public async Task<FuncResult> AddPromoCodeAsync(string accountId, string promoCode)
        {
            var promoCodeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PromoCode");
            var filter = Builders<BsonDocument>.Filter;
            var code = await promoCodeCollection.Find(filter.Eq("Code", promoCode) & filter.Eq("IsActive", "1"))
                .FirstOrDefaultAsync();
            if (code == null)
                return new ErrResult("promocode.invalid");
            var subscriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filterSubscription = Builders<BsonDocument>.Filter.Eq("userId", accountId);
            var subscription = await subscriptionCollection.Find(filterSubscription).FirstOrDefaultAsync();
            if (code["Type"].AsString != subscription["subcription"]["CurrentPlan"].AsString)
                return new ErrResult("promocode.invalid");
            
            var update = Builders<BsonDocument>.Update.Set("PendingPromoCode", promoCode);
            await subscriptionCollection.UpdateOneAsync(filterSubscription, update);
            return new OkResult("subscription.promocode.add.ok", subscription["_id"]);
        }
        public void UpgradeSubcription(string subcriptionId, string planname)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", subcriptionId);
            var update = Builders<BsonDocument>.Update.Set("subcription.CurrentPlan", planname);
            update = update.Set("Upgrade", DateTime.Now.ToString());
            subcriptionCollection.UpdateOne(filter, update);

        }
        public void UpgradeSubcriptionbyUserId(string userId, string planname)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var update = Builders<BsonDocument>.Update.Set("subcription.CurrentPlan", planname);
            update = update.Set("Upgrade", DateTime.Now.ToString());
            subcriptionCollection.UpdateOne(filter, update);

        }
        public void UpgradeSubcriptionWithPromoCodebyUserId(string userId, string planname)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var update = Builders<BsonDocument>.Update.Set("subcription.CurrentPlan", planname);
            update = update.Set("Upgrade", DateTime.Now.ToString());
            update = update.Set("IsFirstPay", "1");
            subcriptionCollection.UpdateOne(filter, update);

        }
        public void SetFreeSubcriptionByAccountId(string userId)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var update = Builders<BsonDocument>.Update.Set("subcription.CurrentPlan", "free");
            update = update.Set("Upgrade", DateTime.Now.ToString());
            update = update.Set("IsFirstPay", "1");
            subcriptionCollection.UpdateOne(filter, update);

        }
        //SetFreeSubcriptionByAccountId
        public BsonDocument GetSubcriptionById(string subcriptionId)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", subcriptionId);
            var subcription = subcriptionCollection.Find(criteria).FirstOrDefault();
            return subcription;
        }
        public BsonDocument GetSubcriptionByUserId(string userid)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var criteria = Builders<BsonDocument>.Filter.Eq("userId", userid);
            var subcription = subcriptionCollection.Find(criteria).FirstOrDefault();
            return subcription;
        }

        // not finished yet
        //public bool CheckPlan(string userid, string id)
        //{
        //    var rs = false;
        //    var _accountService = new AccountService();
        //    var currentUser = _accountService.GetByAccountId(userid);

        //   // var paymentViewModel = new PaymentViewModel();
        //   // PaymentPlanDetail plan = null;
        //    paymentViewModel.PaymentPlanDetailInteraction.currentnumber =
        //        new CampaignBusinessLogic().CountInteractionsUserId(userid);
        //    paymentViewModel.PaymentPlanDetailSyncForm.currentnumber =
        //        new PostHandShakeBusinessLogic().CountUserInvitedHandshake(userid);
        //    paymentViewModel.PaymentPlanDetailWorkFlow.currentnumber =
        //        _accountService.CountMembersInBusiness(currentUser.Id);

        //    return rs;
        //}
       
    }

}
