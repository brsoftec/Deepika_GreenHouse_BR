

namespace GH.Web.Areas.User.ViewModels

{

    using System.ComponentModel.DataAnnotations;

    public class StripeChargeModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public double Amount { get; set; }

        public string CardHolderName { get; set; }
    }

    public class PaymentResult
    {
        public string ChargeId { get; set; }
        public string ChargeStatus { get; set; }
        public string ResponseJson { get; set; }

    }
}