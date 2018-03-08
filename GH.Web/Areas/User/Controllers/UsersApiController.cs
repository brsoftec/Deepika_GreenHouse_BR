using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Threading.Tasks;
using System.Web.Http;
using GH.Core.Models;
using MongoDB.Bson;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("Api/Users")]
    public class UsersApiController : BaseApiController
    {
        static readonly IAccountService AccountService = new AccountService();
        static readonly IUserService UserService = new UserService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public UsersApiController()
        {
        }

        [HttpGet, Route("Search")]
        public async Task<List<UserViewModel>> SearchUsers(string keyword, string by = "name", int? start = null,
            int? length = null)
        {
            var users = UserService.SearchUsers(keyword, by, start, length);

            return users.Select(u => new UserViewModel
            {
                Id = u.Id.ToString(),
                AccountId = u.AccountId,
                Avatar = u.Profile.PhotoUrl,
                DisplayName = u.Profile.DisplayName,
                Email = u.Profile.Email
            }).ToList();
        }
        [HttpGet, Route("Search")]
        public async Task<HttpResponseMessage> SearchUsersAsync(string query, string by = "name", int? start = null,
            int? take = null)
        {
            if (string.IsNullOrEmpty(query)) return Request.CreateApiErrorResponse("Missing query", HttpStatusCode.BadRequest);
            
            var users = await UserService.SearchUsersAsync(query, by, start, take);
            return Request.CreateSuccessResponse(users, $"List {users.Count} users");
        }


        [HttpGet, Route("BasicProfile")]
        public HttpResponseMessage GetBasicProfile()
        {
            return Request.CreateSuccessResponse(new CoreProfile
            {
                id = Account.Id.ToString(),
                accountId = Account.AccountId,
                displayName = Account.Profile.DisplayName,
                avatar = Account.Profile.PhotoUrl
            },"Current user basic profile");
        }

        [HttpGet, Route("Profile")]
        public HttpResponseMessage GetMainProfile()
        {
            return Request.CreateSuccessResponse(new MainProfile(Account),"Current user profile" );
        } 
        [HttpGet, Route("CurrentProfile")]
        public HttpResponseMessage GetCurrentProfile()
        {
            return AsBusiness
                ? Request.CreateSuccessResponse(new MainBusinessProfile(BusinessAccount),"Current business user profile" )
                : Request.CreateSuccessResponse(new MainProfile(Account),"Current user profile" );
        }      
        [HttpGet, Route("Profile/{userId}")]
        public HttpResponseMessage GetProfileById(string userId)
        {
            Account account = null;
            try
            {
                account = AccountService.GetById(new ObjectId(userId));
            } catch
            {
                account = null;
            }
            if (account == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }
            bool isBusiness = account.AccountType == AccountType.Business;
            return isBusiness
                ? Request.CreateSuccessResponse(new MainBusinessProfile(account),"Business user profile" )
                : Request.CreateSuccessResponse(new MainProfile(account),"Invidual user profile" );
        }          
        
        [HttpGet, Route("BusinessProfile/{businessId}")]
        public HttpResponseMessage GetBusinessProfile(string businessId)
        {
            Account account = null;
            try
            {
                account = AccountService.GetById(new ObjectId(businessId));
            } catch
            {
                account = null;
            }
            if (account == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }
            bool isBusiness = account.AccountType == AccountType.Business;
            if (!isBusiness) return Request.CreateApiErrorResponse("Account is individual");
            return Request.CreateSuccessResponse(new MainBusinessProfile(account), "Business user profile");
        }        
        
        [HttpGet, Route("BusinessProfile")]
        public object GetBusinessProfile()
        {
            CheckBusiness();
            return new MainBusinessProfile(BusinessAccount);
        }

        [AllowAnonymous]
        [HttpGet, Route("BasicProfile/{userId}")]
        public HttpResponseMessage GetBasicProfileById(string userId)
        {
            Account account = AccountService.GetById(new ObjectId(userId));
            if (account == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }

            return Request.CreateSuccessResponse (new CoreProfile
            {
                id = account.Id.ToString(),
                accountId = account.AccountId,
                displayName = account.Profile.DisplayName,
                avatar = account.Profile.PhotoUrl
            },"User basic profile");
        }        
        
        [AllowAnonymous]
        [HttpGet, Route("BasicProfile")]
        public HttpResponseMessage GetBasicProfileByAccountId(string accountId)
        {
            Account account = AccountService.GetByAccountId(accountId);
            if (account == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }
            return Request.CreateSuccessResponse (new CoreProfile
            {
                id = account.Id.ToString(),
                accountId = account.AccountId,
                displayName = account.Profile.DisplayName,
                avatar = account.Profile.PhotoUrl
            },"User basic profile");
        }
    }
}