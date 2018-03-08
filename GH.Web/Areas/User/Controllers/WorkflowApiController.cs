using GH.Core.Exceptions;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using GH.Core.Services;
using GH.Core.Models;
using MongoDB.Bson;
using GH.Core.IServices;
using GH.Core.BlueCode.BusinessLogic;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("Api/Workflow")]
    public class WorkflowApiController : ApiController
    {
        private IAccountService _accountService;
        private IJoiningBusinessInvitationService _joinBusService;
        private IRoleService _roleService;
        private INotificationService _notifyService;
        private ITransactionService _transactionService;

        public WorkflowApiController()
        {
            _accountService = new AccountService();
            _joinBusService = new JoiningBusinessInvitationService();
            _roleService = new RoleService();
            _notifyService = new NotificationService();
            _transactionService = new TransactionService();
        }

        [HttpGet, Route("Invitations")]
        public async Task<List<JoiningBusinessInvitation>> GetAllWorkflowInvitationsFromBusiness([FromUri] string businessId)
        {
            return _joinBusService.GetAllWorkflowInvitationsFromBusiness(new ObjectId(businessId));
        }     
               
        [HttpPost, Route("RemoveInvitation/{invitationId}")]
        public async Task RemoveInvitation(string invitationId)
        {
            _joinBusService.RemoveInvitation(new ObjectId(invitationId));
        }           
        [HttpPost, Route("AcceptInvitation/{invitationId}")]
        public async Task AcceptInvitation(string invitationId)
        {
            _joinBusService.AcceptInvitation(new ObjectId(invitationId));
        }                  
        [HttpPost, Route("DenyInvitation/{invitationId}")]
        public async Task DenyInvitation(string invitationId)
        {
            _joinBusService.DenyInvitation(new ObjectId(invitationId));
        }     
        
        [HttpPost, Route("InviteMember")]
        public async Task InviteMember(WorkflowMemberViewModel model)
        {
            var sender = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            var receiver = _accountService.GetById(new ObjectId(model.memberId));

            if (receiver == null)
            {
                throw new CustomException("Receiver not found");
            }

            var invitation = _joinBusService.Invite(new ObjectId(model.businessId), new ObjectId(model.memberId), model.roles.ToList());

          
            //WRITE LOG
            if (sender != null)
            {
                var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordWorkflow)
                {
                    string title = "You sent a workflow member invitation.";
                    string type = "workflow";
                    if (receiver.Profile.DisplayName != null)
                    {
                        title = "You sent workflow invitation to " + receiver.Profile.DisplayName.ToString() + ".";
                    }
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }
        }       
        
        [HttpPost, Route("UpdateMember")]
        public async Task UpdateMember(WorkflowMemberViewModel model)
        {
            var modifier = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            /*ObjectId businessAccountId;
            if (modifier.AccountType == AccountType.Business)
            {
                businessAccountId = modifier.Id;
            }
            else
            {
                businessAccountId = modifier.BusinessAccountRoles.First().AccountId;
            }*/

//            var roles = _roleService.GetAllRoles();
//            var adminRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_ADMIN);
//            var reviewerRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_REVIEWER);
//            var editorRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_EDITOR);
            _accountService.UpdateMember(new ObjectId(model.businessId), new ObjectId(model.memberId), model.roles);
        }       
        [HttpPost, Route("RemoveMember")]
        public async Task RemoveMember(WorkflowMemberViewModel model)
        {
            var modifier = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            /*ObjectId businessAccountId;
            if (modifier.AccountType == AccountType.Business)
            {
                businessAccountId = modifier.Id;
            }
            else
            {
                businessAccountId = modifier.BusinessAccountRoles.First().AccountId;
            }*/

//            var roles = _roleService.GetAllRoles();
//            var adminRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_ADMIN);
//            var reviewerRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_REVIEWER);
//            var editorRole = roles.FirstOrDefault(r => r.Name == Role.ROLE_EDITOR);
            _accountService.RemoveMemberFromBusiness(new ObjectId(model.businessId), new ObjectId(model.memberId),
                modifier.Id);
        }
    }
    

}