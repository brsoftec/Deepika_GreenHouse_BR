using GH.Core.Exceptions;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using GH.Web.Areas.User.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Http;
using GH.Core.BlueCode.Entity.Invite;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    
    public class EmailInvite
    {
        public string toEmail { get; set; }
        public string inviteId { get; set; }
        public string fromAccountId { get; set; }
        public string fromDisplayName { get; set; }
        public string toName { get; set; }
        public string category { get; set; }
        public string message { get; set; }
        public string options { get; set; }
        public object payload { get; set; }
    }  
    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class BulkInviteModel
    {
        public string[] Emails { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
    [Authorize]
    [RoutePrefix("Api/Invite")]
    public class InviteApiController : BaseApiController
    {
        IAccountService _accountService;
        IInviteService _inviteService;

        private Logger log = LogManager.GetCurrentClassLogger();

        public InviteApiController()
        {
            _accountService = new AccountService();
            _inviteService = new InviteService();
        }

        [HttpPost, Route("NewInvite")]
        public async Task NewInvite(EmailInvite model)
        {
            var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            string toEmail = model.toEmail;

            var invite = new Invite
            {
                Created = DateTime.UtcNow,
                Email = toEmail,
                FromUserId = model.fromAccountId,
                FromDisplayName = model.fromDisplayName,
                ToName = model.toName,
                Message = model.message,
                Category = model.category,
                Status = "created"
            };
            if (model.options != null)
            {
                invite.Options = model.options;
            }
            if (model.payload != null)
            {
                invite.Payload = JsonConvert.SerializeObject(model.payload);
            }

            Invite newInvite = _inviteService.CreateInvite(invite);
            _inviteService.SendInvite(newInvite);
        }        
        
        [HttpPost, Route("NewInvites")]
        public async Task NewInvite(EmailInvite[] list)
        {
            var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            foreach (var model in list)
            {
                string toEmail = model.toEmail;

                var invite = new Invite
                {
                    Created = DateTime.Now,
                    Email = toEmail,
                    FromUserId = model.fromAccountId,
                    FromDisplayName = model.fromDisplayName,
                    ToName = model.toName,
                    Message = model.message,
                    Category = model.category,
                    Status = "created"
                };
                if (model.options != null)
                {
                    invite.Options = model.options;
                }
                if (model.payload != null)
                {
                    invite.Payload = JsonConvert.SerializeObject(model.payload);
                }

                Invite newInvite = _inviteService.CreateInvite(invite);
                _inviteService.SendInvite(newInvite);
            }
        }

        private bool isValidEmail(string email)
        {
            const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        
        [HttpPost, Route("Bulk")]
        [BaseApi]
        public async Task<HttpResponseMessage> BulkInviteAsync(BulkInviteModel model)
        {
            if (model == null)
            {
                return Request.CreateApiErrorResponse("Missing parameters", HttpStatusCode.BadRequest);
            }          
            if (model.Emails == null || model.Emails.Length == 0)
            {
                return Request.CreateApiErrorResponse("Missing email addresses", HttpStatusCode.BadRequest);
            }

            var invalidEmails = new List<string>();
            var existingEmails = new List<string>();
            var sentEmails = new List<string>();
            var unsentEmails = new List<string>();

            foreach (var email in model.Emails)
            {
                if (!isValidEmail(email))
                {
                    invalidEmails.Add(email);
                    continue;
                }
                var account = _accountService.GetByEmail(email);
                if (account != null)
                {
                    existingEmails.Add(email);
                    continue;
                }
                var invite = new Invite
                {
                    Created = DateTime.Now,
                    Email = email,
                    FromUserId = AccountId,
                    FromDisplayName = Account?.Profile.DisplayName,
                    Message = model.Message ?? "",
                    Category = "network",
                    Status = "created"
                };
                
                Invite newInvite = _inviteService.CreateInvite(invite);
                if (newInvite == null)
                {
                    unsentEmails.Add(email);
                    continue;
                }

                var result = await _inviteService.SendInviteAsync(newInvite);
                if (!result.Success)
                {
                    unsentEmails.Add(email);
                    continue;
                }

                sentEmails.Add(email);
            }

            var sentAll = sentEmails.Count == model.Emails.Length;
            var sentNone = sentEmails.Count == 0;

            return Request.CreateSuccessResponse(new
            {
                sentEmails,
                invalidEmails,
                existingEmails,
                unsentEmails
            }, sentNone ? "No invitations sent" : sentAll ? "Invitations sent succesfully" : "Some invitations sent");
        }

        [HttpPost, Route("ResendInvite/{inviteId}")]
        public async Task ResendInvite(string inviteId)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var invite = _inviteService.GetInviteById(inviteId);


            try
            {
                _inviteService.SendInvite(invite);
            }
            catch (Exception e)
            {
                log.Debug("Error resending invite: " + e.Message);
            }
        }

        [HttpGet, Route("Invites")]
        public async Task<List<Invite>> GetInvites([FromUri] string fromUserId, [FromUri] string category, [FromUri] string status = null)
        {
            var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            return _inviteService.GetInvites(fromUserId, category, status);
        }

        [HttpPost, Route("DeleteInvite/{inviteId}")]
        public async Task DeleteInvite(string inviteId)
        {
            var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            _inviteService.DeleteInviteById(inviteId);
        }
    }
}