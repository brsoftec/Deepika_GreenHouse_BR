using GH.Core.Adapters;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.ManualHandshake;
using GH.Core.Services;
using GH.Core.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/manualhandshake")]
    public class ManualHandShakeApiController : ApiController
    {
        private IAccountService _accountService;
        private IManualHandshakeBusinessLogic _manualHandshake;
        public ManualHandShakeApiController()
        {
            _accountService = new AccountService();
            _manualHandshake = new ManualHandshakeBusinessLogic();

        }

        [Route("invite")]
        [HttpPost]
        public IHttpActionResult Invite(ManualHandshakeViewModel handshake)
        {
            var rs = "";
            if (string.IsNullOrEmpty(handshake.accountId))
                handshake.accountId = HttpContext.Current.User.Identity.GetUserId();
            handshake.synced = DateTime.UtcNow;
            var account = _accountService.GetByAccountId(handshake.accountId);
            handshake.email = account.Profile.Email;
            handshake.name = account.Profile.DisplayName ?? account.Profile.FirstName + ' ' + account.Profile.LastName;

            var manualHandshake = ManualHandshakeAdapter.ViewModelToModel(handshake);
            if (string.IsNullOrEmpty(manualHandshake.status))
                manualHandshake.status = EnumManualHandshake.Active;
            if (!string.IsNullOrEmpty(manualHandshake.toEmail))
            {
                var toAccount = _accountService.GetByEmail(manualHandshake.toEmail);
                if (toAccount != null)
                    manualHandshake.toAccountId = toAccount.AccountId;
            }
            rs = _manualHandshake.Insert(manualHandshake);
            return Ok(rs);
        }

        [HttpGet, Route("account")]
        public IHttpActionResult GetByAccount(string accountId = null)
        {
            var rs = new List<ManualHandshakeViewModel>();

            if (string.IsNullOrEmpty(accountId))
                accountId = HttpContext.Current.User.Identity.GetUserId();

            var manualHandshake = _manualHandshake.GetActiveListByAccountId(accountId);
            if (manualHandshake.Count > 0)
            {

                foreach (var item in manualHandshake)
                {
                    var manualHandshakeViewModel = ManualHandshakeAdapter.ModelToViewModel(item);
                    if (manualHandshakeViewModel != null)
                        rs.Add(manualHandshakeViewModel);
                }
            }

            return Ok(rs);
        }

        [HttpGet, Route("accountwithpaging")]
        public IHttpActionResult GetPagingByAccount(string accountId = null, int start = 0, int take = 10)
        {
            var rs = new List<ManualHandshakeViewModel>();
            if (string.IsNullOrEmpty(accountId))
                accountId = HttpContext.Current.User.Identity.GetUserId();

            var manualHandshake = _manualHandshake.GetListPagingByAccountId(accountId, start, take);

          

            if (manualHandshake.Count > 0)
            {
                foreach (var item in manualHandshake)
                {
                    var manualHandshakeViewModel = ManualHandshakeAdapter.ModelToViewModel(item);
                    if (manualHandshakeViewModel != null)
                        rs.Add(manualHandshakeViewModel);
                }
            }

            return Ok(rs);
        }

        [HttpGet, Route("toaccountwithpaging")]
        public IHttpActionResult GetPagingByToAccount(string toAccountId = null, int start = 0, int take = 10)
        {
            var rs = new List<ManualHandshakeViewModel>();
            if (string.IsNullOrEmpty(toAccountId))
                toAccountId = HttpContext.Current.User.Identity.GetUserId();
            var manualHandshake = _manualHandshake.GetListPagingByToAccountId(toAccountId, start, take);
            if (manualHandshake.Count > 0)
            {
                foreach (var item in manualHandshake)
                {
                    var manualHandshakeViewModel = ManualHandshakeAdapter.ModelToViewModel(item);
                    if (manualHandshakeViewModel != null)
                        rs.Add(manualHandshakeViewModel);
                }
            }

            return Ok(rs);
        }

        [HttpGet, Route("email")]
        public IHttpActionResult GetPagingByEmail(string email = null, int start = 0, int take = 10)
        {
            var rs = new List<ManualHandshakeViewModel>();

            if (string.IsNullOrEmpty(email))
                return null;
            var manualHandshake = _manualHandshake.GetActiveByEmail(email, start, take);
            if (manualHandshake.Count > 0)
            {

                foreach (var item in manualHandshake)
                {
                    var manualHandshakeViewModel = ManualHandshakeAdapter.ModelToViewModel(item);
                    if (manualHandshakeViewModel != null)
                        rs.Add(manualHandshakeViewModel);
                }
            }

            return Ok(rs);
        }

        [HttpGet, Route("toEmail")]
        public IHttpActionResult GetPagingByToEmail(string toEmail = null, int start = 0, int take = 10)
        {
            var rs = new List<ManualHandshakeViewModel>();
            if (string.IsNullOrEmpty(toEmail))
                return null;
            var manualHandshake = _manualHandshake.GetActiveByEmail(toEmail, start, take);

            if (manualHandshake.Count > 0)
            {
                foreach (var item in manualHandshake)
                {
                    var manualHandshakeViewModel = ManualHandshakeAdapter.ModelToViewModel(item);
                    if (manualHandshakeViewModel != null)
                        rs.Add(manualHandshakeViewModel);
                }
            }

            return Ok(rs);
        }
    }
}
