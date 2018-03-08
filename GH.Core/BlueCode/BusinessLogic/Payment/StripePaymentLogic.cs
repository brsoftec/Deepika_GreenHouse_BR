using GH.Core.BlueCode.Entity.Payment;
using GH.Util;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic.Payment
{
    public class StripePaymentLogic
    {
        private string KeyAPI="";
        public StripePaymentLogic()
        {
            var isliveStripe = ConfigHelp.GetBoolValue("IsStripeLive");
            string KeyAPI = ConfigHelp.GetStringValue("KeyStripeTest");
            if (isliveStripe)
            {
                KeyAPI = ConfigHelp.GetStringValue("KeyStripeLive");
            }
        }

        public StripeCharge ProcessPaymentCreditCard(string currency, decimal price, string description, PaymentCard paymentCard)
        {

            if (price <= 0)
            {
                return null;
            }

            var myCharge = new StripeChargeCreateOptions();

            // always set these properties
            var _price = price * 100;
            myCharge.Amount = (int)_price;
            myCharge.Currency = currency;

            // set this if you want to
           // strdescriptionplan += " Monthly Subscription";
            myCharge.Description = description;
            myCharge.SourceCard = new SourceCard()
            {
                Number = paymentCard.cardnumber,
                ExpirationYear = paymentCard.expiredyear,
                ExpirationMonth = paymentCard.expiredmonth,
                Name = paymentCard.cardname,
                Cvc = paymentCard.cardsecuritycode
            };
         
            // set this property if using a customer
            //myCharge.CustomerId = *customerId*;
            var isliveStripe = ConfigHelp.GetBoolValue("IsStripeLive");
            string keyStripe = ConfigHelp.GetStringValue("KeyStripeTest");
            if (isliveStripe)
            {
                keyStripe = ConfigHelp.GetStringValue("KeyStripeLive");
            }
      
            //Test
            
            //myCharge.SourceCard.Cvc = "123";
            //myCharge.SourceCard.Number = "4242424242424242";
            //myCharge.SourceCard.ExpirationMonth = "12";
            //myCharge.SourceCard.ExpirationYear = "2018";
            //keyStripe = "sk_test_rD6vmp6ZtD1d2iG8bldC8hh4";

            // End Test
            var chargeService = new StripeChargeService(keyStripe);
            // var chargeService = new StripeChargeService("sk_live_tSXLa24J051N2kHX8MQr3gF5");
            StripeCharge stripeCharge = chargeService.Create(myCharge);
            return stripeCharge;
        }
    }
}