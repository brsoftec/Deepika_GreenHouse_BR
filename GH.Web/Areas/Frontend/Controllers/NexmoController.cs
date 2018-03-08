using GH.Core.Exceptions;
using Nexmo.Api;
using System.Web.Http;

namespace GH.Web.Areas.Frontend.Controllers
{
    [RoutePrefix("api/nexmo")]
    public class NexmoController : ApiController
    {
        [HttpGet, Route("number/verify")]
        public IHttpActionResult NumberVerify(string phoneNumber)
        {
            var res = Nexmo.Api.NumberVerify.Verify(new NumberVerify.VerifyRequest
            {
                number = phoneNumber,
                brand = "Regit Verify",
            }, Global.NexmoCredentials);
            if (!string.IsNullOrEmpty(res.error_text))
                throw new CustomException(res.error_text);
            return Json(new { Error = false, RequestId = res.request_id });
        }

        [HttpGet, Route("number/check")]
        public IHttpActionResult PINVerify(string requestId, string PinCode)
        {
            var res = Nexmo.Api.NumberVerify.Check(new NumberVerify.CheckRequest { request_id = requestId, code = PinCode },
                Global.NexmoCredentials);
            if (!string.IsNullOrEmpty(res.error_text))
                throw new CustomException(res.error_text);
            return Ok();
        }
    }
}