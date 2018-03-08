using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Payment;

namespace GH.Web.Areas.User.ViewModels
{
    public class SubcriptionViewModel : TransactionalInformation
    {
        public SubcriptionViewModel()
        {
        }


        public string PromoCode { set; get; }
        public string PromoCodeType { set; get; }

        public string NumberReUse { set; get; }

        public string NumberMonthExpired { set; get; }
        public bool isAdmin { set; get; }
        public string UserId { set; get; }
    
        public string PaymentPlanName { set; get; }
        public string SubcriptionId { set; get; }
        public object SubcriptionTemplate { set; get; }

        public List<PaymentCard> ListPaymentCard { set; get; }

        public List<Billing> ListBilling { set; get; }

        public string PaymentCardId { set; get; }

        public object Subcription { set; get; }

        public object SubcriptionPlans { set; get; }

        public PaymentCard PaymentCard { set; get; }
        //Billing Account Information
        public string AccountId { set; get; }
        public string AccountName { set; get; }

        public string AccountAddress { set; get; }

        public string AccountPlan { set; get; }
        //Payment Card Detail

        public string CardName { set; get; }

        public string CardNumber { set; get; }

        public string CardExpiredMonth { set; get; }

        public string CardExpiredYear { set; get; }

        public string CardSecurity { set; get; }

        public string CardType { set; get; }

        public bool IsDefault { set; get; }



    }
}