using System;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.Models;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NLog;
using GH.Core.Adapters;
using GH.Core.ViewModels;
using MongoDB.Driver;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Request;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("api/interactions")]
    public class InteractionsApiController : BaseApiController
    {
        static readonly IAccountService AccountService = new AccountService();

        private static readonly IBusinessInteractionService BusinessInteractionService =
            new BusinessInteractionService();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [HttpGet, Route("List")]
        public async Task<HttpResponseMessage> ListInteractionsAsync()
        {
            var result = await BusinessInteractionService.ListInteractionsAsync(BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interactions.notFound")
                    return Request.CreateApiErrorResponse("No interactions found", error: "interactions.list.notFound");
                return Request.CreateApiErrorResponse("Error listing interactions", error: "interactions.list.error");
            }

            return Request.CreateSuccessResponse(result.Data, "List interactions");
        }

        [HttpGet, Route("Get/{interactionId}")]
        public async Task<HttpResponseMessage> GetInteractionAsync(string interactionId)
        {
            var result = await BusinessInteractionService.GetBusinessInteractionAsync(interactionId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("interaction.notAuthorized", error: "Interaction not owned");
                return Request.CreateApiErrorResponse("interaction.notFound");
            }

            return Request.CreateSuccessResponse(result.Data, "Interaction found");
        }
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class PushInteractionModel
        {
            public string InteractionId { get; set; }
            public string Message { get; set; }
            public string ToAccountId { get; set; }
        }
       [HttpPost, Route("Push")]
        public async Task<HttpResponseMessage> PushInteractionAsync(PushInteractionModel model)
        {
            if (model == null)  return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            var interactionId = model.InteractionId;
            if (string.IsNullOrEmpty(interactionId)) 
                return Request.CreateApiErrorResponse("Missing interaction ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.ToAccountId)) 
                return Request.CreateApiErrorResponse("Missing account ID", HttpStatusCode.BadRequest);
            
            var result = await BusinessInteractionService.GetInteractionAsync(interactionId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("Interaction not owned", error: "interaction.notAuthorized");
                return Request.CreateApiErrorResponse("Interaction not found", error:"interaction.notFound");
            }

            var interaction = (Interaction) result.Data;

            var toAccount = AccountService.GetByAccountId(model.ToAccountId);
            if (toAccount == null)
                return Request.CreateApiErrorResponse("Account not found");

            result = await BusinessInteractionService.PushInteractionAsync(interaction, model.Message, toAccount, BusinessAccount);
            
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "interaction.notFound": return Request.CreateApiErrorResponse("Interaction not found");
                    case "interaction.not.pending": case "handshake.not.pending": return Request.CreateApiErrorResponse("Interaction already participated");
                    case "handshake.terminated": 
                        return Request.CreateApiErrorResponse("Handshake terminated", error: "handshake.terminated");
                    default: return Request.CreateApiErrorResponse("Error registering interaction");
                }
            }
            return Request.CreateSuccessResponse(new 
            {
                interactionId,
                accountId = model.ToAccountId,
                status = "pending"
            }, "Interaction pushed");
        }
        
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class UnregisterInteractionModel
        {
            public string InteractionId { get; set; }
            public string FromAccountId { get; set; }
        }
       [HttpPost, Route("Unregister")]
        public async Task<HttpResponseMessage> UnregisterInteractionAsync(UnregisterInteractionModel model)
        {
            if (model == null)  return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            var interactionId = model.InteractionId;
            if (string.IsNullOrEmpty(interactionId)) 
                return Request.CreateApiErrorResponse("Missing interaction ID", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.FromAccountId)) 
                return Request.CreateApiErrorResponse("Missing account ID", HttpStatusCode.BadRequest);
            
            var result = await BusinessInteractionService.GetInteractionAsync(interactionId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("Interaction not owned", error: "interaction.notAuthorized");
                return Request.CreateApiErrorResponse("Interaction not found", error:"interaction.notFound");
            }

            var interaction = (Interaction) result.Data;

            var fromAccount = AccountService.GetByAccountId(model.FromAccountId);
            if (fromAccount == null)
                return Request.CreateApiErrorResponse("Account not found");

            result = await BusinessInteractionService.UnregisterInteractionAsync(interaction, fromAccount, BusinessAccount);
            
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "interaction.notParticipated": return Request.CreateApiErrorResponse("Interaction not registered");
                    default: return Request.CreateApiErrorResponse("Error unregistering interaction");
                }
            }
            return Request.CreateSuccessResponse(new 
            {
                interactionId,
                accountId = model.FromAccountId,
                status = "removed"
            }, "Interaction unregistered");
        }

        [HttpGet, Route("Customers")]
        public async Task<HttpResponseMessage> ListCustomersAsync()
        {
            var result = await BusinessInteractionService.ListCustomersAsync(BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "notFound")
                    return Request.CreateSuccessResponse(new object[] { }, "No customers found");
                return Request.CreateApiErrorResponse("customers.list.error");
            }

            return Request.CreateSuccessResponse(result.Data, "List customers");
        }

        [HttpGet, Route("Participants/{interactionId}")]
        public async Task<HttpResponseMessage> ListParticipantsAsync(string interactionId)
        {
            //CheckBusiness();
            var result = await BusinessInteractionService.GetInteractionAsync(interactionId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("interaction.notAuthorized", error: "Interaction not owned");
                return Request.CreateApiErrorResponse("interaction.notFound");
            }

            var interaction = (Interaction) result.Data;
            if (interaction.Type == "Advertising")
                return Request.CreateSuccessResponse(new object[] { }, "Registration not applicable for Broadcast");
            interaction.Id = interactionId;
            result = await BusinessInteractionService.ListCustomersByInteractionAsync(interaction, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "notFound")
                    return Request.CreateSuccessResponse(new object[] { }, "No participant found");
                return Request.CreateApiErrorResponse("participants.list.error");
            }

            return Request.CreateSuccessResponse(result.Data, "List participants");
        }

        [HttpGet, Route("Participant")]
        public async Task<HttpResponseMessage> GetParticipantAsync(string interactionId, string accountId)
        {
            //CheckBusiness();
            var result = await BusinessInteractionService.GetInteractionAsync(interactionId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("interaction.notAuthorized", error: "Interaction not owned");
                return Request.CreateApiErrorResponse("interaction.notFound");
            }

            var interaction = (Interaction) result.Data;
            interaction.Id = interactionId;
            result = await BusinessInteractionService.GetParticipantAsync(interaction, accountId, BusinessAccount);
            if (!result.Success)
            {
                if (result.Status == "interaction.notAuthorized")
                    return Request.CreateApiErrorResponse("Interaction not owned", error: "interaction.notAuthorized");
                return Request.CreateApiErrorResponse("interaction.notFound");
            }

            return Request.CreateSuccessResponse(result.Data, "Participant found");
        }

        [HttpPost, Route("New")]
        public HttpResponseMessage NewInteraction(BusinessInteractionPayload model)
        {
            //CheckBusiness();
            string newId = BusinessInteractionService.CreateInteraction(model.json);
            return Request.CreateResponse<BusinessInteractionPayload>(HttpStatusCode.Created,
                new BusinessInteractionPayload
                {
                    id = newId,
                    status = "created"
                });
        }

        [HttpPost, Route("Save")]
        public HttpResponseMessage UpdateInteraction(BusinessInteractionPayload model)
        {
            //CheckBusiness();

            BusinessInteractionService.UpdateInteraction(model.id, model.json);
            return Request.CreateResponse<BusinessInteractionPayload>(HttpStatusCode.OK,
                new BusinessInteractionPayload
                {
                    id = model.id,
                    status = "saved"
                });
        }

        [HttpGet, Route("Get2/{interactionId}")]
        public BusinessInteractionViewModel GetInteraction(string interactionId)
        {
            //CheckBusiness();
            if (string.IsNullOrEmpty(interactionId))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No interaction ID"));
            }

            BsonDocument camp = BusinessInteractionService.GetInteraction(interactionId);
            if (camp == null)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.NotFound, "Interaction not found"));
            }

            Log.Debug(camp);
            BusinessInteractionViewModel interaction = InteractionAdapter.BsonToInteractionViewModel(camp);
            //new BusinessInteractionViewModel(c);
            return interaction;
        }

        [HttpGet, Route("get/interactions")]
        public IHttpActionResult GetListInteraction(string userId = null, string type = null)
        {
            //CheckBusiness();
            var lstCampaign = new List<CampaignDto>();
            if (string.IsNullOrEmpty(type))
                type = EnumCampaignType.HandShake;
            var lstDoc = BusinessInteractionService.GetListInteractionActive(type, userId);
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            foreach (var item in lstDoc)
            {
                var bsIn = new CampaignDto();
                bsIn = CampaignAdapter.BsonToCampaignDto(item);

                if (bsIn != null)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("campaignid", bsIn.Id);
                    filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                    var status = "";
                    if (postHandShakeCollection.Find(filter).Count() > 0)
                        status = EnumRequest.StatusComplete;
                    bsIn.status = status;
                    lstCampaign.Add(bsIn);
                }
            }

            return Ok(lstCampaign);
        }

        [HttpGet, Route("get/srfis")]
        public IHttpActionResult GetListSRFI(string userId = null)
        {
            //CheckBusiness();
            var lstCampaign = new List<CampaignDto>();

            var type = EnumCampaignType.SRFI;
            var lstDoc = BusinessInteractionService.GetListInteractionActive(type, userId);

            foreach (var item in lstDoc)
            {
                var bsIn = new CampaignDto();
                bsIn = CampaignAdapter.BsonToCampaignDto(item);

                if (bsIn != null)
                    lstCampaign.Add(bsIn);
            }

            return Ok(lstCampaign);
        }

        [HttpGet, Route("active")]
        public IHttpActionResult GetListInteractionActive(string type = null, int start = 0, int take = 10)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();

            var lstCampaign =
                BusinessInteractionService.GetListInteractionActiveWithPagesize(type, userId, start, take);

            return Ok(lstCampaign);
        }
    }
}