using System;
using System.Collections;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
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
    [RoutePrefix("Api/Interaction")]
    public class InteractionApiController : BaseApiController
    {
        static readonly IAccountService AccountService = new AccountService();
        static readonly IInteractionService InteractionService = new InteractionService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [HttpGet, Route("IsFollowing/{businessId}")]
        public HttpResponseMessage IsFollowing(string businessId = "")
        {
            if (string.IsNullOrEmpty(businessId))
                return Request.CreateApiErrorResponse("Missing parameter", HttpStatusCode.BadRequest);
            Account businessAccount;
            try
            {
                businessAccount = AccountService.GetById(new ObjectId(businessId));
            }
            catch
            {
                businessAccount = null;
            }
            if (businessAccount == null)
                return Request.CreateApiErrorResponse("Business not found");

            var following = InteractionService.IsFollowing(AccountId, businessAccount.AccountId);
            return Request.CreateSuccessResponse(new
            {
                following = following,
                businessId = businessId
            }, following ? "Following business" : "Not following business");
        }

        [HttpPost, Route("Follow/{businessId}")]
        public HttpResponseMessage Follow(string businessId)
        {
            if (string.IsNullOrEmpty(businessId))
                return Request.CreateApiErrorResponse("Missing parameter", HttpStatusCode.BadRequest);
            Account businessAccount;
            try
            {
                businessAccount = AccountService.GetById(new ObjectId(businessId));
            }
            catch
            {
                businessAccount = null;
            }
            if (businessAccount == null)
                return Request.CreateApiErrorResponse("Business not found");

            var success = InteractionService.Follow(Account, businessAccount);
            if (success)
                return Request.CreateSuccessResponse(new
                {
                    following = true,
                    businessId = businessId
                }, "Followed business");
            else
                return Request.CreateApiErrorResponse("Error following business");
        }

        [HttpPost, Route("Unfollow/{businessId}")]
        public async Task<HttpResponseMessage> Unfollow(string businessId)
        {
            if (string.IsNullOrEmpty(businessId))
                return Request.CreateApiErrorResponse("Missing parameter", HttpStatusCode.BadRequest);
            Account businessAccount;
            try
            {
                businessAccount = AccountService.GetById(new ObjectId(businessId));
            }
            catch
            {
                businessAccount = null;
            }
            if (businessAccount == null)
                return Request.CreateApiErrorResponse("Business not found");

            var result = await InteractionService.UnfollowAsync(AccountId, businessAccount.AccountId);
            if (!result.Success)
                    return Request.CreateApiErrorResponse("Error unfollowing business");
                return Request.CreateSuccessResponse(new
                {
                    following = false,
                    businessId
                }, "Unfollowed business");
        }

        [HttpGet, Route("Details/{interactionId}")]
        public async Task<HttpResponseMessage> GetInteractionDetailsAsync(string interactionId, string delegationId="")
        {
            var account = Account;
            UserDelegation delegation = null;
            if (!string.IsNullOrEmpty(delegationId))
            {
                var getDel = new DelegationBusinessLogic().GetUserDelegation(delegationId, account);
                if (!getDel.Success)
                    return Request.CreateApiErrorResponse("Delegation not found", error: "delegation.notFound");
                delegation = (UserDelegation)getDel.Data;
                if (delegation.ToAccountId != AccountId)
                    return Request.CreateApiErrorResponse("User data denied for non-delegatee",
                        error: "delegation.notAuthorized");
                account = AccountService.GetByAccountId(delegation.FromAccountId);
            }
            var result = await InteractionService.GetUserInteractionAsync(interactionId, account, delegation);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound": return Request.CreateApiErrorResponse("Interaction not found");
                    default: return Request.CreateApiErrorResponse("Error getting interaction details");
                }
            }
            return Request.CreateSuccessResponse(result.Data, "Interaction details");
        }

        [HttpGet, Route("Details")]
        public async Task<HttpResponseMessage> GetInteractionDetailsById(string interactionId, string delegationId="")
        {
            return await GetInteractionDetailsAsync(interactionId, delegationId);
        }

        [HttpGet, Route("Get")]
        public async Task<HttpResponseMessage> GetInteraction(string interactionId)
        {
            if (string.IsNullOrEmpty(interactionId))
                return Request.CreateApiErrorResponse("Missing interaction ID");
            var result = await InteractionService.GetInteraction(interactionId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound": return Request.CreateApiErrorResponse("Interaction not found", error:"interaction.notFound");
                    default: return Request.CreateApiErrorResponse("Error getting interaction");
                }
            }
            return Request.CreateSuccessResponse(result.Data, "Interaction data");
        }       
        [HttpGet, Route("List")]
        public async Task<HttpResponseMessage> GetActiveInteractionsFromBusiness(string businessAccountId,
            string type = "")
        {
            if (string.IsNullOrEmpty(businessAccountId))
                return Request.CreateApiErrorResponse("Missing business account ID");
            var result = await InteractionService.GetActiveInteractionsFromBusiness(businessAccountId, type);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound": return Request.CreateApiErrorResponse("No active interactions found");
                    default: return Request.CreateApiErrorResponse("Error getting interactions");
                }
            }
            return Request.CreateSuccessResponse(result.Data, "List active interactions");
        }            
        
        [HttpGet, Route("List/Srfi")]
        public async Task<HttpResponseMessage> GetActiveSrfiInteractionsFromBusiness(string businessId = "", string businessAccountId = "")
        {
            if (!string.IsNullOrEmpty(businessId))
            {
                var account = AccountService.GetById(ObjectId.Parse(businessId));
                businessAccountId = account?.AccountId;
            }
            if (string.IsNullOrEmpty(businessAccountId))
                return Request.CreateApiErrorResponse("Missing business ID", HttpStatusCode.BadRequest);
            var result = await InteractionService.GetActiveInteractionsFromBusiness(businessAccountId, "SRFI");
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound": return Request.CreateApiErrorResponse("No active SRFIs found");
                    default: return Request.CreateApiErrorResponse("Error getting SRFIs");
                }
            }
            return Request.CreateSuccessResponse(result.Data, $"List {((IList)result.Data).Count} active SRFIs");
        }

        [HttpGet, Route("Participation")]
        public async Task<HttpResponseMessage> GetParticipation(string interactionId, string accountId=null)
        {
            if (string.IsNullOrEmpty(interactionId)) 
                return Request.CreateApiErrorResponse("Missing interaction ID", HttpStatusCode.BadRequest);
            var account = Account;
            if (string.IsNullOrEmpty(accountId))
            {
                accountId = AccountId;
            }
            else
            {
                account = AccountService.GetByAccountId(accountId);
                if (account == null)
                    return Request.CreateApiErrorResponse("Account not found");
            }
            
            var result = await InteractionService.GetParticipation(interactionId, account);
            
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "notFound": return Request.CreateSuccessResponse(null, "Interaction not registered");
                    default: return Request.CreateApiErrorResponse("Error getting interaction participation");
                }
            }
            return Request.CreateSuccessResponse(result.Data, "Interaction participation details");
        }

        [HttpPost, Route("Unregister")]
        public async Task<HttpResponseMessage> UnregisterInteraction(UnparticipationPostModel model)
        {
            if (model == null) return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            var interactionId = model.InteractionId;
            if (string.IsNullOrEmpty(interactionId))
                return Request.CreateApiErrorResponse("Missing interaction ID", HttpStatusCode.BadRequest);
            var account = Account;
            var delegationId = model.DelegationId;
            UserDelegation delegation = null;
            if (!string.IsNullOrEmpty(delegationId))
            {
                var getDel = new DelegationBusinessLogic().GetUserDelegation(delegationId, account);
                if (!getDel.Success)
                    return Request.CreateApiErrorResponse("Delegation not found",
                        error: "unregister.delegation.notFound");
                delegation = (UserDelegation) getDel.Data;
                if (delegation.ToAccountId != AccountId)
                    return Request.CreateApiErrorResponse("Delegation invalid: only delegatee allowed to unregister",
                        error: "unregister.delegation.invalid");
                if (delegation.Status != "accepted")
                    return Request.CreateApiErrorResponse("Delegation not active",
                        error: "unregister.delegation.pending");
                account = AccountService.GetByAccountId(delegation.FromAccountId);
            }
  
            var result = await InteractionService.Unregister(interactionId, account, delegation);

            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "interaction.notFound": return Request.CreateApiErrorResponse("Interaction not found");
                    case "unregister.handshake.na": return Request.CreateApiErrorResponse("Cannot unregister handshake interaction");
                    case "interaction.notParticipated": return Request.CreateApiErrorResponse("Interaction not already registered");
                    default: return Request.CreateApiErrorResponse("Error unregistering interaction");
                }
            }
            return Request.CreateSuccessResponse(new
            {
                interactionId,
                accountId = account.AccountId,
                participated = false
            }, "Interaction unregistered successfully");

        }

        [HttpPost, Route("Register")]
        public async Task<HttpResponseMessage> RegisterInteraction(ParticipationPostModel post)
        {
            if (post == null)  return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            var interactionId = post.InteractionId;
            if (string.IsNullOrEmpty(interactionId)) 
                return Request.CreateApiErrorResponse("Missing interaction ID", HttpStatusCode.BadRequest);
            var account = Account;
            var delegationId = post.DelegationId;
            UserDelegation delegation = null;
            if (!string.IsNullOrEmpty(delegationId))
            {
                var getDel = new DelegationBusinessLogic().GetUserDelegation(delegationId, account);
                if (!getDel.Success)
                    return Request.CreateApiErrorResponse("Delegation not found");
                delegation = (UserDelegation)getDel.Data;
                account = AccountService.GetByAccountId(delegation.FromAccountId);
            }
            var accountId = account.AccountId;

            var fields = post.Fields;
            
            var result = await InteractionService.Register(interactionId, account, fields, delegation);
            
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "interaction.notFound": return Request.CreateApiErrorResponse("Interaction not found");
                    case "interaction.participated": 
                        return Request.CreateApiErrorResponse("Interaction already registered", error: "interaction.already.registered");    
                    case "handshake.terminated": 
                        return Request.CreateApiErrorResponse("Handshake terminated", error: "handshake.terminated");
                    case "field.missing": 
                        return Request.CreateApiErrorResponse($"Missing required field: {result.Data}", 
                        error: "interaction.field.missing");
                    case "field.null": return Request.CreateApiErrorResponse($"Missing value for field: {result.Data}",
                        error: "interaction.field.empty");              
                    case "field.readonly": return Request.CreateApiErrorResponse($"Delegated field not writable: {result.Data}",
                        error: "interaction.field.readonly");              
                    case "field.forbidden": return Request.CreateApiErrorResponse($"Delegated field denied access: {result.Data}",
                        error: "interaction.field.notAuthorized");              
                    case "field.invalid": return Request.CreateApiErrorResponse($"Value of invalid data type for field: {result.Data}",
                        error: "interaction.field.invalid");                 
                    case "field.invalid.format": return Request.CreateApiErrorResponse($"Invalid format for field: {result.Data}",
                        error: "interaction.field.invalid.format");                   
                    case "field.invalid.value": return Request.CreateApiErrorResponse($"Invalid value for field: {result.Data}",
                        error: "interaction.field.invalid.value");
                    default: return Request.CreateApiErrorResponse("Error registering interaction");
                }
            }
            var participatedFields = (List<FieldUpdate>) result.Data;
            return Request.CreateSuccessResponse(new ParticipationResult
            {
                InteractionId = interactionId,
                AccountId = accountId,
                UpdatedFields = participatedFields
            }, "Interaction registered successfully");
        }

        [HttpGet, Route("Fields/{interactionId}")]
        public string GetInteractionFields(string interactionId)
        {
            var fields = InteractionService.GetInteractionFields(interactionId);
            if (fields == null)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.NotFound, "No interaction fields"));
            }
//            BsonSerializer.Deserialize<object>
//            return fields.Select(p => p.ToBsonDocument()).ToList();
            return fields.ToJson();
//            return fields[0].ToDictionary(f => f["_name"].AsString, f => (object)f["_value"]);
        }

        [HttpGet, Route("Groups/{interactionId}")]
        public IHttpActionResult GetInteractionGroups(string interactionId)
        {
            CheckAuthenticated();
            var groups = InteractionService.GetInteractionGroups(interactionId);
            if (groups == null)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.NotFound, "No interaction groups"));
            }
            return Ok(groups.ToJson());
        }

        [HttpGet, Route("Participation/{interactionId}")]
        public InteractionParticipationViewModel GetInteractionParticipation(string interactionId)
        {
            var participation = new InteractionParticipationViewModel();
            try
            {
                CheckAuthenticated();
                if (AsBusiness)
                {
                    throw new HttpResponseException(
                        Request.CreateErrorResponse(HttpStatusCode.NotFound, "Participation for personal user only"));
                }
                var follower = InteractionService.GetParticipation(AccountId, interactionId);
                if (follower == null)
                {
                    throw new HttpResponseException(
                        Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No participation"));
                }
                var delegators = Account.Delegations?.Where(d => d.Direction.Equals("DelegationIn")).ToList();
                var delegatees = Account.Delegations?.Where(d => d.Direction.Equals("DelegationOut")).ToList();

                var actor = "self";
                var name = "";
                var delegationId = "";
                if (follower.UserId == AccountId)
                {
                    var delegateeId = follower.DelegateeId;
                    if (delegateeId != null && delegateeId != AccountId)
                    {
                        actor = "by";
                        var delegatee = delegatees?.FirstOrDefault(d => d.ToAccountId.Equals(delegateeId));
                        name = delegatee?.ToUserDisplayName;
                    }
                }
                else
                {
                    actor = "for";
                    var delegator = delegators?.FirstOrDefault(d => d.FromAccountId.Equals(follower.UserId));
                    name = delegator?.FromUserDisplayName;
                }
                participation = new InteractionParticipationViewModel
                {
                    Actor = actor,
                    ActorName = name
                };
                if (follower.DelegationId != null)
                {
                    participation.DelegationId = follower.DelegationId;
                }
                if (follower.FollowedDate != null)
                {
                    participation.Participated = follower.FollowedDate;
                }
            }
            catch
            {
            }

            return participation;
        }

        [HttpGet, Route("Feed")]
        public HttpResponseMessage GetInteractionFeed([FromUri] int page = 1, [FromUri] int limit = 4,
            [FromUri] string businessAccountId = null)
        {
            int PageSize = limit;
            if (!ConfigHelp.GetBoolValue("IsEnableInteractionsInNewFeed"))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Interaction feed inaccessible"));
            }

            string userId = AccountId;

            var campaignService = new CampaignBusinessLogic();


//            var listhandshakefollow = new PostHandShakeBusinessLogic().Getlisthandshakefollow(userId);

            List<DelegationItemTemplate> delegators = null;
            List<DelegationItemTemplate> delegatees = null;

            List<FeedInteractionViewModel> interactions = new List<FeedInteractionViewModel>();

            if (!AsBusiness)
            {
                delegators = Account.Delegations?.Where(d => d.Direction.Equals("DelegationIn")).ToList();
                delegatees = Account.Delegations?.Where(d => d.Direction.Equals("DelegationOut")).ToList();
                var campaignList = businessAccountId == null
                    ? campaignService.GetActiveCampaignsForUser(userId, "", "feed", true, "")
                        .Skip((page - 1) * PageSize).Take(PageSize)
                    : campaignService.GetInteractionFeedFromBusiness(userId, businessAccountId)
                        .Skip((page - 1) * PageSize).Take(PageSize);
                //var campaignList = campaignService.GetActiveCampaignsForUser(userId, "", "feed", true, "");

                foreach (var item in campaignList.OrderByDescending(x => x.Id))
                {
                    var interaction = new FeedInteractionViewModel();
                    Account businessAccount = new AccountService().GetByAccountId(item.BusinessUserId);
                    if (businessAccount != null)
                    {
                        interaction.id = item.Id.ToString();
                        interaction.type = item.CampaignType == "Advertising"
                            ? "broadcast"
                            : item.CampaignType.ToLower();
                        interaction.name = item.CampaignName;
                        interaction.status = item.Status;
                        interaction.description = item.Description;
                        interaction.business = new FeedBusinessViewModel
                        {
                            id = businessAccount.Id.ToString(),
                            accountId = item.BusinessUserId,
                            avatar = businessAccount.Profile.PhotoUrl,
                            name = businessAccount.Profile.DisplayName,
                        };
                        if (interaction.type == "event")
                        {
                            interaction.eventInfo = new InteractionEventViewModel
                            {
                                fromTime = item.starttime,
                                fromDate = item.startdate,
                                toTime = item.endtime,
                                toDate = item.enddate,
                                location = item.location,
                                theme = item.theme,
                            };
                        }

                        if (interaction.type == "event" || interaction.type == "registration")
                        {
                            interaction.paid = item.usercodetype == "Paid";
                            interaction.price = item.usercode;
                            interaction.priceCurrency = item.usercodecurrentcy;
                        }
                        interaction.from = item.SpendEffectDate;
                        interaction.until = item.SpendEndDate;
                        interaction.indefinite = item.TimeType == "Daily";
                        interaction.image = item.Image;
                        interaction.targetUrl = item.TargetLink;
                        interaction.termsUrl = item.termsAndConditionsFile;
                        interaction.verb = item.Verb;
                        interaction.socialShare = item.SocialShare;
                        var accountId = AccountId;
                        interaction.business.following = InteractionService.IsFollowing(accountId, item.BusinessUserId);

                        if (interaction.type != "broadcast")
                        {
                            var participations = InteractionService.GetParticipations(Account, interaction.id);
                            if (participations != null)
                            {
                                interaction.participations = new List<FeedParticipationViewModel>();
                                foreach (var participation in participations)
                                {
                                    var actor = "self";
                                    var name = "";
                                    var delegationId = "";
                                    if (participation.UserId == accountId)
                                    {
                                        var delegateeId = participation.DelegateeId;
                                        if (delegateeId != null && delegateeId != accountId)
                                        {
                                            actor = "by";
                                            var delegatee =
                                                delegatees?.FirstOrDefault(d => d.ToAccountId == delegateeId);
                                            name = delegatee?.ToUserDisplayName;
                                        }
                                    }
                                    else
                                    {
                                        actor = "for";
                                        var delegator =
                                            delegators?.FirstOrDefault(d => d.FromAccountId == participation.UserId);
                                        name = delegator?.FromUserDisplayName;
                                    }
                                    if (actor != "for")
                                    interaction.participated = true;
                                    var part = new FeedParticipationViewModel
                                    {
                                        actor = actor,
                                        actorName = name
                                    };
                                    if (participation.DelegationId != null)
                                    {
                                        part.delegationId = participation.DelegationId;
                                    }
                                    if (participation.ParticipatedAt != null)
                                    {
                                        part.participated = participation.ParticipatedAt?.ToString("o");
                                    }
                                    else if (participation.FollowedDate != null)
                                    {
                                        part.participated = participation.FollowedDate;
                                    }
                                    interaction.participations.Add(part);
                                }
                            }
                        }

                        var endDate = (DateTime) interaction.until;
                        var endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

                        if (!interaction.indefinite && endTime < DateTime.Today)
                        {
                            interaction.expired = true;
                        }

                        interaction.participants = InteractionService.CountParticipants(interaction.id);
                    }
                    if (interaction != null)
                        interactions.Add(interaction);

                    //end for
                }
            }
            else
            {
                Account businessAccount = new AccountService().GetByAccountId(businessAccountId);
                string businessId = BusinessAccount.AccountId;
                var business = new FeedBusinessViewModel
                {
                    id = BusinessAccount.Id.ToString(),
                    accountId = businessId,
                    avatar = BusinessAccount.Profile.PhotoUrl,
                    name = BusinessAccount.Profile.DisplayName,
                };
                var campaignListAll = campaignService
                    .GetCampaignByBusinessUser(businessId, "All", "All", true, page, PageSize, true, true, "", false,
                        true,
                        true).DataOfCurrentPage;
                var campaignList = campaignListAll.Where(x => x.BusinessUserId == businessId);

                foreach (var item in campaignList.OrderByDescending(x => x.Id))
                {
                    var interaction = new FeedInteractionViewModel();

                    interaction.id = item.Id;
                    interaction.type = item.CampaignType == "Advertising" ? "broadcast" : item.CampaignType.ToLower();
                    interaction.status = item.Status;
                    interaction.name = item.CampaignName;
                    interaction.description = item.Description;

                    interaction.business = business;

                    interaction.termsUrl = item.termsAndConditionsFile;
                    if (interaction.type == "event")
                    {
                        interaction.eventInfo = new InteractionEventViewModel
                        {
                            fromTime = item.starttime,
                            fromDate = item.startdate,
                            toTime = item.endtime,
                            toDate = item.enddate,
                            location = item.location,
                            theme = item.theme
                        };
                    }

                    interaction.from = item.SpendEffectDate;
                    interaction.until = item.SpendEndDate;
                    interaction.indefinite = item.TimeType == "Daily";
                    interaction.image = item.Image;
                    interaction.targetUrl = item.TargetLink;
                    interaction.verb = item.Verb;
                    interaction.socialShare = item.SocialShare;

                    var endDate = (DateTime) interaction.until;
                    var endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

                    if (!interaction.indefinite && endTime < DateTime.Today)
                    {
                        interaction.expired = true;
                    }

                    interaction.participants = InteractionService.CountParticipants(interaction.id);

                    interactions.Add(interaction);
                }
            }

            return Request.CreateSuccessResponse(interactions, $"List {interactions.Count} items");
        }

        const int PageSize = 20;

        [HttpGet, Route("Newsfeed")]
        public async Task<HttpResponseMessage> GetInteractionFeedPage([FromUri] int page = 1)
        {
            if (page < 1) return Request.CreateApiErrorResponse("Invalid parameter", HttpStatusCode.BadRequest);

            string userId = AccountId;

            var campaignService = new CampaignBusinessLogic();
            List<FeedInteractionViewModel> interactions = new List<FeedInteractionViewModel>();

            var campaignList = campaignService.GetActiveCampaignsForUser(userId, "", "feed", true, "")
                .Skip((page - 1) * PageSize).Take(PageSize);

            foreach (var item in campaignList.OrderByDescending(x => x.Id))
            {
                var interaction = new FeedInteractionViewModel();

                interaction.id = item.Id;
                interaction.business = new FeedBusinessViewModel
                {
                    accountId = item.BusinessUserId
                };
                Account businessAccount = new AccountService().GetByAccountId(item.BusinessUserId);
                if (businessAccount != null)
                {
                    interaction.business.id = businessAccount.Id.ToString();
                    interaction.business.name = businessAccount.Profile.DisplayName;
                    interaction.business.avatar = businessAccount.Profile.PhotoUrl;
                    interaction.business.following = true;
                }

                interaction.type = item.CampaignType == "Advertising" ? "broadcast" : item.CampaignType.ToLower();
                interaction.name = item.CampaignName;
                interaction.status = item.Status;
                interaction.description = item.Description;
                if (interaction.type == "event")
                {
                    interaction.eventInfo = new InteractionEventViewModel
                    {
                        fromTime = item.starttime,
                        fromDate = item.startdate,
                        toTime = item.endtime,
                        toDate = item.enddate,
                        location = item.location,
                        theme = item.theme,
                    };
                }

                if (interaction.type == "event" || interaction.type == "registration")
                {
                    interaction.paid = item.usercodetype == "Paid";
                    interaction.price = item.usercode;
                    interaction.priceCurrency = item.usercodecurrentcy;
                }
                interaction.from = item.SpendEffectDate;
                interaction.until = item.SpendEndDate;
                interaction.indefinite = item.TimeType == "Daily";
                interaction.image = item.Image;
                interaction.targetUrl = item.TargetLink;
                interaction.termsUrl = item.termsAndConditionsFile;
                interaction.verb = item.Verb;
                interaction.socialShare = item.SocialShare;
                var accountId = AccountId;

                var endDate = (DateTime) interaction.until;
                var endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

                if (!interaction.indefinite && endTime < DateTime.Today)
                {
                    interaction.expired = true;
                }
                
                
                interaction.participated = await InteractionService.IsParticipated(interaction.id, Account);

                interaction.participants = InteractionService.CountParticipants(interaction.id);

                    interactions.Add(interaction);

                //end for
            }

            return Request.CreateSuccessResponse(interactions, $"List {interactions.Count} items");
        }
        
        private MultipartFormDataStreamProvider GetMultipartProvider()
        {
            var uploadFolder = "~/";
            var root = HttpContext.Current.Server.MapPath(uploadFolder);
            Directory.CreateDirectory(root + @"Content\vault\documents\" + AccountId + @"\uploads");
            return new MultipartFormDataStreamProvider(root);
        }
                
        [HttpPost, Route("Upload")]
        public async Task<HttpResponseMessage> UploadFileAsync(string interactionId)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateApiErrorResponse("Invalid or unsupported file format",
                    HttpStatusCode.UnsupportedMediaType, "upload.invalid");
            }

            var filePath = $"~/Content/vault/documents/{AccountId}/uploads";
            
            var provider = GetMultipartProvider();
            var result = await Request.Content.ReadAsMultipartAsync(provider);
            var currentUser = Account;
            string fileName = "";
            var files = new List<object>();
            if (!provider.FileData.Any())
                return Request.CreateApiErrorResponse("Missing files",
                    HttpStatusCode.BadRequest, "upload.missing");

            try
            {
                foreach (var fileData in provider.FileData)
                {
                    fileName = fileData.Headers.ContentDisposition.FileName;
                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                    {
                        fileName = fileName.Trim('"');
                    }
                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                    {
                        fileName = Path.GetFileName(fileName);
                    }
                    string extension = Path.GetExtension(fileName);
                    var uploadFolder = filePath;
                    var storagePath = HttpContext.Current.Server.MapPath(uploadFolder);
                    if (File.Exists(Path.Combine(storagePath, fileName)))
                        File.Delete(Path.Combine(storagePath, fileName));

                    File.Move(fileData.LocalFileName, Path.Combine(storagePath, fileName));
                    files.Add(new
                    {
                        filePath = filePath.Substring(1),
                        fileName = fileName,
                        status = "uploaded"
                    });
                }
            }
            catch
            {
                return Request.CreateApiErrorResponse("Error uploading files",
                    HttpStatusCode.InternalServerError, "upload.error");
            }

            return Request.CreateSuccessResponse(files, "Files uploaded");
        }
    }
}