using GH.Core.Services;
using System.Web.Mvc;
using GH.Core.Models;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using System;
using NLog;
using GH.Core.ViewModels;

namespace GH.Web.Areas.User.Controllers
{
    public class InteractionController : BaseController
    {
        private readonly IInteractionService interactionService;
        private readonly IAccountService accountService;

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InteractionController()
        {
            interactionService = new InteractionService();
            accountService = new AccountService();
        }

        // GET: /interaction/id
        public ActionResult Index(string id)
        {
            ViewBag.InteractionId = id;
            ViewBag.Account = Account;

            BsonDocument bsoncampaign = interactionService.GetInteractionById(id);
            if (bsoncampaign == null)
            {
                return ErrorView("You may be following an incorrect link, or the interaction may have been deleted.", "Interaction Not Found");
            }
                     
            string businessId = bsoncampaign["userId"].AsString;
            Account account = accountService.GetByAccountId(businessId);

            DateTime? until = DateTime.Parse(bsoncampaign["campaign"]["criteria"]["spend"]["endDate"]?.AsString);
            bool indefinite = bsoncampaign["campaign"]["criteria"]["spend"]["type"].AsString == "Daily";
            string type = bsoncampaign["campaign"]["type"].AsString.ToLower();
            if (type == "advertising")
            {
                type = "broadcast";
            }
            var campaign = bsoncampaign["campaign"].AsBsonDocument;
            var cp = campaign;
            var termsUrl = campaign.GetValue("termsAndConditionsFile", new BsonString(string.Empty)).AsString;

            InteractionViewModel interaction = new InteractionViewModel
            {
                Id = id,
                Type = type,
                Name = bsoncampaign["campaign"]["name"].AsString,
                Description = bsoncampaign["campaign"]["description"].AsString,
                Business = new InteractionBusinessViewModel
                {
                    Id = account.Id.ToString(),
                    AccountId = businessId,
                    Avatar = account.Profile.PhotoUrl,
                    Name = account.Profile.DisplayName,
                },
                Image = cp.GetValue("image", BsonString.Empty).AsString,
                SocialShare = campaign.GetValue("socialShare", new BsonString("all")).AsString,
                From = DateTime.Parse(bsoncampaign["campaign"]["criteria"]["spend"]["effectiveDate"]?.AsString),
                Until = until,
                TermsUrl = termsUrl,
                Verb = cp.GetValue("verb", BsonString.Empty).AsString,
                Participants = interactionService.CountParticipants(id)
            };
            if (interaction.Type == "event")
            {
                BsonDocument e = bsoncampaign["campaign"]["event"].AsBsonDocument;
                var enddate = e.GetValue("enddate", null);
                var endtime = e.GetValue("endtime", null);
                var theme = e.GetValue("theme", null);
                interaction.EventInfo = new InteractionEventViewModel
                {
                    fromDate = e.GetValue("startdate", BsonString.Empty).AsString,
                    fromTime = e.GetValue("starttime", BsonString.Empty).AsString,
                    toDate = enddate == BsonNull.Value ? null : enddate.AsString,
                    toTime = endtime == BsonNull.Value ? null : endtime.AsString,
                    location = e.GetValue("location", BsonString.Empty).AsString,
                    theme = theme == BsonNull.Value ? null : theme.AsString
                };
            }

            if (interaction.Type == "event" || interaction.Type == "registration")
            {
                bool? paid = cp.GetValue("paid", BsonNull.Value).AsNullableBoolean;
                if (paid == null)
                {
                    string pay = cp.GetValue("usercodetype", "Free").AsString;
                    paid = pay == "Pay";
                }
                if (paid == true)
                {
                    string usercode = cp.GetValue("price", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(usercode))
                    {
                        usercode = cp.GetValue("usercode", new BsonString("0")).AsString;
                    }
                    interaction.price = usercode;
                    interaction.priceCurrency = cp.GetValue("priceCurrency", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(interaction.priceCurrency))
                    {
                        interaction.priceCurrency = cp.GetValue("usercodecurrentcy", new BsonString("USD")).AsString;
                    }
                }
                
                interaction.paid = paid == true;

                interaction.termsType = cp.GetValue("termsType", new BsonString("file")).AsString;
                interaction.termsUrl = cp.GetValue("termsUrl", BsonString.Empty).AsString;

                if (string.IsNullOrEmpty(termsUrl))
                {
                    interaction.termsUrl = cp.GetValue("termsAndConditionsFile", BsonString.Empty).AsString;
                }

            }
            if (Account != null && !AsBusiness && type != "broadcast")
            {
                var accountId = Account.AccountId;
                var delegators = Account.Delegations?.Where(d => d.Direction.Equals("DelegationIn")).ToList();
                var delegatees = Account.Delegations?.Where(d => d.Direction.Equals("DelegationOut")).ToList();

                interaction.Business.Following = interactionService.IsFollowing(accountId, businessId);
                var participations = interactionService.GetParticipations(Account, id);
                if (participations != null)
                {
                    interaction.Participations = new List<InteractionParticipationViewModel>();
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
                                var delegatee = delegatees?.FirstOrDefault(d => d.ToAccountId.Equals(delegateeId));
                                name = delegatee?.ToUserDisplayName;
                            }
                        }
                        else
                        {
                            actor = "for";
                            var delegator =
                                delegators?.FirstOrDefault(d => d.FromAccountId.Equals(participation.UserId));
                            name = delegator?.FromUserDisplayName;
                        }
                        var part = new InteractionParticipationViewModel
                        {
                            Actor = actor,
                            ActorName = name
                        };
                        if (participation.DelegationId != null)
                        {
                            part.DelegationId = participation.DelegationId;
                        }
                        if (participation.FollowedDate != null)
                        {
                            part.Participated = participation.FollowedDate;
                        }
                        interaction.Participations.Add(part);
                    }
                }
            }
            if (until != null && !indefinite)
            {
                var endDate = (DateTime) until;
                var endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

                if (endTime < DateTime.Today)
                {
                    interaction.Expired = true;
                }
            }

            return View(interaction);
        }
    }
}