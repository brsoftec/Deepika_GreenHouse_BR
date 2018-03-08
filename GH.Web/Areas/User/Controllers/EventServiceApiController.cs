using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/EventService")]
    public class EventServiceApiController : ApiController
    {
        readonly IAccountService _accountService;

        public IBusinessMemberLogic BusinessMemberLogic { get; set; }
        public IPostBusinessLogic PostBusinessLogic { get; set; }
        public ICampaignCaculator CampaignCalculator { get; set; }
        public EventBusinessLogic eventBusinessLogic { get; set; }
        public IProfileBusinessLogic ProfileBusinessLogic { get; set; }
        private IRoleService _roleService;

        public EventServiceApiController()
        {
            this._accountService = new AccountService();
            this.ProfileBusinessLogic = new ProfileBusinessLogic();
            this.eventBusinessLogic = new EventBusinessLogic();
            this.CampaignCalculator = new CampaignCalculator();
            this.PostBusinessLogic = new PostBusinessLogic();
            this.BusinessMemberLogic = new BusinessMemberLogic();
            _roleService = new RoleService();
        }

      
        [Route("GetEventTemplate")]
        [HttpPost]
        public HttpResponseMessage GetEventTemplate(HttpRequestMessage request, EventModelView eventModelView)
        {
            if (eventModelView == null)
                eventModelView = new EventModelView();
            try
            {
                BsonDocument result =
                eventBusinessLogic.GetEventTemplate();
                var strresult = result.ToJson();
                eventModelView.EventTemplate = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                eventModelView.ReturnStatus = true;
                eventModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                eventModelView.ReturnStatus = false;
                eventModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, eventModelView);
            return response;
        }
        [Route("GetEventById")]
        [HttpPost]
        public HttpResponseMessage GetEventById(HttpRequestMessage request, EventModelView eventModelView)
        {
            if (eventModelView == null)
                eventModelView = new EventModelView();
            try
            {
                BsonDocument result = null;
                if (!string.IsNullOrEmpty(eventModelView.EventId))
                    result = eventBusinessLogic.GetEventById(eventModelView.EventId);
                else
                    result=eventBusinessLogic.GetEventTemplate();
                var strresult = result.ToJson();
                eventModelView.EventTemplate = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                eventModelView.ReturnStatus = true;
                eventModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                eventModelView.ReturnStatus = false;
                eventModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, eventModelView);
            return response;
        }

        [Route("GetEventsByUser")]
        [HttpPost]
        public HttpResponseMessage GetEventsByUser(HttpRequestMessage request, EventModelView eventModelView)
        {
            if (eventModelView == null)
                eventModelView = new EventModelView();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

               // busAccount.Profile.FirstName +" "
                var dataList = eventBusinessLogic.GetEventsByUser(busAccount);

                eventModelView.Listitems = dataList;

                eventModelView.ReturnStatus = true;
                eventModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                eventModelView.ReturnStatus = false;
                eventModelView.ReturnMessage = new string[] { ex.ToString() }.ToList();
            } 

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, eventModelView);
            return response;
        }
        [Route("SaveEvent")]
        [HttpPost]
        public HttpResponseMessage SaveEvent(HttpRequestMessage request, EventModelView vm)
        {
            if (vm == null)
                vm = new EventModelView();
            try
            {
                if (!string.IsNullOrEmpty(vm.EventId))
                    eventBusinessLogic.SaveEvent(vm.EventId, vm.StrEvent);
                else
                {
                    var busAccount = _accountService.GetByAccountId(User.Identity.GetUserId());


                    BsonDocument result = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrEvent);
                    eventBusinessLogic.InsertEvent(busAccount.AccountId, result);

                }
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, vm);
            return response;
        }
        [Route("InsertEvent")]
        [HttpPost]
        public HttpResponseMessage InsertEvent(HttpRequestMessage request, EventModelView vm)
        {
            if (vm == null)
                vm = new EventModelView();
            try
            {
                var busAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
               

                BsonDocument result = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrEvent);
                eventBusinessLogic.InsertEvent(busAccount.AccountId, result);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, vm);
            return response;
        }
        [Route("DeleteEvent")]
        [HttpPost]
        public HttpResponseMessage DeleteEvent(HttpRequestMessage request, EventModelView eventModelView)
        {
            if (eventModelView == null)
                eventModelView = new EventModelView();
            try
            {

                eventBusinessLogic.DeleteEvent(eventModelView.EventId);
                eventModelView.ReturnStatus = true;
                eventModelView.ReturnMessage = new string[] { "successfully" }.ToList();
            }
            catch
            {
                eventModelView.ReturnStatus = false;
                eventModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<EventModelView>(HttpStatusCode.OK, eventModelView);
            return response;
        }
      
    }
}
