using GH.Web.Areas.User.ViewModels;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("api/billing")]
    public class BillingApiController : BaseApiController
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [HttpPost, Route("PaymentMethod/SetDefault/{methodId}")]
        public async Task<HttpResponseMessage> SetDefaultPaymentMethodAsync(string methodId)
        {
            if (string.IsNullOrEmpty(methodId)) 
                return Request.CreateApiErrorResponse("Missing method ID", HttpStatusCode.BadRequest);
            
            var result = await new PaymentCardLogic().SetDefaultPaymentCardAsync(methodId);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error updating payment method",
                    error: "payment.method.update.error");

            return Request.CreateSuccessResponse(new 
            {
                methodId,
                isDefault = true
            }, "Payment method set as default");
        }
        
        [HttpPost, Route("Subscription/PromoCode/Add")]
        public async Task<HttpResponseMessage> AddPromoCodeAsync(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
                return Request.CreateApiErrorResponse("Missing promo code", HttpStatusCode.BadRequest);
            
            var result = await new SubcriptionLogic().AddPromoCodeAsync(AccountId, promoCode);
            if (!result.Success)
            {
                if (result.Status == "promocode.invalid")
                    return Request.CreateApiErrorResponse("Invalid promo code",
                        error: "promocode.invalid");
                return Request.CreateApiErrorResponse("Error adding promo code",
                    error: "subscription.update.error");
            }

            return Request.CreateSuccessResponse(new 
            {
                subscriptionId = result.Data,
                pendingPromoCode = promoCode
            }, "Promo code added to subscription");
        }
    }
}