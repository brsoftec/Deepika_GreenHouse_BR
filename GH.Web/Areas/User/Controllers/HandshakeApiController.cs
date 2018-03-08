using System;
using System.Collections;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.BlueCode.Entity.ManualHandshake;
using GH.Core.BlueCode.Entity.Request;
using GH.Core.Models;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NLog;
using GH.Core.ViewModels;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [ApiAuthorize]
    [RoutePrefix("Api/Handshake")]
    public class HandshakeApiController : BaseApiController
    {
        static readonly IAccountService AccountService = new AccountService();
        static readonly IInteractionService InteractionService = new InteractionService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

   
        [HttpGet, Route("List")]
        public async Task<HttpResponseMessage> ListHandshakesAsync(string include="")
        {
            var result = await InteractionService.ListHandshakesAsync(Account, include);
            if (result.Status == "handshakes.notFound")
                return Request.CreateSuccessResponse(new object[] {}, "No handshakes found");
            return Request.CreateSuccessResponse(result.Data, $"List {((List<UserHandshake>)result.Data).Count} handshakes");
        }   
    
        [HttpGet, Route("Individual/List")]
        public async Task<HttpResponseMessage> ListPersonalHandshakesAsync(string filter="all", string include="")
        {
            var result = await InteractionService.ListPersonalHandshakesAsync(Account, filter, include);
            if (result.Status == "handshakes.notFound")
                return Request.CreateSuccessResponse(new object[] {}, "No handshakes found");
            return Request.CreateSuccessResponse(result.Data, $"List {((List<UserPersonalHandshake>)result.Data).Count} handshakes");
        }       
            
        [HttpGet, Route("Request/List")]
        public async Task<HttpResponseMessage> ListHandshakeRequestsAsync()
        {
            var result = await InteractionService.ListHandshakeRequestsAsync(Account);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error listing handshake requests",
                    HttpStatusCode.InternalServerError, "hsRequests.list.error");
            var hsrs = (List<UserHandshakeRequest>) result.Data;
            if (hsrs.Count == 0)
                return Request.CreateSuccessResponse(new object[] {}, "No handshake requests found");
            return Request.CreateSuccessResponse(hsrs, $"List {hsrs.Count} handshake requests");
        }          
        [HttpGet, Route("Business/Request/List")]
        public async Task<HttpResponseMessage> ListHandshakeRequestsByBusinessAsync()
        {
            CheckBusiness();
            var result = await InteractionService.ListHandshakeRequestsByBusinessAsync(Account);
            if (!result.Success)
                return Request.CreateApiErrorResponse("Error listing handshake requests",
                    HttpStatusCode.InternalServerError, "hsRequests.list.error");
            var hsrs = (List<UserHandshakeRequest>) result.Data;
            if (hsrs.Count == 0)
                return Request.CreateSuccessResponse(new object[] {}, "No handshake requests found");
            return Request.CreateSuccessResponse(hsrs, $"List {hsrs.Count} handshake requests");
        }       
        
        [HttpPost, Route("Individual/Add")]
        public async Task<HttpResponseMessage> AddPersonalHandshakesAsync(PersonalHandshakePostModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Invalid parameters");
            if (string.IsNullOrEmpty(model.ToAccountId) && string.IsNullOrEmpty(model.ToEmail))
                return Request.CreateApiErrorResponse("Missing recipient ID or email", error: "handshake.missing.recipient");
            if (model.FieldPaths == null || model.FieldPaths.Count == 0)
                return Request.CreateApiErrorResponse("Missing field list", error: "handshake.missing.fields");
            var result = await InteractionService.AddPersonalHandshakeAsync(model, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.exists")
                {
                    var hs = (ManualHandshake) result.Data;
                    return Request.CreateApiErrorResponse("Handshake relationship exists with recipient",
                        error: "handshake.exists", payload: new
                        {
                            handshakeId = hs.Id.ToString(),
                            hs.status,
                            recipient = string.IsNullOrEmpty(model.ToAccountId) ? hs.toEmail : model.ToAccountId,
                        });

                }
                if (result.Status == "handshake.expiry.invalid")
                    return Request.CreateApiErrorResponse("Invalid expiry date", error: "handshake.expiry.invalid");
                if (result.Status == "handshake.fields.invalid")
                    return Request.CreateApiErrorResponse("Invalid fields: unknown path", error: "handshake.fields.invalid");
                    return Request.CreateApiErrorResponse("Error creating handshake", error: "handshake.create.error");
            }

            return Request.CreateSuccessResponse(new { handshakeId = result.Data }, "Individual handshake created successfully");
        }   
 
        
        [HttpPost, Route("Pause/{interactionId}")]
        public async Task<HttpResponseMessage> PauseHandshakeAsync(string interactionId)
        {
            var result = await InteractionService.UpdateHandshakeStatusAsync(interactionId, "paused", Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.pending")
                    return Request.CreateApiErrorResponse("Handshake not registered", error: "handshake.pending");
                if (result.Status == "handshake.status.update.not")
                    return Request.CreateApiErrorResponse("Handshake already paused", error: "handshake.already.paused");
                return Request.CreateApiErrorResponse("Handshake update error", error: "handshake.update.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId = result.Data,
                interactionId,
                status = "paused"
            }, "Handshake paused");
        }        
        [HttpPost, Route("Individual/Pause/{handshakeId}")]
        public async Task<HttpResponseMessage> PausePersonalHandshakeAsync(string handshakeId)
        {
            var result = await InteractionService.UpdateStatusPersonalHandshakeAsync(handshakeId, "paused", Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.terminated")
                    return Request.CreateApiErrorResponse("Handshake terminated", error: "handshake.terminated");
                if (result.Status == "handshake.status.update.not")
                    return Request.CreateApiErrorResponse("Handshake already paused", error: "handshake.already.paused");
                return Request.CreateApiErrorResponse("Handshake update error", error: "handshake.update.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId,
                withAccountId = result.Data,
                status = "paused"
            }, "Handshake paused");
        }         
        [HttpPost, Route("Individual/Block/{handshakeId}")]
        public async Task<HttpResponseMessage> BlockPersonalHandshakeAsync(string handshakeId)
        {
            var result = await InteractionService.UpdateStatusPersonalHandshakeAsync(handshakeId, "blocked", Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.terminated")
                    return Request.CreateApiErrorResponse("Handshake terminated", error: "handshake.terminated");
                if (result.Status == "handshake.recipient.invalid")
                    return Request.CreateApiErrorResponse("Only recipient allowed to block handshake", error: "handshake.notAuthorized");
                if (result.Status == "handshake.status.update.not")
                    return Request.CreateApiErrorResponse("Handshake already blocked", error: "handshake.already.blocked");
                return Request.CreateApiErrorResponse("Handshake update error", error: "handshake.update.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId,
                withAccountId = result.Data,
                status = "blocked"
            }, "Handshake blocked");
        }          
        [HttpPost, Route("Request/Remove/{requestId}")]
        public async Task<HttpResponseMessage> RemoveHandshakeRequestAsync(string requestId)
        {
            var result = await InteractionService.RemoveHandshakeRequestAsync(requestId, AccountId);
            if (!result.Success)
            {
                if (result.Status == "handshake.request.invalid")
                    return Request.CreateApiErrorResponse("Invalid handshake request ID", error: "handshake.request.invalid");
                if (result.Status == "handshake.request.notFound")
                    return Request.CreateApiErrorResponse("Handshake request not found", error: "handshake.request.notFound");
                if (result.Status == "handshake.request.notAuthorized")
                    return Request.CreateApiErrorResponse("Only owner allowed to remove handshake request", error: "handshake.request.notAuthorized");
                return Request.CreateApiErrorResponse("Handshake request removal error", error: "handshake.request.remove.error");
            }
            return Request.CreateSuccessResponse(new
            {
                requestId,
                toBusinessId = result.Data
            }, "Handshake request removed");
        }       
                        
        [HttpPost, Route("Request/Add")]
        public async Task<HttpResponseMessage> AddHandshakeRequestAsync(HandshakeRequestPostModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Invalid parameters");
            if (string.IsNullOrEmpty(model.ToAccountId) && string.IsNullOrEmpty(model.ToUcbId))
                return Request.CreateApiErrorResponse("Missing business ID", error: "handshake.request.missing.business");
            var result = await InteractionService.AddHandshakeRequestAsync(model, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.request.exists")
                {
                    var hsr = (Request) result.Data;
                    return Request.CreateApiErrorResponse("A handshake request already sent to business",
                        error: "handshake.request.exists", payload: new
                        {
                            requestId = hsr.Id.ToString(),
                            businessId = hsr.ToUserId,
                            status = "sent"
                        });

                }

                return Request.CreateApiErrorResponse("Error creating handshake request", error: "handshake.request.create.error");
            }

            return Request.CreateSuccessResponse(new { requestId = result.Data }, "Handshake request created successfully");
        }  
        [HttpPost, Route("Individual/Resume/{handshakeId}")]
        public async Task<HttpResponseMessage> ResumePersonalHandshakeAsync(string handshakeId)
        {
            var result = await InteractionService.UpdateStatusPersonalHandshakeAsync(handshakeId, "active", Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.terminated")
                    return Request.CreateApiErrorResponse("Handshake terminated", error: "handshake.terminated");
                if (result.Status == "handshake.recipient.invalid")
                    return Request.CreateApiErrorResponse("Only recipient allowed to unblock handshake", error: "handshake.notAuthorized");
                if (result.Status == "handshake.status.update.not")
                    return Request.CreateApiErrorResponse("Handshake already active", error: "handshake.already.active");
                return Request.CreateApiErrorResponse("Handshake update error", error: "handshake.update.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId,
                withAccountId = result.Data,
                status = "active"
            }, "Handshake resumed");
        }      
        [HttpPost, Route("Resume/{interactionId}")]
        public async Task<HttpResponseMessage> ResumeHandshakeAsync(string interactionId)
        {
            var result = await InteractionService.UpdateHandshakeStatusAsync(interactionId, "active", Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.pending")
                    return Request.CreateApiErrorResponse("Handshake not registered", error: "handshake.pending");               
                if (result.Status == "handshake.status.update.not")
                    return Request.CreateApiErrorResponse("Handshake already active", error: "handshake.already.active");
                return Request.CreateApiErrorResponse("Handshake update error", error: "handshake.update.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId = result.Data,
                interactionId,
                status = "active"
            }, "Handshake resumed");
        }       
        [HttpPost, Route("Terminate/{interactionId}")]
        public async Task<HttpResponseMessage> TerminateHandshakeAsync(string interactionId)
        {
            var result = await InteractionService.TerminateHandshakeAsync(interactionId, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                return Request.CreateApiErrorResponse("Error terminating handshake", error: "handshake.terminate.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId = result.Data,
                interactionId,
                status = "terminated"
            }, "Handshake terminated successfully");
        }      
              
        [HttpPost, Route("Individual/Terminate/{handshakeId}")]
        public async Task<HttpResponseMessage> TerminatePersonalHandshakeAsync(string handshakeId="")
        {
            if (string.IsNullOrEmpty(handshakeId))
                return Request.CreateApiErrorResponse("Missing handshake ID", HttpStatusCode.BadRequest);
            var result = await InteractionService.TerminatePersonalHandshakeAsync(handshakeId, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.invalid")
                    return Request.CreateApiErrorResponse("Invalid handshake ID", error: "handshake.invalid");
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                return Request.CreateApiErrorResponse("Error terminating handshake", error: "handshake.terminate.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId,
                withAccountId = result.Data,
                status = "terminated"
            }, "Handshake terminated successfully");
        }        
        
        [HttpPost, Route("Individual/Remove/{handshakeId}")]
        public async Task<HttpResponseMessage> RemovePersonalHandshakeAsync(string handshakeId="")
        {
            if (string.IsNullOrEmpty(handshakeId))
                return Request.CreateApiErrorResponse("Missing handshake ID", HttpStatusCode.BadRequest);
            var result = await InteractionService.RemovePersonalHandshakeAsync(handshakeId, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.invalid")
                    return Request.CreateApiErrorResponse("Invalid handshake ID", error: "handshake.invalid");
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");
                if (result.Status == "handshake.notTerminated")
                    return Request.CreateApiErrorResponse("Handshake not already terminated", error: "handshake.notTerminated");

                return Request.CreateApiErrorResponse("Error removing handshake", error: "handshake.remove.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId,
                withAccountId = result.Data,
            }, "Handshake removed successfully");
        }      
        
        [HttpPost, Route("Remove/{interactionId}")]
        public async Task<HttpResponseMessage> RemoveHandshakeAsync(string interactionId)
        {
            var result = await InteractionService.RemoveHandshakeAsync(interactionId, Account);
            if (!result.Success)
            {
                if (result.Status == "handshake.notFound")
                    return Request.CreateApiErrorResponse("Handshake not found", error: "handshake.notFound");   
                if (result.Status == "handshake.notTerminated")
                    return Request.CreateApiErrorResponse("Handshake not already terminated", error: "handshake.notTerminated");
                return Request.CreateApiErrorResponse("Error removing handshake", error: "handshake.remove.error");
            }
            return Request.CreateSuccessResponse(new
            {
                handshakeId = result.Data,
                interactionId,
            }, "Handshake removed successfully");
        }

    }
}