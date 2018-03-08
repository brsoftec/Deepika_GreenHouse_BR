using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using GH.Core.Models;
using GH.Core.Services;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Fluent;

namespace GH.Web.Areas.User.Controllers
{
    public class ApiAccount
    {
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
    }

    public class BaseApiController : ApiController
    {
        public bool Authenticated;
        public string UserId;
        public string AccountId;
        public Account Account;
        public Account BusinessAccount;
        public bool IsBusiness;
        public bool IsBusinessMaster;
        public bool IsBusinessMember;
        public bool AsBusiness;

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public BaseApiController()
        {
        }

        protected void CheckAuthenticated()
        {
            if (!Authenticated)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Not authenticated"));
            }
        }

        protected void CheckBusiness()
        {
            CheckAuthenticated();
            if (!AsBusiness)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.NotFound, "No business associated with user"));
            }
        }
    }
    
            
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AuthErrorResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
        
    public class ApiAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse<AuthErrorResult>
                (new AuthErrorResult() { Success = false, Message = "User not authorized", Error = "unauthorized" });
            response.StatusCode = HttpStatusCode.Unauthorized;
            actionContext.Response = response;
        }
    }

    public class BaseApi : ActionFilterAttribute

    {
        private static readonly IAccountService AccountService = new AccountService();
        private static readonly IResourceService ResourceService = new ResourceService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<Resource> Resources;
        private Dictionary<string, Resource> ResourcesPerPath;

        private string _userId;
        private string _accountId;
        private Account _account;
        private Account _businessAccount;
        private bool _authenticated;
        private bool _isBusiness;
        private bool _isBusinessMember;
        private bool _asBusiness;

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            _authenticated = HttpContext.Current.User.Identity.IsAuthenticated;
            if (!_authenticated)
            {
                _userId = _accountId = null;
                _account = null;
            }
            else
            {
                _accountId = HttpContext.Current.User.Identity.GetUserId();
                _account = AccountService.GetByAccountId(_accountId);
                if (_account == null)
                {
                    throw new HttpResponseException(
                        filterContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Not authenticated"));
                }
                _userId = _account.Id.ToString();
            }

            BaseApiController controller = filterContext.ControllerContext.Controller as BaseApiController;
            if (controller != null)
            {
                controller.Authenticated = _authenticated;
                controller.UserId = _userId;
                controller.AccountId = _accountId;
                controller.Account = _account;
            }
            else
            {
                filterContext.Request.Properties["userId"] = _userId;
                filterContext.Request.Properties["accountId"] = _accountId;
                filterContext.Request.Properties["account"] = _account;
            }

            if (!_authenticated)
            {
                return;
            }

            if (_account != null)
            {

                _isBusiness = _account.AccountType == AccountType.Business;

                if (!_isBusiness)
                {
                    var linkedAccounts = AccountService.GetBusinessAccountsLinkWithPersonalAccount(_account.Id);
                    _businessAccount = linkedAccounts.FirstOrDefault(x => x.EmailVerified);
                    if (_businessAccount != null)
                    {
                        _isBusinessMember = true;
                    }
                }
                else
                {
                    _businessAccount = _account;
                }

                if (HttpContext.Current.Session["IsBusinessView"] != null)
                {
                    _asBusiness = (bool) HttpContext.Current.Session["IsBusinessView"];
                }
                if (controller != null)
                {
                    controller.IsBusinessMember = _isBusinessMember;
                    controller.IsBusiness = _isBusiness;
                    controller.BusinessAccount = _businessAccount;
                    controller.AsBusiness = _asBusiness;
                }
            }
            else
            {
                Log.Debug($"Account check error: {_isBusiness}, {_isBusinessMember}");
            }
        }
    }
}