using System.Net;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("Api/Delegation")]
    public class DelegationApiController : BaseApiController
    {
        private static readonly IDelegationBusinessLogic DelegationService = new DelegationBusinessLogic();
        private IAccountService _accountService;

        public DelegationApiController()
        {
        }
        
        [HttpGet, Route("List/In")]
        public HttpResponseMessage GetDelegations()
        {
            var result = DelegationService.GetUserDelegations(Account,"DelegationIn");
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound":
                        return Request.CreateSuccessResponse(new object[]{}, "No inbound delegation found");
                    default:
                        return Request.CreateApiErrorResponse("Error getting delegations", HttpStatusCode.InternalServerError);
                }
            }
            
            return Request.CreateSuccessResponse(result.Data, "List inbound delegations");
        }         
        [HttpGet, Route("Details/{delegationId}")]
        public HttpResponseMessage GetDelegation(string delegationId)
        {
            var result = DelegationService.GetUserDelegation(delegationId, Account);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "missing.id":
                        return Request.CreateApiErrorResponse("Missing delegation ID");    
                    case "notFound":
                        return Request.CreateApiErrorResponse("Delegation not found");   
                    default:
                        return Request.CreateApiErrorResponse("Error getting delegation");
                }
            }
            
            return Request.CreateSuccessResponse(result.Data, "Delegation details");
        }        
        
        [HttpPost, Route("Accept")]
        public HttpResponseMessage AcceptDelegation(string delegationId)
        {
            var result = DelegationService.AcceptDelegation(AccountId, delegationId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "delegation.notFound":
                        return Request.CreateApiErrorResponse("Delegation not found");  
                    default:
                        return Request.CreateApiErrorResponse("Error accepting delegation");
                }
            }
            
            return Request.CreateSuccessResponse(new
            {
                delegationId = delegationId,
                status = "accepted"
            }, "Delegation accepted");
        }       
        
        [HttpPost, Route("Deny")]
        public HttpResponseMessage DenyDelegation(string delegationId)
        {
            var result = DelegationService.DenyDelegation(AccountId, delegationId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "delegation.notFound":
                        return Request.CreateApiErrorResponse("Delegation not found");  
                    default:
                        return Request.CreateApiErrorResponse("Error denying delegation");
                }
            }
            
            return Request.CreateSuccessResponse(new
            {
                delegationId = delegationId,
                status = "removed"
            }, "Delegation denied");
        }

    }
}