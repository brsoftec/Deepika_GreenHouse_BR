using GH.Core.Services;
using System.Web.Mvc;
using GH.Web.Areas.User.ViewModels;
using GH.Core.Models;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using System;
using System.Web.Http;
using GH.Core.BlueCode.Entity.Campaign;
using NLog;
using GH.Core.ViewModels;
using GH.Core.Adapters;
using GH.Core.BlueCode.BusinessLogic;

namespace GH.Web.Areas.User.Controllers
{
    public class InteractionsController : BusinessController
    {
        private readonly IBusinessInteractionService BusinessInteractionService;
        private readonly IAccountService accountService;
        
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InteractionsController()
        {
            BusinessInteractionService = new BusinessInteractionService();
            accountService = new AccountService();
        }

        // GET: /interactions/
        public ActionResult Index(string id)
        {
            return View();
        }       
        // GET: /interactions/customers
        public ActionResult Customers(string interaction)
        {
            return View((object)interaction);
        }

        // GET: /interactions/edit/id
        public ActionResult Edit(string id, string type = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (string.IsNullOrEmpty(type)) type = EnumCampaignType.Registration;
                var newInteraction = new BusinessInteractionViewModel
                {
                    id = "",
                    type = type,
                    name = string.Empty,
                    description = string.Empty,
                    state = "new"
                };
                
                return View(newInteraction);
            }
            BsonDocument intdoc = BusinessInteractionService.GetInteraction(id);
            if (intdoc == null)
            {
                return ErrorView("Interaction not found", "Invalid Interaction", "interaction", "business");
            }
            if (intdoc.GetValue("userId", BsonString.Empty) != BusinessAccount.AccountId)
            {
                return ErrorView("Interaction not owned by your business", "Unauthorized Interaction", "interaction", "business");
            }
            BusinessInteractionViewModel interaction = InteractionAdapter.BsonToInteractionViewModel(intdoc);
            
            //    Count participants
            var participants = 0;
            int.TryParse(interaction.participants, out participants);
            try
            {
                var postlogic = new PostBusinessLogic();
                var userList = postlogic.GetFollowersList(interaction.id);
                if (userList.DataOfCurrentPage != null)
                    participants = userList.DataOfCurrentPage.Count();
            }
            catch
            {
            }
            try
            {
                if (interaction.type == "handshake")
                {
                    participants =
                        new PostHandShakeBusinessLogic().GetCountUserhoidHandshakebycmapaignid(interaction.id);
                    var countinvitedhandshake =
                        new PostHandShakeBusinessLogic().GetCountUserInvitedPending(interaction.id);
                    participants += countinvitedhandshake;
                }
            }
            catch
            {
            }
            interaction.participants = participants.ToString();
            interaction.state = "edit";
            return View(interaction);
        }

        public ActionResult New(string id = null, [FromUri] string templateId = null)
        {
            if (string.IsNullOrEmpty(id)) id = EnumCampaignType.Registration;
            BusinessInteractionViewModel newInteraction = null;
            if (string.IsNullOrEmpty(templateId))
            {
                newInteraction = new BusinessInteractionViewModel
                {
                    id = "",
                    type = id,
                    name = string.Empty,
                    status = "new",
                    description = string.Empty,
                    state = "new"
                };
            }
            else
            {
                BsonDocument intdoc = BusinessInteractionService.GetInteraction(templateId);
                if (intdoc == null)
                {
                    return ErrorView("Template not found", "Invalid Interaction", "interaction", "business");
                }
                if (intdoc.GetValue("userId", BsonString.Empty) != BusinessAccount.AccountId)
                {
                    return ErrorView("Interaction template not owned by your business", "Unauthorized Interaction", "interaction", "business");
                }
                newInteraction = InteractionAdapter.BsonToInteractionViewModel(intdoc);
                newInteraction.state = "newFromTemplate";
                newInteraction.status = "New";
                newInteraction.id = string.Empty;
                
            }

            return View("Edit", newInteraction);
        }       
        public ActionResult Review(string id = null)
        {
            BsonDocument intdoc = BusinessInteractionService.GetInteraction(id);
            if (string.IsNullOrEmpty(id) || intdoc == null)
            {
                return ErrorView("Interaction not found", "Invalid Interaction", "interaction", "business");
            }
            if (intdoc.GetValue("userId", BsonString.Empty) != BusinessAccount.AccountId)
            {
                return ErrorView("Interaction not owned by your business", "Unauthorized Interaction", "interaction", "business");
            }
            BusinessInteractionViewModel interaction = InteractionAdapter.BsonToInteractionViewModel(intdoc);
            interaction.state = "review";
            ViewBag.Reviewing = true;
            return View("Edit", interaction);
        }
    }
}