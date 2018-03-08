using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using GH.Core.Exceptions;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.BlueCode.Entity.Notification;
using System.IO;
using System.Net;
using System.Reflection;
using GH.Core.ViewModels;
using GH.Core.Models;
using MongoDB.Bson;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("Api/Network")]
    public class NetworkApiController : BaseApiController
    {
        private static readonly INetworkService _networkService = new NetworkService();
        private static readonly IInteractionService InteractionService = new InteractionService();
        private IAccountService _accountService;
        private IOutsiteBusinessLogic _outsiteBusinessLogic;

        public NetworkApiController()
        {
            _accountService = new AccountService();
            _outsiteBusinessLogic = new OutsiteBusinessLogic();
        }

        [HttpGet, Route("IsFriend/{userId}")]
        public HttpResponseMessage IsFriend(string userId)
        {
            var result = _networkService.IsFriend(UserId, userId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "id.invalid":
                        return Request.CreateApiErrorResponse("Invalid user ID");
                    case "other.notFound":
                        return Request.CreateApiErrorResponse("Error finding user");
                    case "notFound":
                        return Request.CreateSuccessResponse(new
                        {
                            isMember = false,
                            userId,
                        }, "User is not network member");
                    default:
                        return Request.CreateApiErrorResponse("Error finding user");
                }
            }
            if (result.Status == "found.pending")
                return Request.CreateSuccessResponse(new
                {
                    isMember = false,
                    isPending = true,
                    userId,
                    invitationId = result.Data.ToString()
                }, "User is pending network member");

            var type = result.Status == "found.trust" ? "trust" : "normal";
            return Request.CreateSuccessResponse(new
            {
                isMember = true,
                userId,
                network = type
            }, "User is network member");
        }

        [HttpGet, Route("IsFriend")]
        public HttpResponseMessage IsFriendByAccountId(string accountId)
        {
            var account = _accountService.GetByAccountId(accountId);
            if (account == null)
                return Request.CreateApiErrorResponse("User not found");
            var result = _networkService.IsFriend(UserId, account.Id.ToString());
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "id.invalid":
                        return Request.CreateApiErrorResponse("Invalid user ID");
                    case "other.notFound":
                        return Request.CreateApiErrorResponse("Error finding user");
                    case "notFound":
                        return Request.CreateSuccessResponse(new
                        {
                            isMember = false,
                            accountId,
                        }, "User is not network member");
                    default:
                        return Request.CreateApiErrorResponse("Error finding user");
                }
            }
            if (result.Status == "found.pending")
                return Request.CreateSuccessResponse(new
                {
                    isMember = false,
                    isPending = true,
                    accountId,
                    invitationId = result.Data.ToString()
                }, "User is pending network member");

            var type = result.Status == "found.trust" ? "trust" : "normal";
            return Request.CreateSuccessResponse(new
            {
                isMember = true,
                accountId,
                network = type
            }, "User is network member");
        }

        [HttpGet, Route("Friends")]
        public async Task<HttpResponseMessage> GetFriendsAsync()
        {
            var result = await _networkService.GetFriendsAsync(Account);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound":
                        return Request.CreateSuccessResponse(new object[] { }, "No member in network");
                    default:
                        return Request.CreateApiErrorResponse("Error finding user");
                }
            }

            return Request.CreateSuccessResponse(result.Data, "List network members");
        }

        [HttpPost, Route("Trust")]
        public async Task<HttpResponseMessage> TrustMember(string userId = null, string accountId = null)
        {
            var sender = Account;

            Account receiver;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!ObjectId.TryParse(userId, out var objId))
                    return Request.CreateApiErrorResponse("Invalid member ID");
                receiver = _accountService.GetById(objId);
            }
            else if (!string.IsNullOrEmpty(accountId))
            {
                receiver = _accountService.GetByAccountId(accountId);
            }
            else
                return Request.CreateApiErrorResponse("Missing member ID");


            if (receiver == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }


            if (receiver.AccountId == sender.AccountId)
                return Request.CreateApiErrorResponse("Member cannot be yourself");

            var result = await _networkService.MoveFriendAsync(sender, receiver, true);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "friend.notFound": return Request.CreateApiErrorResponse("Member not found in normal network");
                    default: return Request.CreateApiErrorResponse("Error moving network member");
                }
            }

            return Request.CreateSuccessResponse(new
                {    userId = receiver.Id.ToString(),
                    accountId = receiver.AccountId,
                    isMember = true,
                    network = "trust"
                },"Member moved to trust network");
        }

        [HttpPost, Route("Untrust")]
        public async Task<HttpResponseMessage> UntrustMember(string userId = null, string accountId = null)
        {
            var sender = Account;

            Account receiver;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!ObjectId.TryParse(userId, out var objId))
                    return Request.CreateApiErrorResponse("Invalid member ID");
                receiver = _accountService.GetById(objId);
            }
            else if (!string.IsNullOrEmpty(accountId))
            {
                receiver = _accountService.GetByAccountId(accountId);
            }
            else
                return Request.CreateApiErrorResponse("Missing member ID");


            if (receiver == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }


            if (receiver.AccountId == sender.AccountId)
                return Request.CreateApiErrorResponse("Member cannot be yourself");

            var result = await _networkService.MoveFriendAsync(sender, receiver, false);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "friend.notFound": return Request.CreateApiErrorResponse("Member not found in trust network");
                    default: return Request.CreateApiErrorResponse("Error moving network member");
                }
            }

            return Request.CreateSuccessResponse(new
                {    userId = receiver.Id.ToString(),
                    accountId = receiver.AccountId,
                    isMember = true,
                    network = "normal"
                }, "Member moved to normal network");
        }     
        
        [HttpPost, Route("Remove")]
        public async Task<HttpResponseMessage> RemoveMember(string userId = null, string accountId = null)
        {
            var sender = Account;

            Account receiver;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!ObjectId.TryParse(userId, out var objId))
                    return Request.CreateApiErrorResponse("Invalid member ID");
                receiver = _accountService.GetById(objId);
            }
            else if (!string.IsNullOrEmpty(accountId))
            {
                receiver = _accountService.GetByAccountId(accountId);
            }
            else
                return Request.CreateApiErrorResponse("Missing member ID");


            if (receiver == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }


            if (receiver.AccountId == sender.AccountId)
                return Request.CreateApiErrorResponse("Member cannot be yourself");

            var result = await _networkService.RemoveFriendAsync(sender, receiver);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "friend.notFound": return Request.CreateApiErrorResponse("Member not found in network");
                    default: return Request.CreateApiErrorResponse("Error removing network member");
                }
            }

            return Request.CreateSuccessResponse(new
                {    userId = receiver.Id.ToString(),
                    accountId = receiver.AccountId,
                    status = "removed",
                    isMember = false
                }, "Member removed from network");
        }

        [HttpPost, Route("Invite")]
        public HttpResponseMessage InviteMember([FromUri] string accountId)
        {
            var sender = Account;
            Account receiver = _accountService.GetByAccountId(accountId);


            if (receiver == null)
            {
                return Request.CreateApiErrorResponse("Account not found");
            }

            var result = _networkService.Invite(sender.Id, receiver.Id);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "existing": return Request.CreateApiErrorResponse("User already invited");
                    case "existingMutual": return Request.CreateApiErrorResponse("User has already invited you");
                    default: return Request.CreateApiErrorResponse("Error inviting to network");
                }
            }

            // Write activity log
            if (sender.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You invited " + receiver.Profile.DisplayName + " to your network";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(sender.AccountId, title, type);
            }

            return Request.CreateSuccessResponse(new {inviteeId = accountId, invitationId = result.Data},
                "Network invitation sent");
        }

        [HttpPost, Route("Invitation/Cancel")]
        public void CancelInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var remover = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // Write activity log
            if (remover.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You removed network invitation to ";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(remover.AccountId, title, type);
            }

            _networkService.RemoveInvitation(new MongoDB.Bson.ObjectId(model.InvitationId), remover.Id);
        }


        [HttpPost, Route("Accept")]
        public HttpResponseMessage AcceptInvitation(string invitationId)
        {
            if (!ObjectId.TryParse(invitationId, out ObjectId objId))
                return Request.CreateApiErrorResponse("Invalid invitation", HttpStatusCode.BadRequest);

            var result = _networkService.AcceptInvitation(objId, Account.Id);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "invitation.notFound": return Request.CreateApiErrorResponse("Invitation not found");
                    case "invitation.notTargeted":
                        return Request.CreateApiErrorResponse("Invitation not intended for you");
                    default: return Request.CreateApiErrorResponse("Error accepting network invitation");
                }
            }

            // Write activity log
            if (Account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You accepted an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(AccountId, title, type);
            }

            return Request.CreateSuccessResponse(new
            {
                invitationId = invitationId,
                status = "accepted",
                memberAccountId = result.Data.ToString(),
                isMember = true,
                network = "normal"
            }, "Network invitation accepted succesfully");
        }

        [HttpPost, Route("Deny")]
        public HttpResponseMessage DenyInvitation(string invitationId)
        {
            if (!ObjectId.TryParse(invitationId, out ObjectId objId))
                return Request.CreateApiErrorResponse("Invalid invitation", HttpStatusCode.BadRequest);

            var result = _networkService.DenyInvitation(objId, Account.Id);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "invitation.notFound": return Request.CreateApiErrorResponse("Invitation not found");
                    case "invitation.notTargeted":
                        return Request.CreateApiErrorResponse("Invitation not intended for you");
                    default: return Request.CreateApiErrorResponse("Error denying network invitation");
                }
            }

            // Write activity log
            if (Account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You denied an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(AccountId, title, type);
            }

            return Request.CreateSuccessResponse(new
            {
                invitationId = invitationId,
                status = "removed",
                senderAccountId = result.Data.ToString(),
                isMember = false,
            }, "Network invitation denied");
        }
        
        [HttpGet, Route("Business/List")]
        public async Task<HttpResponseMessage> ListFollowedBusinessesAsync()
        {
            var result = await InteractionService.ListFollowedBusinessesAsync(Account);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error getting followed businesses", error: "network.business.list.error");
            return Request.CreateSuccessResponse(result.Data, $"List {((List<InteractionService.UserFollowedBusiness>)result.Data).Count} businesses");
        }   
    
        
    }
}