using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Payment;

namespace GH.Web.Areas.User.ViewModels
{
    public class PaymentViewModel : TransactionalInformation
    {
        public PaymentViewModel()
        {
            PaymentPlanDetailInteraction = new PaymentEstimationDetail();
            PaymentPlanDetailWorkFlow = new PaymentEstimationDetail();
            PaymentPlanDetailSyncForm = new PaymentEstimationDetail();
        }
        public string UserId { set; get; }
    
        public string PaymentPlanName { set; get; }

        public string PaymentPlanDesc { set; get; }

        public PaymentEstimationDetail PaymentPlanDetailInteraction { set; get; }

        public PaymentEstimationDetail PaymentPlanDetailWorkFlow { set; get; }

        public PaymentEstimationDetail PaymentPlanDetailSyncForm { set; get; }
    }

    public class PaymentEstimationDetail {
        public long currentnumber { set; get; }

        public long currentmaxnumber { set; get; }

        public bool IsOver { get { return currentmaxnumber <= currentnumber; } }

    }

}