using Core.Common;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.Services;
using GH.Core.TaskServices;
using MongoDB.Bson;
using MongoDB.Driver;
using RegitSocial.Business.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
//using MongoDB.Bson.IO;
using WizLocal.BL.Core;
using Stripe;
using GH.Util;

namespace GH.Core.BlueCode.BusinessLogic.Payment
{
    public class SubcriptionTask : ASubTask
    {
        public SubcriptionTask(string nameMinutesToSleep) : base(nameMinutesToSleep)
        {
        }

        public override void ExecuteMethod()
        {
            try
            {
                var subcriptionLogic = new SubcriptionLogic();
                var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
                var criteria = Builders<BsonDocument>.Filter.Eq("IsFirstPay", "1");
                criteria = criteria & Builders<BsonDocument>.Filter.Ne("subcription.CurrentPlan", "free");
                var subcriptions = subcriptionCollection.Find(criteria).ToEnumerable();
                var paymentCardLogic = new PaymentCardLogic();
                var ccService = new CreditCardLogic(PayPalConfig.GetAPIContext());
                foreach (var item in subcriptions)
                {
                    var planname = item["subcription"]["CurrentPlan"].AsString;
                    var subscriptionid = item["_id"].AsString;
                    var plan = subcriptionLogic.GetPlanFromName(planname);
                    var userid = item["userId"].AsString;
                    var strdescriptionplan = plan.PlanName == "medium" ? "Medium Plan" : "Heavy Plan";
                    var user = new AccountService().GetByAccountId(userid);
                    var paymentCard = paymentCardLogic.GetPaymentCardDefaultByUserId(userid);
                    if (paymentCard != null && plan != null && user != null && plan.Price > 0)
                    {
                      
                        strdescriptionplan += " Monthly Subscription";
                      
                        StripeCharge stripeCharge = new StripePaymentLogic().ProcessPaymentCreditCard("usd", plan.Price, strdescriptionplan, paymentCard);

                        //    If charge succeeded
                        if (stripeCharge!=null && stripeCharge.Status == "succeeded")
                        {

                            var method = paymentCard.cardtype + "**" + paymentCard.cardnumber.Substring(Math.Max(0, paymentCard.cardnumber.Length - 4));

                            var transactionid = stripeCharge.Id;
                            new SubcriptionLogic().UpdateSubcriptionFirstTime(subscriptionid);
                            new BillingLogic().InsertBilling(user.AccountId, subscriptionid, transactionid,
                                strdescriptionplan, "Paid", DateTime.Now, plan.Price.ToString(), method);


                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyBillingFirst;
                            notificationMessage.ToAccountId = user.AccountId;
                            notificationMessage.ToUserDisplayName = user.Profile.DisplayName;
                            // notificationMessage.Content = delegationMessage.Message;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);

                        }
                        else 
                        {
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyBillingFailed;
                            notificationMessage.ToAccountId = user.AccountId;
                            notificationMessage.ToUserDisplayName = user.Profile.DisplayName;
                            // notificationMessage.Content = delegationMessage.Message;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                        }
                    }
                    else
                    {
                        // send notifycation
                        // send mail
                    }
                }
            }
            catch (Exception ex)
            {
                LogLogic.Insert("", ex.ToString(), "SubcriptionTask");
            }
        }
    }
}