using Core.Common;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.Services;
using GH.Core.TaskServices;
using MongoDB.Bson;
using MongoDB.Driver;
using RegitSocial.Business.Notification;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WizLocal.BL.Core;

namespace GH.Core.BlueCode.BusinessLogic.Payment
{
    public class BillingTask : ASubTask
    {
        public BillingTask(string nameMinutesToSleep) : base(nameMinutesToSleep) {

        }
        public override  void ExecuteMethod() {
            var subcriptionLogic = new SubcriptionLogic();
            var billingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Billing");
            var builder = Builders<BsonDocument>.Filter;
            var criteria = builder.Eq("isCurrent", "1") & 
                           (builder.Lte("DateEnd", DateTime.Now)
                            | builder.Eq("isPending", true) & builder.Lte("DateStart", DateTime.Now));
            var billings = billingCollection.Find(criteria).ToEnumerable();
            var paymentCardLogic = new PaymentCardLogic();
            var ccService = new CreditCardLogic(PayPalConfig.GetAPIContext());
            foreach (var item in billings)
            {
                var subcription = subcriptionLogic.GetSubcriptionById(item["subcriptionid"].ToString());
                var planname = subcription["subcription"]["CurrentPlan"].AsString;
                var subscriptionid = subcription["_id"].AsString;
                var plan = subcriptionLogic.GetPlanFromName(planname);
                var userid = item["userId"].AsString;
                var planName = plan.PlanName.First().ToString().ToUpper() + plan.PlanName.Substring(1);
                var strdescriptionplan = planName + " Plan Monthly Subscription";
                var user = new AccountService().GetByAccountId(userid);
                var paymentCard = paymentCardLogic.GetPaymentCardDefaultByUserId(userid);

                if (paymentCard != null && plan != null && user != null)
                {
                  
                    StripeCharge stripeCharge = new StripePaymentLogic().ProcessPaymentCreditCard("usd", plan.Price, strdescriptionplan, paymentCard);
                    if (stripeCharge != null && stripeCharge.Status == "succeeded")
                    {
                        var method = paymentCard.cardtype + "**" + paymentCard.cardnumber.Substring(Math.Max(0, paymentCard.cardnumber.Length - 4));

                        var transactionid = stripeCharge.Id;
                        if (item.GetValue("isPending",false).AsBoolean)
                            new BillingLogic().UpdateBillingPending(item["_id"].AsString, plan.Price, method);
                        else
                        {
                            new BillingLogic().UpdateBillingCurrent(item["_id"].AsString);
                            new BillingLogic().InsertBilling(user.AccountId, subscriptionid, transactionid,
                                strdescriptionplan,
                                "Paid", DateTime.Now, plan.Price.ToString(), method);
                        }

                        //send notifycation
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = EnumNotificationType.NotifyBillingRenew;
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
    }
}