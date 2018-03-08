using System;
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
using GH.Core.BlueCode.Entity.Post;
using GH.Core.Models;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using Newtonsoft.Json;
using GH.Util;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.PostHandShake;
using static GH.Web.Areas.User.ViewModels.CampaignViewModel;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.ViewModels;
using System.Reflection;
using System.Web.Hosting;
using System.Text.RegularExpressions;
using NLog;
using GH.Core.BlueCode.Entity.Request;
using GH.Core.Adapters;

namespace GH.Web.Areas.User.Controllers
{
    //api/CampaignService/GetCampaignsByUserNormal
    [Authorize]
    [RoutePrefix("api/CampaignService")]
    public class CampaignServiceController : ApiController
    {
        readonly IAccountService _accountService;
        public EventBusinessLogic eventBusinessLogic { get; set; }
        public IBusinessMemberLogic BusinessMemberLogic { get; set; }
        public IPostBusinessLogic PostBusinessLogic { get; set; }
        public ICampaignCaculator CampaignCalculator { get; set; }
        public ICampaignBusinessLogic CampaignBusinessLogic { get; set; }
        public IProfileBusinessLogic ProfileBusinessLogic { get; set; }
        public InfomationVaultBusinessLogic infomationVaultBusinessLogic { get; set; }
        private IRoleService _roleService;
        private IOutsiteBusinessLogic _outsiteBusinessLogic;

        string rootFolder = HttpContext.Current.Server.MapPath("~/App_Data");
        private Logger log = LogManager.GetCurrentClassLogger();
        public CampaignServiceController()
        {
            this._accountService = new AccountService();
            this.ProfileBusinessLogic = new ProfileBusinessLogic();
            this.CampaignBusinessLogic = new CampaignBusinessLogic();
            this.CampaignCalculator = new CampaignCalculator();
            this.PostBusinessLogic = new PostBusinessLogic();
            this.BusinessMemberLogic = new BusinessMemberLogic();
            infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
            _roleService = new RoleService();
            eventBusinessLogic = new EventBusinessLogic();
            _outsiteBusinessLogic = new OutsiteBusinessLogic();
        }

        [Route("GetCalculateNumberOfUser")]
        [HttpPost]
        public HttpResponseMessage GetCalculateNumberOfUser(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                campaignModelView.CampaignFilter.BusinessUserId = User.Identity.GetUserId();
                CampaignUserFilterResult result =
                    CampaignCalculator.CalculateNumberOfUser(campaignModelView.CampaignFilter);
                campaignModelView.CampaignUserFilterResult = result;
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetVaultTreeForRegistration")]
        [HttpPost]
        public HttpResponseMessage GetVaultTreeForRegistration(HttpRequestMessage request,
            CampaignRegisterTreeVaultViewModel vm)
        {
            vm = new CampaignRegisterTreeVaultViewModel();
            try
            {
                BsonDocument result = CampaignBusinessLogic.GetVaultTreeForRegistration();
                var strresult = result.ToJson();
                vm.TreeVault = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }
            var response = Request.CreateResponse<CampaignRegisterTreeVaultViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetFormTemplate")]
        [HttpPost]
        public HttpResponseMessage GetFormTemplate(HttpRequestMessage request, CampaignRegisterTreeVaultViewModel vm)
        {
            vm = new CampaignRegisterTreeVaultViewModel();
            try
            {
                BsonDocument result = CampaignBusinessLogic.GetFormTemplate();
                var strresult = result.ToJson();
                vm.TreeVault = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<CampaignRegisterTreeVaultViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetPustToVaultTemplate")]
        [HttpPost]
        public HttpResponseMessage GetPustToVaultTemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result = CampaignBusinessLogic.GetCampaignTemplate("campaignPustToVaultTemplate");
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignAdvertisingTemplate")]
        [HttpPost]
        public HttpResponseMessage GetCampaignAdvertisingTemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result =
                    CampaignBusinessLogic.GetCampaignAdvertisingTemplate();
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.Urlpublic = ConfigHelp.GetStringValue("URLCampaignPublic") + "Advertising/" +
                                              BsonHelper.GenerateObjectIdString();
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignById")]
        [HttpPost]
        public HttpResponseMessage GetCampaignById(HttpRequestMessage request, CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result = CampaignBusinessLogic.GetCampaignById(campaignModelView.CampaignId);
                var strresult = result.ToJson();
                campaignModelView.Campaign = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);

                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("SetBoostAdvertising")]
        [HttpPost]
        public HttpResponseMessage SetBoostAdvertising(HttpRequestMessage request, CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                CampaignBusinessLogic.SetBoostAdvertising(campaignModelView.CampaignId);
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            // Write log
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You boosted an interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }


            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignHandshakeTemplate")]
        [HttpPost]
        public HttpResponseMessage GetCampaignHandshakeTemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result =
                    CampaignBusinessLogic.GetCampaignTemplate("campaignHandshakeTemplate");
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.Urlpublic = ConfigHelp.GetStringValue("URLCampaignPublic") + "Handshake/" +
                                              BsonHelper.GenerateObjectIdString();
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignSRFITemplate")]
        [HttpPost]
        public HttpResponseMessage GetCampaignSRFITemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result =
                    CampaignBusinessLogic.GetCampaignSRFITemplate();
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignRegistrationTemplate")]
        [HttpPost]
        public HttpResponseMessage GetCampaignRegistrationTemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result =
                    CampaignBusinessLogic.GetCampaignRegistrationTemplate();
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.Urlpublic = ConfigHelp.GetStringValue("URLCampaignPublic") +
                                              BsonHelper.GenerateObjectIdString();
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignEventTemplate")]
        [HttpPost]
        public HttpResponseMessage GetCampaignEventTemplate(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                BsonDocument result =
                    CampaignBusinessLogic.GetCampaignEventTemplate();
                var strresult = result.ToJson();
                campaignModelView.CampaignTemplateAdvertising =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                campaignModelView.Urlpublic = ConfigHelp.GetStringValue("URLCampaignPublic") + "Event/" +
                                              BsonHelper.GenerateObjectIdString();
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignsByUser")]
        [HttpPost]
        public IHttpActionResult GetCampaignsByUser(HttpRequestMessage request, CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (busAccount.AccountType == AccountType.Personal)
                {
                    var baAccount = _accountService.GetById(busAccount.BusinessAccountRoles[0].AccountId);
                    campaignModelView.ListRoles = _roleService.GetRolesOfAccount(busAccount, baAccount.Id);
                    campaignModelView.Isviewedit =
                        campaignModelView.ListRoles.Any(x => x.Name == Role.ROLE_EDITOR || x.Name == Role.ROLE_ADMIN);
                    campaignModelView.Isviewapproved = campaignModelView.ListRoles.Any(x =>
                        x.Name == Role.ROLE_REVIEWER || x.Name == Role.ROLE_ADMIN);

                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                var dataList = CampaignBusinessLogic.GetCampaignByBusinessUser(busAccount.AccountId,
                    campaignModelView.CampaignType, campaignModelView.CampaignStatus, campaignModelView.Isdraff,
                    campaignModelView.CurrentPageNumber, campaignModelView.PageSize, campaignModelView.Istemplate);
                if (dataList.DataOfCurrentPage.Count > 0)
                {
                    campaignModelView.TotalPages = dataList.TotalItems;
                    campaignModelView.Listitems = dataList.DataOfCurrentPage;
                }

                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            return Ok(campaignModelView);
        }

        [Route("GetCampaignsByUserNormal")]
        [HttpPost]
        public HttpResponseMessage GetCampaignsByUserNormal(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var listposthandshake = new PostHandShakeBusinessLogic().GetPostHandShakeByuserId(busAccount.AccountId);
                var listbusids = listposthandshake.Select(x => x.CampaignId).ToList();

                var dataList = CampaignBusinessLogic.GetCampaignByBusinessUser(busAccount.AccountId,
                    campaignModelView.CampaignType, campaignModelView.CampaignStatus, campaignModelView.Isdraff,
                    campaignModelView.CurrentPageNumber, int.MaxValue, campaignModelView.Istemplate, false, "", false,
                    false, false, listbusids, true);
                if (dataList.DataOfCurrentPage.Count > 0)
                {
                    campaignModelView.TotalPages = dataList.TotalItems;
                    campaignModelView.Listitems = dataList.DataOfCurrentPage;
                    for (int i = 0; i < campaignModelView.Listitems.Count; i++)
                    {
                        try
                        {
                            var camp = new CampaignBusinessLogic();
                            var checkEndDate = camp.CheckEndDateCampaign(campaignModelView.Listitems[i].CampaignId);
                        }
                        catch
                        {
                        }
                        campaignModelView.Listitems[i].postHandShake =
                            listposthandshake.FirstOrDefault(x => x.CampaignId == campaignModelView.Listitems[i].Id);
                        if (campaignModelView.Listitems[i].CampaignType == "Handshake")
                        {
                            var bus = _accountService.GetByAccountId(campaignModelView.Listitems[i].BusinessUserId);
                            campaignModelView.Listitems[i].BusinessName = string.IsNullOrEmpty(bus.Profile.DisplayName)
                                ? (bus.Profile.FirstName + " " + bus.Profile.LastName)
                                : bus.Profile.DisplayName;
                            campaignModelView.Listitems[i].BusinessAvatar = bus.Profile.PhotoUrl;
                        }
                    }
                }

                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetTerminateCampaignsByUserNormal")]
        [HttpPost]
        public HttpResponseMessage GetTerminateCampaignsByUserNormal(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var listposthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeTerminateByuserId(busAccount.AccountId);
                var listbusids = listposthandshake.Select(x => x.CampaignId).ToList();

                var dataList = CampaignBusinessLogic.GetCampaignByBusinessUser(busAccount.AccountId,
                    campaignModelView.CampaignType, campaignModelView.CampaignStatus, campaignModelView.Isdraff,
                    campaignModelView.CurrentPageNumber, int.MaxValue, campaignModelView.Istemplate, false, "", false,
                    false, false, listbusids, true);
                if (dataList.DataOfCurrentPage.Count > 0)
                {
                    campaignModelView.TotalPages = dataList.TotalItems;
                    campaignModelView.Listitems = dataList.DataOfCurrentPage;
                    for (int i = 0; i < campaignModelView.Listitems.Count; i++)
                    {
                        campaignModelView.Listitems[i].postHandShake =
                            listposthandshake.FirstOrDefault(x => x.CampaignId == campaignModelView.Listitems[i].Id);
                        if (campaignModelView.Listitems[i].CampaignType == "Handshake")
                        {
                            var bus = _accountService.GetByAccountId(campaignModelView.Listitems[i].BusinessUserId);
                            campaignModelView.Listitems[i].BusinessName = string.IsNullOrEmpty(bus.Profile.DisplayName)
                                ? (bus.Profile.FirstName + " " + bus.Profile.LastName)
                                : bus.Profile.DisplayName;
                            campaignModelView.Listitems[i].BusinessAvatar = bus.Profile.PhotoUrl;
                        }
                    }
                }

                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignsPushtovaultByUserNormal")]
        [HttpPost]
        public HttpResponseMessage GetCampaignsPushtovaultByUserNormal(HttpRequestMessage request,
            CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var dataList = CampaignBusinessLogic.GetCampaignByBusinessUser(busAccount.AccountId, "PushToVault",
                    campaignModelView.CampaignStatus, campaignModelView.Isdraff,
                    campaignModelView.CurrentPageNumber, int.MaxValue, campaignModelView.Istemplate, false, "", false,
                    false, false, null, true);
                if (dataList.DataOfCurrentPage.Count > 0)
                {
                    campaignModelView.TotalPages = dataList.TotalItems;
                    campaignModelView.Listitems = dataList.DataOfCurrentPage;
                }

                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("GetCampaignListByBusinessId")]
        [HttpPost]
        public HttpResponseMessage GetCampaignListByBusinessId(HttpRequestMessage request, CampaignViewModel vm)
        {
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            string businessId = vm.BusinessUserId;
            if (businessId == null)
            {
                businessId = account.AccountId.ToString();
                if (account.AccountType == AccountType.Personal)
                {
                    if (account.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = account.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            businessId = businessOject.AccountId;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(vm.keyword))
                vm.keyword = "";
            var campaignListAll = CampaignBusinessLogic
                .GetCampaignByBusinessUser(businessId, "All", "All", true, 1, 100, true, true, vm.keyword, false, true,
                    true).DataOfCurrentPage;
            var campaignList = campaignListAll.Where(x => x.BusinessUserId == businessId).ToList();
            var newFeedList = new List<NewFeedsViewModel>();

            foreach (var item in campaignList.OrderByDescending(x => x.Id))
            {
                var newfeed = new NewFeedsViewModel();
                newfeed.UserId = businessId;
                newfeed.AllowCreateQrCode = item.AllowCreateQrCode;
                newfeed.PublicURL = item.PublicURL;
                newfeed.CampaignId = item.Id;
                newfeed.CampaignType = item.CampaignType;
                newfeed.Name = item.CampaignName;
                newfeed.Description = item.Description;
                newfeed.termsAndConditionsFile = item.termsAndConditionsFile;
                newfeed.starttime = item.starttime;
                newfeed.startdate = item.startdate;
                newfeed.endtime = item.endtime;
                newfeed.enddate = item.enddate;
                newfeed.location = item.location;
                newfeed.theme = item.theme;

                newfeed.usercodecurrentcy = item.usercodecurrentcy;
                newfeed.usercodetype = item.usercodetype;
                newfeed.usercode = item.usercode;
                newfeed.BusinessUserId = item.BusinessUserId;
                newfeed.BusinessName = "";
                newfeed.BusinessImageUrl = "";
                newfeed.UserName = "";
                var businessItem = _accountService.GetByAccountId(item.BusinessUserId);
                if (businessItem != null)
                {
                    newfeed.BusinessName = businessItem.Profile.DisplayName;
                    newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                    newfeed.UserName = businessItem.Profile.DisplayName;
                }

                newfeed.MaxAge = item.MaxAge;
                newfeed.MinAge = item.MinAge;
                newfeed.Gender = item.Gender;

                newfeed.LocationType = item.LocationType;
                newfeed.CountryName = item.CountryName;
                newfeed.CityName = item.CityName;
                newfeed.TargetNetwork = item.TargetNetwork;
                newfeed.SpendMoney = item.SpendMoney;
                newfeed.SpendEffectDate = item.SpendEffectDate;
                newfeed.SpendEndDate = item.SpendEndDate;
                newfeed.ResidenceStatus = item.ResidenceStatus;
                newfeed.EstimatedReach = item.EstimatedReach;
                newfeed.Image = item.Image;
                newfeed.TargetLink = item.TargetLink;
                newfeed.Image = item.Image;
                newfeed.Fields = item.Fields;
                newFeedList.Add(newfeed);
            }

            vm.NewFeedsItemsList = newFeedList;

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetCampaignListByBusiness")]
        [HttpPost]
        public HttpResponseMessage GetCampaignListByBusiness(CampaignViewModel vm)
        {
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            string businessId = account.AccountId.ToString();
            if (account.AccountType == AccountType.Personal)
            {
                if (account.BusinessAccountRoles.Count > 0)
                {
                    var objectId = account.BusinessAccountRoles[0].AccountId;
                    if (objectId != null)
                    {
                        var businessOject = _accountService.GetById(objectId);
                        businessId = businessOject.AccountId;
                    }
                }
            }
            if (string.IsNullOrEmpty(vm.keyword))
                vm.keyword = "";
            var campaignListAll = CampaignBusinessLogic
                .GetCampaignByBusinessUser(businessId, "All", "All", true, 1, 100, true, true, vm.keyword, false, true,
                    true).DataOfCurrentPage;
            var campaignList = campaignListAll.Where(x => x.BusinessUserId == businessId).ToList();
            var newFeedList = new List<NewFeedsViewModel>();

            foreach (var item in campaignList.OrderByDescending(x => x.Id))
            {
                var newfeed = new NewFeedsViewModel();
                newfeed.UserId = businessId;
                newfeed.AllowCreateQrCode = item.AllowCreateQrCode;
                newfeed.PublicURL = item.PublicURL;
                newfeed.CampaignId = item.Id;
                newfeed.CampaignType = item.CampaignType;
                newfeed.Name = item.CampaignName;
                newfeed.Description = item.Description;
                newfeed.termsAndConditionsFile = item.termsAndConditionsFile;
                newfeed.starttime = item.starttime;
                newfeed.startdate = item.startdate;
                newfeed.endtime = item.endtime;
                newfeed.enddate = item.enddate;
                newfeed.location = item.location;
                newfeed.theme = item.theme;

                newfeed.usercodecurrentcy = item.usercodecurrentcy;
                newfeed.usercodetype = item.usercodetype;
                newfeed.usercode = item.usercode;
                newfeed.BusinessUserId = item.BusinessUserId;
                var businessItem = _accountService.GetByAccountId(item.BusinessUserId);
                if (businessItem != null)
                {
                    newfeed.BusinessName = businessItem.Profile.DisplayName;
                    newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                    newfeed.UserName = businessItem.Profile.DisplayName;
                }

                newfeed.MaxAge = item.MaxAge;
                newfeed.MinAge = item.MinAge;
                newfeed.Gender = item.Gender;

                newfeed.LocationType = item.LocationType;
                newfeed.CountryName = item.CountryName;
                newfeed.CityName = item.CityName;
                newfeed.TargetNetwork = item.TargetNetwork;

                newfeed.SpendMoney = item.SpendMoney;
                newfeed.SpendEffectDate = item.SpendEffectDate;
                newfeed.SpendEndDate = item.SpendEndDate;

                newfeed.ResidenceStatus = item.ResidenceStatus;
                newfeed.EstimatedReach = item.EstimatedReach;
                newfeed.Image = item.Image;
                newfeed.TargetLink = item.TargetLink;
                newfeed.Image = item.Image;

                newfeed.Fields = item.Fields;

                newFeedList.Add(newfeed);
            }

            vm.NewFeedsItemsList = newFeedList;

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetActiveCampaignForCurrentUser")]
        [HttpPost]
        public HttpResponseMessage GetActiveCampaignForCurrentUser(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (!ConfigHelp.GetBoolValue("IsEnableInteractionsInNewFeed"))
            {
                vm.NewFeedsItemsList = new List<NewFeedsViewModel>();
                var rs = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
                return rs;
            }
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());

            string userId = account.AccountId.ToString();
            BsonDocument result =
                eventBusinessLogic.GetEventTemplate();
            var strresult = result.ToJson();
            vm.EventTemplate = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
            string campaignpublicurl = "";
            if (!string.IsNullOrEmpty(vm.campaignpublicid))
                campaignpublicurl = ConfigHelp.GetStringValue("URLCampaignPublic") + vm.CampaignType + "/" +
                                    vm.campaignpublicid;
            var businessList = BusinessMemberLogic.GetFollowingBusinesses(userId).ToList();
            if (string.IsNullOrEmpty(vm.keyword))
            {
                vm.keyword = "";
            }
            string cmapigntype = String.IsNullOrEmpty(vm.CampaignType) ? "all" : vm.CampaignType;
            var campaignList =
                CampaignBusinessLogic.GetActiveCampaignsForUser(userId, campaignpublicurl, cmapigntype, true,
                    vm.keyword);
            var businessCampaignList = PostBusinessLogic.GetFollowedBusinessByUserId(userId).DataOfCurrentPage;
            var postList = PostBusinessLogic.GetPostListbyUserId(userId);
            var listhandshakefollow = new PostHandShakeBusinessLogic().Getlisthandshakefollow(userId);
            var newFeedList = new List<NewFeedsViewModel>();
            var register = false;
            var post = new PostBusinessLogic();


            foreach (var item in campaignList.OrderByDescending(x => x.Id))
            {
                register = false;
                var newfeed = new NewFeedsViewModel();
                try
                {
                    newfeed.UserId = userId;
                    newfeed.AllowCreateQrCode = item.AllowCreateQrCode;
                    newfeed.PublicURL = item.PublicURL;

                    newfeed.CampaignId = item.Id;
                    newfeed.CampaignType = item.CampaignType;
                    if (newfeed.CampaignType == "Registration")
                    {
                        try
                        {
                            var followers = post.GetFollowersByCampaignId(newfeed.CampaignId);
                            if (followers.Exists(value => value.UserId == newfeed.UserId))
                                register = true;
                        }
                        catch
                        {
                        }
                    }
                    newfeed.Register = register;

                    newfeed.Name = item.CampaignName;
                    newfeed.Description = item.Description;
                    newfeed.termsAndConditionsFile = item.termsAndConditionsFile;
                    //Event

                    newfeed.starttime = item.starttime;
                    newfeed.startdate = item.startdate;
                    newfeed.endtime = item.endtime;
                    newfeed.enddate = item.enddate;
                    newfeed.location = item.location;
                    newfeed.theme = item.theme;
                    newfeed.usercodecurrentcy = item.usercodecurrentcy;
                    newfeed.usercodetype = item.usercodetype;
                    newfeed.usercode = item.usercode;
                    newfeed.BusinessUserId = item.BusinessUserId;
                    var businessItem = _accountService.GetByAccountId(item.BusinessUserId);
                    newfeed.BusinessUserobjectId = businessItem.Id.ToString();

                    if (businessItem != null)
                    {
                        newfeed.BusinessName = businessItem.Profile.DisplayName;
                        newfeed.UserName = businessItem.Profile.DisplayName;
                        newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                        var membersOfCampaignList = PostBusinessLogic.GetFollowersList(item.Id);
                        if (membersOfCampaignList.DataOfCurrentPage != null)
                        {
                            newfeed.MembersOfBusiness = membersOfCampaignList.DataOfCurrentPage.ToList();
                            newfeed.MembersOfBusinessNbr = newfeed.MembersOfBusiness.Count;
                        }
                        newfeed.MembersOfBusinessNbr = membersOfCampaignList.TotalItems +
                                                       new PostHandShakeBusinessLogic()
                                                           .Getlisthandshakefollowcampaign(item.Id).Count;
                    }

                    newfeed.MaxAge = item.MaxAge;
                    newfeed.MinAge = item.MinAge;
                    newfeed.Gender = item.Gender;

                    newfeed.LocationType = item.LocationType;
                    newfeed.CountryName = item.CountryName;
                    newfeed.CityName = item.CityName;

                    newfeed.TargetNetwork = item.TargetNetwork;
                    newfeed.SpendMoney = item.SpendMoney;
                    newfeed.SpendEffectDate = item.SpendEffectDate;
                    newfeed.SpendEndDate = item.SpendEndDate;
                    newfeed.ResidenceStatus = item.ResidenceStatus;
                    newfeed.EstimatedReach = item.EstimatedReach;
                    newfeed.Image = item.Image;
                    newfeed.TargetLink = item.TargetLink;
                    newfeed.Image = item.Image;
                    newfeed.FollowerIds = item.FollowerIds;
                    newfeed.BusinessList2 = businessList;
                }
                catch
                {
                }
                if (newfeed != null)
                    newFeedList.Add(newfeed);
            }

            vm.NewFeedsItemsList = newFeedList;
            var strbusinessList = businessList.Select(item => item.Id).ToList();
            vm.BusinessIdList = strbusinessList;
            var strcampaignIdList = postList.Select(item => item.CampaignId).ToList();
            vm.CampaignIdInPostList = strcampaignIdList;
            strcampaignIdList.AddRange(listhandshakefollow);
            var strBusinessCampaignIdList = businessCampaignList.Select(item => item.UserId).ToList();
            vm.BusinessCampaignIdList = strBusinessCampaignIdList;

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetActiveCampaignbyCampaignId")]
        [HttpPost]
        public HttpResponseMessage GetActiveCampaignbyCampaignId(HttpRequestMessage request, CampaignViewModel vm)
        {
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = account.AccountId.ToString();
            var businessList = BusinessMemberLogic.GetFollowingBusinesses(userId).ToList();
            var campaignList = CampaignBusinessLogic.GetActiveCampaignsForUser(userId);
            var businessCampaignList = PostBusinessLogic.GetFollowedBusinessByUserId(userId).DataOfCurrentPage;
            var postList = PostBusinessLogic.GetPostListbyUserId(userId);
            var newFeedList = new List<NewFeedsViewModel>();

            foreach (var item in campaignList.OrderByDescending(x => x.Id))
            {
                var newfeed = new NewFeedsViewModel();
                try
                {
                    newfeed.UserId = userId;
                    newfeed.AllowCreateQrCode = item.AllowCreateQrCode;
                    newfeed.PublicURL = item.PublicURL;
                    newfeed.CampaignId = item.Id;
                    newfeed.CampaignType = item.CampaignType;
                    newfeed.Name = item.CampaignName;
                    newfeed.Description = item.Description;
                    newfeed.termsAndConditionsFile = item.termsAndConditionsFile;
                    newfeed.starttime = item.starttime;
                    newfeed.startdate = item.startdate;
                    newfeed.endtime = item.endtime;
                    newfeed.enddate = item.enddate;
                    newfeed.location = item.location;
                    newfeed.theme = item.theme;
                    newfeed.usercodecurrentcy = item.usercodecurrentcy;
                    newfeed.usercodetype = item.usercodetype;
                    newfeed.usercode = item.usercode;
                    newfeed.BusinessUserId = item.BusinessUserId;
                    var businessItem = _accountService.GetByAccountId(item.BusinessUserId);

                    if (businessItem != null)
                    {
                        newfeed.BusinessName = businessItem.Profile.DisplayName;
                        newfeed.UserName = businessItem.Profile.DisplayName;
                        newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                        var membersOfCampaignList = PostBusinessLogic.GetFollowersList(item.Id);
                        if (membersOfCampaignList.DataOfCurrentPage != null)
                        {
                            newfeed.MembersOfBusiness = membersOfCampaignList.DataOfCurrentPage.ToList();
                            newfeed.MembersOfBusinessNbr = newfeed.MembersOfBusiness.Count;
                        }
                        newfeed.MembersOfBusinessNbr = membersOfCampaignList.TotalItems;
                    }

                    newfeed.MaxAge = item.MaxAge;
                    newfeed.MinAge = item.MinAge;
                    newfeed.Gender = item.Gender;
                    newfeed.LocationType = item.LocationType;
                    newfeed.CountryName = item.CountryName;
                    newfeed.CityName = item.CityName;
                    newfeed.TargetNetwork = item.TargetNetwork;
                    newfeed.SpendMoney = item.SpendMoney;
                    newfeed.SpendEffectDate = item.SpendEffectDate;
                    newfeed.SpendEndDate = item.SpendEndDate;

                    newfeed.ResidenceStatus = item.ResidenceStatus;
                    newfeed.EstimatedReach = item.EstimatedReach;
                    newfeed.Image = item.Image;
                    newfeed.TargetLink = item.TargetLink;
                    newfeed.Image = item.Image;
                    newfeed.FollowerIds = item.FollowerIds;
                    newfeed.BusinessList2 = businessList;
                }
                catch
                {
                }
                if (newfeed != null)
                    newFeedList.Add(newfeed);
            }
            vm.NewFeedsItemsList = newFeedList.Where(x => x.CampaignId == vm.CampaignId).ToList();

            var strbusinessList = businessList.Select(item => item.Id).ToList();
            vm.BusinessIdList = strbusinessList;
            var strcampaignIdList = postList.Select(item => item.CampaignId).ToList();
            vm.CampaignIdInPostList = strcampaignIdList;
            var strBusinessCampaignIdList = businessCampaignList.Select(item => item.UserId).ToList();
            vm.BusinessCampaignIdList = strBusinessCampaignIdList;
            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [HttpGet, HttpPost, Route("GetCampaignInfor")]
        public HttpResponseMessage GetCampaignInfor(HttpRequestMessage request, string campaignId)
        {
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = account.AccountId.ToString();
            var businessList = BusinessMemberLogic.GetFollowingBusinesses(userId).ToList();
            var newfeed = new NewFeedsViewModel();
            var campaignInfor = CampaignBusinessLogic.GetCampaignInfor(campaignId);
            if (campaignInfor != null)
            {
                newfeed.UserId = userId;
                newfeed.CampaignId = campaignInfor.Id;
                newfeed.CampaignType = campaignInfor.CampaignType;
                newfeed.Name = campaignInfor.CampaignName;
                newfeed.Description = campaignInfor.Description;
                newfeed.BusinessUserId = campaignInfor.BusinessUserId;
                var businessItem = _accountService.GetByAccountId(campaignInfor.BusinessUserId);


                if (businessItem != null)
                {
                    newfeed.BusinessName = !string.IsNullOrEmpty(businessItem.Profile.DisplayName)
                        ? businessItem.Profile.DisplayName
                        : businessItem.Profile.FirstName + " " + businessItem.Profile.LastName;
                    newfeed.BusinessImageUrl = businessItem.Profile.PhotoUrl;
                }

                if (newfeed.CampaignType == "PushToVault" || newfeed.CampaignType == "ManualPushToVault")
                {
                    var response1 = Request.CreateResponse<NewFeedsViewModel>(HttpStatusCode.OK, newfeed);
                    return response1;
                }

                if (newfeed.CampaignType == "Handshake")
                {
                    newfeed.PostHandShake =
                        new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(userId,
                            newfeed.CampaignId);
                }

                if (businessItem != null)
                {
                    newfeed.UserName = businessItem.Profile.DisplayName;
                    var membersOfCampaignList = PostBusinessLogic.GetFollowersList(campaignInfor.Id);
                    if (membersOfCampaignList.DataOfCurrentPage != null)
                    {
                        newfeed.MembersOfBusiness = membersOfCampaignList.DataOfCurrentPage.ToList();
                        newfeed.MembersOfBusinessNbr = newfeed.MembersOfBusiness.Count;
                    }
                }
                newfeed.Timetype = campaignInfor.TimeType;
                newfeed.PublicURL = campaignInfor.PublicURL;
                newfeed.MaxAge = campaignInfor.MaxAge;
                newfeed.MinAge = campaignInfor.MinAge;
                newfeed.Gender = campaignInfor.Gender;
                newfeed.termsAndConditionsFile = campaignInfor.termsAndConditionsFile;
                newfeed.LocationType = campaignInfor.LocationType;
                newfeed.CountryName = campaignInfor.CountryName;
                newfeed.CityName = campaignInfor.CityName;
                newfeed.TargetNetwork = campaignInfor.TargetNetwork;
                newfeed.SpendMoney = campaignInfor.SpendMoney;
                newfeed.SpendEffectDate = campaignInfor.SpendEffectDate;
                newfeed.SpendEndDate = campaignInfor.SpendEndDate;

                newfeed.ResidenceStatus = campaignInfor.ResidenceStatus;
                newfeed.EstimatedReach = campaignInfor.EstimatedReach;
                newfeed.Image = campaignInfor.Image;
                newfeed.TargetLink = campaignInfor.TargetLink;
                newfeed.Image = campaignInfor.Image;
                newfeed.Fields = campaignInfor.Fields;
                newfeed.FollowerIds = campaignInfor.FollowerIds;
                newfeed.BusinessList2 = businessList;
            }

            var response = Request.CreateResponse<NewFeedsViewModel>(HttpStatusCode.OK, newfeed);
            return response;
        }

        [Route("GetUserInformationsExportAllHandshake")]
        [HttpPost]
        public HttpResponseMessage GetUserInformationsExportAllHandshake(HttpRequestMessage request,
            CampaignViewModel vm)
        {
            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            var campaignId = vm.CampaignId;

            List<PostHandShake> listhndshakes =
                new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(campaignId);
            var listfieldexports = new List<ExportAllHandshakeModel>();
            var pHs = new PostHandShakeBusinessLogic();
            foreach (var handshakein in listhndshakes.Where(h => h.IsJoin))
            {
                // handshakein.Fields
                var exportAllHandshakeModel = new ExportAllHandshakeModel();
                var userjoin = _accountService.GetByAccountId(handshakein.UserId);
                exportAllHandshakeModel.UserId = userjoin.AccountId.ToString();
                exportAllHandshakeModel.DisplayName = userjoin.Profile.DisplayName;
                try
                {
                    var listData =
                        infomationVaultBusinessLogic.getInformationvaultforcampaign(userjoin.AccountId.ToString(),
                            campaignId);
                    var arrListField =
                        pHs.GetArrayFieldsHandShakeByUserIdCamapignIdFull(handshakein.UserId, handshakein.CampaignId);
                    var newData = new List<FieldinformationVault>();
                    foreach (var item in arrListField)
                    {
                        for (var i = 0; i < listData.Count(); i++)
                        {
                            if (listData[i].jsPath == item["jsPath"].AsString)
                            {
                                newData.Add(listData[i]);
                            }
                        }
                    }
                    exportAllHandshakeModel.ListOfFields = newData;
                }
                catch
                {
                }
                try
                {
                    if (!string.IsNullOrEmpty(handshakein.JsondataOld))
                    {
                        exportAllHandshakeModel.ListOfFieldsOld =
                            JsonConvert.DeserializeObject<List<FieldinformationVault>>(handshakein.JsondataOld);
                        exportAllHandshakeModel.ListOfFieldsOld = exportAllHandshakeModel.ListOfFields.Select(x =>
                            exportAllHandshakeModel.ListOfFieldsOld.FirstOrDefault(y => y.jsPath == x.jsPath)).ToList();
                    }
                    else
                        exportAllHandshakeModel.ListOfFieldsOld = new List<FieldinformationVault>();
                }
                catch
                {
                    exportAllHandshakeModel.ListOfFieldsOld = new List<FieldinformationVault>();
                }
                exportAllHandshakeModel.DateUpdateJson = handshakein.DateUpdateJson;
                listfieldexports.Add(exportAllHandshakeModel);
            }

            // Write log export all handshake
            if (userAccount.AccountActivityLogSettings.RecordInteraction)
            {
                var act = new ActivityLogBusinessLogic();
                string title = userAccount.Profile.DisplayName + " export all your handshake.";

                act.WriteActivityLogFromAcc(userAccount.AccountId, title, "interactions", "");
            }
            vm.CampaignId = campaignId;
            vm.ExportAllHandshakeModels = listfieldexports;
            return this.Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
        }

        [Route("GetUserInformationForCampaign")]
        [HttpPost]
        public HttpResponseMessage GetUserInformationForCampaign(HttpRequestMessage request, CampaignViewModel vm)
        {
            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            var businessUserAccount = _accountService.GetByAccountId(vm.BusinessUserId);
            var camp = new CampaignBusinessLogic();
            if (string.IsNullOrEmpty(vm.UserId))
                vm.UserId = userAccount.AccountId;
            var campaignId = vm.CampaignId;
            var campaignType = vm.CampaignType;

            vm.ListOfFields = infomationVaultBusinessLogic.getInformationvaultforcampaign(vm.UserId, campaignId);
            // vm.ListOfFields = CampaignBusinessLogic.CampaignById(campaignId);

            IBusinessInteractionService interactionService = new BusinessInteractionService();
            var c = interactionService.GetInteraction(campaignId);
            if (c != null)
            {
                var groups = c["campaign"].AsBsonDocument.GetValue("groups", new BsonArray()).AsBsonArray.ToArray();
                if (groups.Length > 0)
                {
                    vm.Groups = new List<FieldGroup>();
                    foreach (var g in groups)
                    {
                        var name = g.AsBsonDocument.GetValue("name", BsonString.Empty).AsString;
                        var displayName = g.AsBsonDocument.GetValue("displayName", BsonString.Empty).AsString;
                        vm.Groups.Add(new FieldGroup { name = name, displayName = displayName });
                    }
                }
            }

            var postb = new PostBusinessLogic();

            if (campaignType.ToLower() == EnumCampaignType.Registration.ToLower())
            {
                vm.ListOfFields = camp.UpdateCampaignCustom(campaignId, vm.UserId, vm.ListOfFields);
            }
            vm.ListOfFields = postb.checkQA(vm.CampaignId, vm.UserId, vm.ListOfFields);
            if (campaignType == EnumCampaignType.HandShake)
            {
                var checkEndDate = camp.CheckEndDateCampaign(vm.CampaignId);
                if (!checkEndDate)
                {
                    var posthandshake =
                        new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(vm.UserId, campaignId);
                    if (posthandshake != null && posthandshake.IsJoin)
                    {
                        var lstFields =
                            JsonConvert.DeserializeObject<List<FieldinformationVault>>(posthandshake.Jsondata);
                        if (lstFields.Count > 0)
                            vm.ListOfFields = lstFields;
                        vm.ListOfFieldsOld =
                            JsonConvert.DeserializeObject<List<FieldinformationVault>>(posthandshake.JsondataOld);
                        vm.ListOfFieldsOld = vm.ListOfFields
                            .Select(x => vm.ListOfFieldsOld.FirstOrDefault(y => y.jsPath == x.jsPath)).ToList();
                        vm.handshakeupadte = posthandshake.DateUpdateJson;
                    }
                }
            }
            vm.BusinessUserId = businessUserAccount.AccountId;
            vm.CampaignId = campaignId;
            vm.CampaignType = campaignType;

            return this.Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
        }

        [Route("GetUserInformationForCampaignPushVault")]
        [HttpPost]
        public HttpResponseMessage GetUserInformationForCampaignPushVault(HttpRequestMessage request,
            CampaignViewModel vm)
        {
            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            var businessUserAccount = _accountService.GetByAccountId(vm.BusinessUserId);
            var campaignId = vm.CampaignId;
            var campaignType = vm.CampaignType;

            MongoRepository<Post> _postRepos = new MongoRepository<Post>();
            var listueridregis = _postRepos.Many(x => x.CampaignId == campaignId).FirstOrDefault().Followers
                .Where(y => y.UserId == businessUserAccount.AccountId)
                .Select(x => new DataRegisCampaign()
                {
                    datefollow = x.FollowedDate,
                    ListFieldsRegis = (x.fields == null ? new List<FieldinformationVault>() : x.fields.ToList())
                }).ToList();

            vm.ListOfFields = listueridregis.FirstOrDefault().ListFieldsRegis;

            for (int i = 0; i < vm.ListOfFields.Count; i++)
            {
                try
                {
                    vm.ListOfFields[i].options =
                        JsonConvert.DeserializeObject<List<string>>(vm.ListOfFields[i].modelarraysstr);
                    vm.ListOfFields[i].modelarrays =
                        JsonConvert.DeserializeObject<List<string>>(vm.ListOfFields[i].modelarraysstr);
                }
                catch
                {
                }
            }
            vm.BusinessUserId = businessUserAccount.AccountId;
            vm.CampaignId = campaignId;
            vm.CampaignType = campaignType;
            return this.Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
        }

        [Route("GetUserInformationForCampaignEmpty")]
        [HttpPost]
        public HttpResponseMessage GetUserInformationForCampaignEmpty(HttpRequestMessage request, CampaignViewModel vm)
        {
            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            var businessUserAccount = _accountService.GetByAccountId(userAccount.AccountId);
            var campaignId = vm.CampaignId;
            var campaignType = vm.CampaignType;
            vm.ListOfFields = infomationVaultBusinessLogic.getInformationvaultforcampaign(campaignId);
            vm.BusinessUserId = businessUserAccount.AccountId;
            vm.CampaignId = campaignId;
            vm.CampaignType = campaignType;
            return this.Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
        }

        [Route("GetCampaignRole")]
        [HttpPost]
        public HttpResponseMessage GetCampaignRole(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                var baAccount = account;
                if (account.AccountType == AccountType.Personal)
                {
                    baAccount = _accountService.GetById(account.BusinessAccountRoles[0].AccountId);
                    var roles = _roleService.GetRolesOfAccount(account, baAccount.Id)
                        .Where(s => s.Name == Role.ROLE_EDITOR).FirstOrDefault();
                    if (roles != null && roles.Name.Equals(Role.ROLE_EDITOR))
                        vm.IsActive = true;
                    else
                    {
                        vm.IsActive = false;
                    }
                }
                else
                {
                    vm.IsActive = true;
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "Update fail" }.ToList();
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("SaveCampaign")]
        [HttpPost]
        public HttpResponseMessage SaveCampaign(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                CampaignBusinessLogic.SaveCampaign(vm.CampaignId, vm.StrCampaignAdvertising);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }
            // Write log
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You updated advertising interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }


        [Route("UserNormalInsertCampaignAdvertising")]
        [HttpPost]
        public HttpResponseMessage UserNormalInsertCampaignAdvertising(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                string userId = busAccount.AccountId.ToString();
                BsonDocument result =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrCampaignAdvertising);
                vm.CampaignId = CampaignBusinessLogic.InsertCampaign(busAccount.AccountId, result);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }
            // Write log
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You created a new interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("UserNormalSaveCampaign")]
        [HttpPost]
        public HttpResponseMessage UserNormalSaveCampaign(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                CampaignBusinessLogic.SaveCampaign(vm.CampaignId, vm.StrCampaignAdvertising);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            // Write log
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You updated advertising interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("InsertCampaignAdvertising")]
        [HttpPost]
        public HttpResponseMessage InsertCampaignAdvertising(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                string userId = busAccount.AccountId.ToString();
                if (busAccount.AccountType == AccountType.Personal)
                {
                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                BsonDocument result =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrCampaignAdvertising);
                CampaignBusinessLogic.InsertCampaign(busAccount.AccountId, result);

                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            // Write log
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You created a new interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("InsertCampaignRegistration")]
        [HttpPost]
        public HttpResponseMessage InsertCampaignRegistration(HttpRequestMessage request, CampaignViewModel vm)
        {
            if (vm == null)
                vm = new CampaignViewModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                string userId = busAccount.AccountId.ToString();
                if (busAccount.AccountType == AccountType.Personal)
                {
                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                BsonDocument result =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrCampaignAdvertising);
                CampaignBusinessLogic.InsertCampaign(busAccount.AccountId, result);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            //WRITE LOG
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordCampaign)
            {
                string title = "You created a new registration interaction.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("DeleteCampaign")]
        [HttpPost]
        public HttpResponseMessage DeleteCampaign(HttpRequestMessage request, CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                CampaignBusinessLogic.DeleteCampaign(campaignModelView.CampaignId);
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("RemoveCampaign")]
        [HttpPost]
        public HttpResponseMessage RemoveCampaign(HttpRequestMessage request, CampaignViewModel campaignModelView)
        {
            if (campaignModelView == null)
                campaignModelView = new CampaignViewModel();
            try
            {
                CampaignBusinessLogic.RemoveCampaign(campaignModelView.CampaignId);
                campaignModelView.ReturnStatus = true;
                campaignModelView.ReturnMessage = new string[] { "successfully" }.ToList();
            }
            catch
            {
                campaignModelView.ReturnStatus = false;
                campaignModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<CampaignViewModel>(HttpStatusCode.OK, campaignModelView);
            return response;
        }

        [Route("UploadFileRegisform")]
        public async Task<HttpResponseMessage> UploadFileRegisform(string jsPath = null)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = GetMultipartProviderRegisform();
            var result = await Request.Content.ReadAsMultipartAsync(provider);
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = currentUser.AccountId.ToString();
            if (!string.IsNullOrEmpty(jsPath))
            {
                try
                {
                    string[] s = Regex.Split(jsPath, @"/");
                    var newUserId = s[s.Length - 2];
                    if (!string.IsNullOrEmpty(newUserId))
                        userId = newUserId;
                }
                catch
                {
                }
            }
            string fileName = "";
            var InfomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
            var uploadFolder = "~/" + ConfigHelp.GetStringValue("folderVault") + userId;
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
                fileName = fileName.Replace("[", "").Replace("\\", "").Replace("\"", "").Replace("]", "");

                // string extension = Path.GetExtension(fileName);


                if (!string.IsNullOrEmpty(jsPath))
                {
                    // / Content / vault / documents / 52fcb06f - 15e2 - 4a58 - bc3c - d90f0e9e4d50 /
                }
                var directory = new DirectoryInfo(HostingEnvironment.MapPath(uploadFolder));
                if (!directory.Exists)
                {
                    directory.Create();
                }
                // check set file
                try
                {
                    var doc = new Document();
                    doc.SaveName = fileName;
                    doc.Category = "Custom";
                    doc.FileName = "Custom" + fileName;
                    doc.Path = "Custom";

                    doc.UploadDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    doc.ExpiredDate = "";
                    doc.NoSearch = false;

                    var docrs = InfomationVaultBusinessLogic.InsertDocumentFieldByAccountId(userId, doc);
                    if (docrs != null)
                    {
                        fileName = docrs.SaveName;
                    }
                }
                catch
                {
                }

                var StoragePath = HttpContext.Current.Server.MapPath(uploadFolder);
                if (File.Exists(Path.Combine(StoragePath, fileName)))
                    File.Delete(Path.Combine(StoragePath, fileName));
                File.Move(fileData.LocalFileName, Path.Combine(StoragePath, fileName));
            }
            return this.Request.CreateResponse(HttpStatusCode.OK,
                new { fileName = fileName, filepath = "/Content/vault/documents/" + userId });
        }

        [Route("UploadImage")]
        public async Task<HttpResponseMessage> UploadImage()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = GetMultipartProvider();
            var result = await Request.Content.ReadAsMultipartAsync(provider);
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = currentUser.AccountId.ToString();
            var m = userId;
            string fileName = "";
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
                fileName = m + "Campaign" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                var uploadFolder = "~/Content/UploadImages"; // you could put this to web.config
                var StoragePath = HttpContext.Current.Server.MapPath(uploadFolder);
                if (File.Exists(Path.Combine(StoragePath, fileName)))
                    File.Delete(Path.Combine(StoragePath, fileName));

                File.Move(fileData.LocalFileName, Path.Combine(StoragePath, fileName));
            }
            fileName = "/Content/UploadImages/" + fileName;

            return this.Request.CreateResponse(HttpStatusCode.OK, new { fileName });
        }

        [Route("RegisterCampaignForBus")]
        [HttpPost]
        public HttpResponseMessage RegisterCampaignForBus(HttpRequestMessage request, VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (userAccount.AccountType != AccountType.Business)
                {
                    var businessAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(userAccount.Id)
                        .FirstOrDefault();
                    if (businessAccount != null)
                    {
                        userAccount = businessAccount;
                    }
                }
                ObjectId usernormalid = ObjectId.Empty;
                var usernotifycation = new Account();
                vm.ReturnStatus = true;
                if (vm.pushtype == "email")
                {
                    if (!string.IsNullOrEmpty(vm.usernotifycationemail))
                    {
                        var accouuntpushtovault = _accountService.GetByEmail(vm.usernotifycationemail);
                        if (accouuntpushtovault != null && accouuntpushtovault.AccountType == AccountType.Personal)
                        {
                            usernormalid = accouuntpushtovault.Id;
                        }
                        else
                        {
                            vm.ReturnStatus = false;
                            vm.ReturnMessage = new string[] { " email not exits " }.ToList();
                            goto done;
                        }
                    }
                    else
                    {
                        vm.ReturnStatus = false;
                        vm.ReturnMessage = new string[] { " email is empty " }.ToList();
                        goto done;
                    }
                }
                else
                {
                    if (userAccount.AccountType == AccountType.Business)
                        usernormalid = ObjectId.Parse(vm.usernotifycationid);
                }
                if (string.IsNullOrEmpty(vm.CampaignId))
                    vm.CampaignId = BsonHelper.GenerateObjectIdString();
                PostBusinessLogic.RegisterCampaign(userAccount, userAccount, vm.CampaignId, vm.CampaignType, "",
                    vm.Listvaults);
                usernotifycation = _accountService.GetById(usernormalid);
                if (usernotifycation == null)
                    usernotifycation = _accountService.GetByAccountId(vm.usernotifycationid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyPushToVault;
                notificationMessage.FromAccountId = userAccount.AccountId;
                notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = usernotifycation.AccountId;
                notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                notificationMessage.PreserveBag = vm.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "Update fail " + ex.ToString() }.ToList();
            }
        done:
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("ManualPushVault")]
        [HttpPost]
        public HttpResponseMessage ManualPushVault(HttpRequestMessage request, VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                ObjectId usernormalid = ObjectId.Empty;
                var usernotifycation = new Account();
                vm.ReturnStatus = true;
                if (vm.pushtype == "email")
                {
                    if (!string.IsNullOrEmpty(vm.usernotifycationemail))
                    {
                        var accouuntpushtovault = _accountService.GetByEmail(vm.usernotifycationemail);
                        if (accouuntpushtovault != null && accouuntpushtovault.AccountType == AccountType.Personal)
                        {
                            usernormalid = accouuntpushtovault.Id;
                        }
                        else
                        {
                            vm.ReturnStatus = false;
                            vm.ReturnMessage = new string[] { " email not exits " }.ToList();
                            goto done;
                        }
                    }
                    else
                    {
                        vm.ReturnStatus = false;
                        vm.ReturnMessage = new string[] { " email is empty " }.ToList();
                        goto done;
                    }
                }
                else
                {
                    if (userAccount.AccountType == AccountType.Business)
                        usernormalid = ObjectId.Parse(vm.usernotifycationid);
                }
                //Vu 
                if (string.IsNullOrEmpty(vm.CampaignId))
                    vm.CampaignId = BsonHelper.GenerateObjectIdString();
                BsonDocument result =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrVaultInformation);
                vm.CampaignId = CampaignBusinessLogic.InsertCampaign(userAccount.AccountId, result);
                PostBusinessLogic.RegisterCampaign(userAccount, userAccount, vm.CampaignId, vm.CampaignType, "",
                    vm.Listvaults);
                usernotifycation = _accountService.GetById(usernormalid);
                if (usernotifycation == null)
                    usernotifycation = _accountService.GetByAccountId(vm.usernotifycationid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyPushToVault;
                notificationMessage.FromAccountId = userAccount.AccountId;
                notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = usernotifycation.AccountId;
                notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                notificationMessage.PreserveBag = vm.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "Update fail " + ex.ToString() }.ToList();
            }
        done:
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }


        [Route("RegisterCampaign")]
        [HttpPost]
        public HttpResponseMessage RegisterCampaign(HttpRequestMessage request, VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            var camp = new CampaignBusinessLogic();
            var checkEndDate = camp.CheckEndDateCampaign(vm.CampaignId);
            if (checkEndDate)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.Forbidden,
                        "No participation allowed for expired interaction"));
            }

            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                var currentAccount = userAccount;
                var businessAccount = _accountService.GetByAccountId(vm.BusinessUserId);
                if (!string.IsNullOrEmpty(vm.UserId))
                {
                    try
                    {
                        userAccount = _accountService.GetByAccountId(vm.UserId);
                    }
                    catch
                    {
                    }
                }

                var userList =
                    BusinessMemberLogic.GetMembersOfBusiness(businessAccount.AccountId, EnumFollowType.Following);
                List<FieldinformationVault> listOfFields = new List<FieldinformationVault>();

                if (userList != null &&
                    userList.ToList().Where(x => x.UserId == userAccount.AccountId).ToList().Count > 0)
                {
                    if (vm.CampaignType == "Handshake")
                    {
                        try
                        {
                            listOfFields =
                                infomationVaultBusinessLogic.getInformationvaultforcampaign(userAccount.AccountId,
                                    vm.CampaignId);
                        }
                        catch
                        {
                        }
                        new PostHandShakeBusinessLogic().UserUnjoinorjoinHandshake(vm.CampaignId, userAccount.AccountId,
                            false);
                        Account userfrom = userAccount;
                        Account userto = businessAccount;
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = EnumNotificationType.NotifyJoinHandshake;
                        notificationMessage.FromAccountId = userfrom.AccountId;
                        notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                        notificationMessage.ToAccountId = userto.AccountId;
                        notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                        notificationMessage.PreserveBag = vm.CampaignId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                        new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(userAccount.AccountId, true);
                    }
                    else
                    {
                        PostBusinessLogic.RegisterCampaign(userAccount, businessAccount, vm.CampaignId, vm.CampaignType,
                            "", vm.Listvaults, vm.DelegationId, vm.DelegateeId);
                    }
                }
                else
                {
                    new AccountService().FollowBusiness(userAccount.Id, businessAccount.Id);
                    BusinessMemberLogic.AddBusinessMember(userAccount, businessAccount);
                    if (vm.CampaignType == "Handshake")
                    {
                        try
                        {
                            listOfFields =
                                infomationVaultBusinessLogic.getInformationvaultforcampaign(userAccount.AccountId,
                                    vm.CampaignId);
                        }
                        catch
                        {
                        }

                        new PostHandShakeBusinessLogic().UserUnjoinorjoinHandshake(vm.CampaignId, userAccount.AccountId,
                            false);
                        Account userfrom = userAccount;
                        Account userto = businessAccount;
                        var notificationMessage = new NotificationMessage();
                        notificationMessage.Id = ObjectId.GenerateNewId();
                        notificationMessage.Type = EnumNotificationType.NotifyJoinHandshake;
                        notificationMessage.FromAccountId = userfrom.AccountId;
                        notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                        notificationMessage.ToAccountId = userto.AccountId;
                        notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                        notificationMessage.PreserveBag = vm.CampaignId;
                        var notificationBus = new NotificationBusinessLogic();
                        notificationBus.SendNotification(notificationMessage);
                        new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(userAccount.AccountId, true);
                    }
                    else

                    {
                        PostBusinessLogic.RegisterCampaign(userAccount, businessAccount, vm.CampaignId, vm.CampaignType,
                            "", vm.Listvaults, vm.DelegationId, vm.DelegateeId);
                    }
                }

                //Log: Register Campaign when click on newsfeed
                if (currentAccount.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    string title = "You register Campaign " + businessAccount.Profile.DisplayName;
                    if (currentAccount.AccountId != userAccount.AccountId)
                        title = "You register Campaign " + businessAccount.Profile.DisplayName + " for " +
                                userAccount.Profile.DisplayName;

                    act.WriteActivityLogFromAcc(currentAccount.AccountId, title, "interactions",
                        businessAccount.AccountId);
                }
                if (!string.IsNullOrEmpty(vm.StrEvent) && vm.CampaignType == "Event")
                {
                    BsonDocument result =
                        MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrEvent);
                    eventBusinessLogic.InsertEventwithcampaign(userAccount.AccountId, result, vm.CampaignId);
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "Update fail" }.ToList();
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("RegisterCampaignToHandShake")]
        [HttpPost]
        public HttpResponseMessage RegisterCampaignToHandShake(HttpRequestMessage request, VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            //
            var camp = new CampaignBusinessLogic();
            var checkEndDate = camp.CheckEndDateCampaign(vm.CampaignId);
            if (!checkEndDate)
            {
                try
                {
                    var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                    var currentAccount = userAccount;
                    var businessAccount = _accountService.GetByAccountId(vm.BusinessUserId);

                    if (!string.IsNullOrEmpty(vm.UserId))
                    {
                        try
                        {
                            userAccount = _accountService.GetByAccountId(vm.UserId);
                        }
                        catch
                        {
                        }
                    }

                    var userList =
                        BusinessMemberLogic.GetMembersOfBusiness(businessAccount.AccountId, EnumFollowType.Following);
                    List<FieldinformationVault> listOfFields = new List<FieldinformationVault>();
                    if (userList != null &&
                        userList.ToList().Where(x => x.UserId == userAccount.AccountId).ToList().Count > 0)
                    {
                        if (vm.CampaignType == "Handshake")
                        {
                            try
                            {
                                listOfFields =
                                    infomationVaultBusinessLogic.getInformationvaultforcampaign(userAccount.AccountId,
                                        vm.CampaignId);
                            }
                            catch
                            {
                            }
                            new PostHandShakeBusinessLogic().UserUnjoinorjoinHandshakeListField(vm.CampaignId,
                                userAccount.AccountId, false, vm.Listvaults);
                            Account userfrom = userAccount;
                            Account userto = businessAccount;
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyJoinHandshake;
                            notificationMessage.FromAccountId = userfrom.AccountId;
                            notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                            notificationMessage.ToAccountId = userto.AccountId;
                            notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                            notificationMessage.PreserveBag = vm.CampaignId;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                            new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(userAccount.AccountId, true);
                        }
                        else
                        {
                            PostBusinessLogic.RegisterCampaign(userAccount, businessAccount, vm.CampaignId,
                                vm.CampaignType, "", vm.Listvaults);
                        }
                    }
                    else
                    {
                        new AccountService().FollowBusiness(userAccount.Id, businessAccount.Id);
                        BusinessMemberLogic.AddBusinessMember(userAccount, businessAccount);
                        if (vm.CampaignType == "Handshake")
                        {
                            try
                            {
                                listOfFields =
                                    infomationVaultBusinessLogic.getInformationvaultforcampaign(userAccount.AccountId,
                                        vm.CampaignId);
                            }
                            catch
                            {
                            }

                            new PostHandShakeBusinessLogic().UserUnjoinorjoinHandshake(vm.CampaignId,
                                userAccount.AccountId, false);
                            Account userfrom = userAccount;
                            Account userto = businessAccount;
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyJoinHandshake;
                            notificationMessage.FromAccountId = userfrom.AccountId;
                            notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                            notificationMessage.ToAccountId = userto.AccountId;
                            notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                            // notificationMessage.Content = delegationMessage.Message;
                            notificationMessage.PreserveBag = vm.CampaignId;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                            //if (vm.isEdit)
                            new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(userAccount.AccountId, true);
                        }
                        else

                        {
                            PostBusinessLogic.RegisterCampaign(userAccount, businessAccount, vm.CampaignId,
                                vm.CampaignType, "", vm.Listvaults);
                        }
                    }

                    //Log: Register Campaign when click on newsfeed
                    if (currentAccount.AccountActivityLogSettings.RecordInteraction)
                    {
                        var act = new ActivityLogBusinessLogic();
                        string title = "You register Campaign " + businessAccount.Profile.DisplayName;
                        if (currentAccount.AccountId != userAccount.AccountId)
                            title = "You register Campaign " + businessAccount.Profile.DisplayName + " for " +
                                    userAccount.Profile.DisplayName;

                        act.WriteActivityLogFromAcc(currentAccount.AccountId, title, "interactions",
                            businessAccount.AccountId);
                    }
                    if (!string.IsNullOrEmpty(vm.StrEvent) && vm.CampaignType == "Event")
                    {
                        BsonDocument result =
                            MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(vm.StrEvent);
                        eventBusinessLogic.InsertEventwithcampaign(userAccount.AccountId, result, vm.CampaignId);
                    }
                }
                catch
                {
                    vm.ReturnStatus = false;
                    vm.ReturnMessage = new string[] { "Update fail" }.ToList();
                }
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("DeRegisterCampaign")]
        [HttpPost]
        public HttpResponseMessage DeRegisterCampaign(HttpRequestMessage request, VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (!string.IsNullOrEmpty(vm.UserId))
                {
                    userAccount = _accountService.GetByAccountId(vm.UserId);
                }
                var businessAccount = _accountService.GetByAccountId(vm.BusinessUserId);
                if (vm.CampaignType == "Handshake")
                {
                    new PostHandShakeBusinessLogic().DeletePostHandshake(vm.CampaignId, userAccount.AccountId);
                }
                else
                {
                    PostBusinessLogic.DeRegisterCampaign(userAccount, businessAccount, vm.CampaignId, vm.DelegateeId);
                }
                if (userAccount.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    string title = "You deregistered with " + businessAccount.Profile.DisplayName;
                    act.WriteActivityLogFromAcc(userAccount.AccountId, title, "interactions",
                        businessAccount.AccountId);
                }
                eventBusinessLogic.DeleteEventfromcampaign(vm.CampaignId);
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "Update fail" }.ToList();
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        private object GetFormData<T>(MultipartFormDataStreamProvider result)
        {
            if (result.FormData.HasKeys())
            {
                var unescapedFormData =
                    Uri.UnescapeDataString(result.FormData.GetValues(0).FirstOrDefault() ?? String.Empty);
                if (!String.IsNullOrEmpty(unescapedFormData))
                    return JsonConvert.DeserializeObject<T>(unescapedFormData);
            }

            return null;
        }

        [HttpPost]
        private MultipartFormDataStreamProvider GetMultipartProvider()
        {
            var uploadFolder = "~/";
            var root = HttpContext.Current.Server.MapPath(uploadFolder);
            Directory.CreateDirectory(root + "..\\GH.Web\\Content\\UploadImages");
            return new MultipartFormDataStreamProvider(root);
        }

        [HttpPost]
        private MultipartFormDataStreamProvider GetMultipartProviderRegisform()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = currentUser.AccountId.ToString();
            var root = rootFolder;
            return new MultipartFormDataStreamProvider(root);
        }

        #region V2

        //UserIdByCampaignId
        [Route("UserIdByCampaignId")]
        [HttpGet]
        public HttpResponseMessage UserIdByCampaignId(string campaignId)
        {
            try
            {
                var campaign = new CampaignBusinessLogic();
                var rs = campaign.UserIdByCampaignId(campaignId);
                var response = Request.CreateResponse<string>(HttpStatusCode.OK, rs);
                return response;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        [Route("CampaignById")]
        [HttpGet]
        public HttpResponseMessage CampaignById(string campaignId)
        {
            var rs = new CampaignVM();

            try
            {
                rs = CampaignBusinessLogic.CampaignById(campaignId);
                var acc = _accountService.GetByAccountId(rs.UserId);
                rs.DisplayName = acc.Profile.DisplayName;
                rs.Avatar = acc.Profile.PhotoUrl;
            }
            catch (Exception ex)
            {
            }

            var response = Request.CreateResponse<CampaignVM>(HttpStatusCode.OK, rs);
            return response;
        }

        #endregion V2

        #region PostHandShake

        [HttpPost]
        [Route("GetUserInfomationfromahndshakeid")]
        public HttpResponseMessage GetUserInfomationfromahndshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (busAccount.AccountType == AccountType.Personal)
                {
                    var baAccount = _accountService.GetById(busAccount.BusinessAccountRoles[0].AccountId);
                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                var handshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
                        postHandShakeModel.CampaignId);
                if (handshake != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(handshake.Jsondata))
                            postHandShakeModel.listinformations =
                                JsonConvert.DeserializeObject<List<FieldinformationVault>>(handshake.Jsondata);
                        else
                            postHandShakeModel.listinformations = new List<FieldinformationVault>();
                    }
                    catch
                    {
                        postHandShakeModel.listinformations = new List<FieldinformationVault>();
                    }
                }

                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("GetPostHandShakeByCamapignId")]
        public HttpResponseMessage GetPostHandShakeByCamapignId(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var camp = new CampaignBusinessLogic();
                var checkEndDate = camp.CheckEndDateCampaign(postHandShakeModel.CampaignId);

                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (busAccount.AccountType == AccountType.Personal)
                {
                    var baAccount = _accountService.GetById(busAccount.BusinessAccountRoles[0].AccountId);
                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                var dataList =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(postHandShakeModel.CampaignId);
                if (dataList.Count > 0)
                {
                    postHandShakeModel.List = dataList;
                }

                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("GetPostHandShakeTerminateByCamapignId")]
        public HttpResponseMessage GetPostHandShakeTerminateByCamapignId(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                if (busAccount.AccountType == AccountType.Personal)
                {
                    var baAccount = _accountService.GetById(busAccount.BusinessAccountRoles[0].AccountId);
                    if (busAccount.BusinessAccountRoles.Count > 0)
                    {
                        var objectId = busAccount.BusinessAccountRoles[0].AccountId;
                        if (objectId != null)
                        {
                            var businessOject = _accountService.GetById(objectId);
                            busAccount.AccountId = businessOject.AccountId;
                        }
                    }
                }

                var dataListTerminate =
                    new PostHandShakeBusinessLogic().GetPostHandShakeTerminateByCamapignId(
                        postHandShakeModel.CampaignId);


                if (dataListTerminate.Count > 0)
                {
                    postHandShakeModel.List = dataListTerminate;
                }

                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("UserUnjoinorjoinHandshake")]
        public HttpResponseMessage UserUnjoinorjoinHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

                Account userfrom = busAccount;
                Account userto = new Account();
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
                        postHandShakeModel.CampaignId);

                if (busAccount.AccountType == AccountType.Personal)
                    userto = _accountService.GetByAccountId(posthandshake.BusId);
                else
                    userto = _accountService.GetByAccountId(postHandShakeModel.Userid);

                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = postHandShakeModel.IsJoin == false
                    ? EnumNotificationType.NotifyResumeHandshake
                    : EnumNotificationType.NotifyPauseHandshake;
                notificationMessage.FromAccountId = userfrom.AccountId;
                notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                notificationMessage.ToAccountId = userto.AccountId;
                notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                // notificationMessage.Content = delegationMessage.Message;
                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                new PostHandShakeBusinessLogic().UserUnjoinorjoinHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid, postHandShakeModel.IsJoin);
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("TerminatePostHandshake")]
        public HttpResponseMessage TerminatePostHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                // var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
                        postHandShakeModel.CampaignId);

                var userfrom = _accountService.GetByAccountId(posthandshake.BusId);

                var userto = _accountService.GetByAccountId(postHandShakeModel.Userid);

                new PostHandShakeBusinessLogic().TerminatePostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyTerminateHandshake;
                notificationMessage.FromAccountId = userfrom.AccountId;
                notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                notificationMessage.ToAccountId = userto.AccountId;
                notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                var dataList =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(postHandShakeModel.CampaignId);
                if (dataList.Count > 0)
                {
                    postHandShakeModel.List = dataList;
                }
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [Route("TerminatePostHandshakeUser")]
        public HttpResponseMessage TerminatePostHandshakeUser(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                // var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
                        postHandShakeModel.CampaignId);
                var userto = _accountService.GetByAccountId(posthandshake.BusId);

                var userfrom = _accountService.GetByAccountId(postHandShakeModel.Userid);


                //
                //Business
                //if (userAccount.AccountType != AccountType.Business)
                //{
                //    var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                //    userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id).FirstOrDefault();
                //}
                //
                new PostHandShakeBusinessLogic().TerminatePostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyTerminateHandshake;
                notificationMessage.FromAccountId = userfrom.AccountId;
                notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                notificationMessage.ToAccountId = userto.AccountId;
                notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                var dataList =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(postHandShakeModel.CampaignId);
                if (dataList.Count > 0)
                {
                    postHandShakeModel.List = dataList;
                }
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("DeletePostHandshake")]
        public HttpResponseMessage DeletePostHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                //  var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignidForDelete(
                        postHandShakeModel.Userid, postHandShakeModel.CampaignId);
                var userfrom = _accountService.GetByAccountId(posthandshake.BusId);
                var userto = _accountService.GetByAccountId(postHandShakeModel.Userid);

                new PostHandShakeBusinessLogic().DeletePostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid);
                //var notificationMessage = new NotificationMessage();
                //notificationMessage.Id = ObjectId.GenerateNewId();
                //notificationMessage.Type = EnumNotificationType.NotifyTerminateHandshake;
                //notificationMessage.FromAccountId = userfrom.AccountId;
                //notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                //notificationMessage.ToAccountId = userto.AccountId;
                //notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                //notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                //var notificationBus = new NotificationBusinessLogic();
                //notificationBus.SendNotification(notificationMessage);
                var dataList =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(postHandShakeModel.CampaignId);
                if (dataList.Count > 0)
                {
                    postHandShakeModel.List = dataList;
                }
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("DeletePostHandshakeUser")]
        public HttpResponseMessage DeletePostHandshakeUser(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignidForDelete(
                        postHandShakeModel.Userid, postHandShakeModel.CampaignId);
                var userto = _accountService.GetByAccountId(posthandshake.BusId);

                var userfrom = _accountService.GetByAccountId(postHandShakeModel.Userid);

                new PostHandShakeBusinessLogic().DeletePostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid);
                //var notificationMessage = new NotificationMessage();
                //notificationMessage.Id = ObjectId.GenerateNewId();
                //notificationMessage.Type = EnumNotificationType.NotifyTerminateHandshake;
                //notificationMessage.FromAccountId = userfrom.AccountId;
                //notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                //notificationMessage.ToAccountId = userto.AccountId;
                //notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                //notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                //var notificationBus = new NotificationBusinessLogic();
                //notificationBus.SendNotification(notificationMessage);
                var dataList =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByCamapignId(postHandShakeModel.CampaignId);
                if (dataList.Count > 0)
                {
                    postHandShakeModel.List = dataList;
                }
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("BusReInvitedMemberHandshake")]
        public HttpResponseMessage BusReInvitedMemberHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                // Check business
                if (userAccount.AccountType != AccountType.Business)
                {
                    var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                    userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                        .FirstOrDefault();
                }
                var usernotifycation = _accountService.GetByAccountId(postHandShakeModel.userinvitedid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
                notificationMessage.FromAccountId = userAccount.AccountId;
                notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = usernotifycation.AccountId;
                notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                notificationMessage.Content = postHandShakeModel.posthandshakecomment;
                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("BusInvitedMemberHandshake")]
        public HttpResponseMessage BusInvitedMemberHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (!string.IsNullOrEmpty(postHandShakeModel.Userid))
                    userAccount = _accountService.GetByAccountId(postHandShakeModel.Userid);
                //Business
                if (userAccount.AccountType != AccountType.Business)
                {
                    var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                    userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                        .FirstOrDefault();
                }


                ObjectId usernormalid = ObjectId.Empty;
                var usernotifycation = new Account();
                postHandShakeModel.ReturnStatus = true;
                if (postHandShakeModel.invitetype == "email")
                {
                    if (!string.IsNullOrEmpty(postHandShakeModel.userinvitedemail))
                    {
                        var accouuntpushtovault = _accountService.GetByEmail(postHandShakeModel.userinvitedemail);
                        if (accouuntpushtovault != null && accouuntpushtovault.AccountType == AccountType.Personal)
                        {
                            usernormalid = accouuntpushtovault.Id;
                        }
                        else
                        {
                            postHandShakeModel.ReturnStatus = false;
                            postHandShakeModel.ReturnMessage = new string[] { " email not exits " }.ToList();
                            goto done;
                        }
                    }
                    else
                    {
                        postHandShakeModel.ReturnStatus = false;
                        postHandShakeModel.ReturnMessage = new string[] { " email is empty " }.ToList();
                        goto done;
                    }
                }
                else
                {
                    if (userAccount.AccountType == AccountType.Business)
                        usernormalid = ObjectId.Parse(postHandShakeModel.userinvitedid);
                }
                usernotifycation = _accountService.GetById(usernormalid);
                new PostHandShakeBusinessLogic().InsertPostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.posthandshakecomment, userAccount.AccountId, usernotifycation.AccountId,
                    new List<FieldinformationVault>());

                if (usernotifycation == null)
                    usernotifycation = _accountService.GetByAccountId(postHandShakeModel.userinvitedid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
                notificationMessage.FromAccountId = userAccount.AccountId;
                notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = usernotifycation.AccountId;
                notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                notificationMessage.Content = postHandShakeModel.posthandshakecomment;

                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }
        done:
            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }
        [HttpPost]
        [Route("request/invite")]
        public IHttpActionResult InviteRequestByBusiness(HttpRequestMessage request,
           RequestViewModel requestViewModel)
        {
            if (requestViewModel == null)
                return BadRequest();

            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (!string.IsNullOrEmpty(requestViewModel.ToUserId))
                userAccount = _accountService.GetByAccountId(requestViewModel.ToUserId);
            if (userAccount.AccountType != AccountType.Business)
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                    .FirstOrDefault();
            }

            var usernotifycation = _accountService.GetByAccountId(requestViewModel.FromUserId);
            if (usernotifycation == null)
                return BadRequest();
            var rs = new PostHandShakeBusinessLogic().InsertPostHandshake(requestViewModel.InteractionId,
              requestViewModel.InteractionId, userAccount.AccountId, usernotifycation.AccountId,
              new List<FieldinformationVault>());
            if (!string.IsNullOrEmpty(rs))
            {
                requestViewModel.Status = EnumRequest.StatusComplete;
                var rq = new Request();
                rq = RequestAdapter.RequestViewModelToRequest(requestViewModel);
                var requestBus = new RequestBusinessLogic();
                requestBus.Update(rq);
            }

            var notificationMessage = new NotificationMessage();
            notificationMessage.Id = ObjectId.GenerateNewId();
            notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
            notificationMessage.FromAccountId = userAccount.AccountId;
            notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
            notificationMessage.ToAccountId = usernotifycation.AccountId;
            notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
            notificationMessage.Content = requestViewModel.Message;

            notificationMessage.PreserveBag = requestViewModel.InteractionId;
            var notificationBus = new NotificationBusinessLogic();
            notificationBus.SendNotification(notificationMessage);


            return Ok(requestViewModel);
        }
        [HttpPost]
        [Route("BusReInvitedMemberTerminateHandshake")]
        public HttpResponseMessage BusReInvitedMemberTerminateHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (!string.IsNullOrEmpty(postHandShakeModel.Userid))
                    userAccount = _accountService.GetByAccountId(postHandShakeModel.Userid);
                // Check business
                if (userAccount.AccountType != AccountType.Business)
                {
                    var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                    userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                        .FirstOrDefault();
                }
                ObjectId usernormalid = ObjectId.Empty;
                var usernotifycation = new Account();
                postHandShakeModel.ReturnStatus = true;
                if (postHandShakeModel.invitetype == "email")
                {
                    if (!string.IsNullOrEmpty(postHandShakeModel.userinvitedemail))
                    {
                        var accouuntpushtovault = _accountService.GetByEmail(postHandShakeModel.userinvitedemail);
                        if (accouuntpushtovault != null && accouuntpushtovault.AccountType == AccountType.Personal)
                        {
                            usernormalid = accouuntpushtovault.Id;
                        }
                        else
                        {
                            postHandShakeModel.ReturnStatus = false;
                            postHandShakeModel.ReturnMessage = new string[] { " email not exits " }.ToList();
                            goto done;
                        }
                    }
                    else
                    {
                        postHandShakeModel.ReturnStatus = false;
                        postHandShakeModel.ReturnMessage = new string[] { " email is empty " }.ToList();
                        goto done;
                    }
                }
                else
                {
                    if (userAccount.AccountType == AccountType.Business)
                        usernotifycation = _accountService.GetByAccountId(postHandShakeModel.userinvitedid);
                }


                new PostHandShakeBusinessLogic().InsertPostHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.posthandshakecomment, userAccount.AccountId, usernotifycation.AccountId,
                    new List<FieldinformationVault>());

                if (usernotifycation == null)
                    usernotifycation = _accountService.GetByAccountId(postHandShakeModel.userinvitedid);
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
                notificationMessage.FromAccountId = userAccount.AccountId;
                notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = usernotifycation.AccountId;
                notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                notificationMessage.Content = postHandShakeModel.posthandshakecomment;

                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }
        done:
            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("BusInvitedMemberHandshakeList")]
        public HttpResponseMessage BusInvitedMemberHandshakeList(HttpRequestMessage request,
            PostHandShakeListModel postHandShakeModel)
        {
            var lstEmailOutSite = new List<string>();
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeListModel();
            var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (!string.IsNullOrEmpty(postHandShakeModel.Userid))
                userAccount = _accountService.GetByAccountId(postHandShakeModel.Userid);
            // Check business
            if (userAccount.AccountType != AccountType.Business)
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                userAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                    .FirstOrDefault();
            }

            postHandShakeModel.ReturnStatus = true;
            for (int i = 0; i < postHandShakeModel.listEmailInvite.Count; i++)
            {
                var accountpushtovault = new Account();
                try
                {
                    accountpushtovault = _accountService.GetByEmail(postHandShakeModel.listEmailInvite[i]);
                }
                catch
                {
                    postHandShakeModel.ReturnStatus = false;
                    lstEmailOutSite.Add(postHandShakeModel.listEmailInvite[i]);
                }
                if (accountpushtovault != null && accountpushtovault.AccountType == AccountType.Personal)
                {
                    var usernotifycation = new Account();
                    postHandShakeModel.ReturnMessage.Add(postHandShakeModel.listEmailInvite[i]);
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            new PostHandShakeBusinessLogic().InsertPostHandshake(postHandShakeModel.CampaignId,
                                postHandShakeModel.posthandshakecomment, userAccount.AccountId,
                                accountpushtovault.AccountId, new List<FieldinformationVault>());
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyInvitedHandshake;
                            notificationMessage.FromAccountId = userAccount.AccountId;
                            notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                            notificationMessage.ToAccountId = accountpushtovault.AccountId;
                            notificationMessage.ToUserDisplayName = accountpushtovault.Profile.DisplayName;
                            notificationMessage.Content = postHandShakeModel.posthandshakecomment;
                            notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                        }
                        catch
                        {
                            postHandShakeModel.ReturnStatus = false;
                        }
                    });
                }
                else
                {
                    postHandShakeModel.ReturnStatus = false;
                    lstEmailOutSite.Add(postHandShakeModel.listEmailInvite[i]);
                }
            }

            // Out site
            for (int i = 0; i < lstEmailOutSite.Count; i++)
            {
                var outsite = new Outsite();
                outsite.DateCreate = DateTime.Now;
                outsite.Email = lstEmailOutSite[i];
                outsite.FromUserId = userAccount.AccountId;
                outsite.CompnentId = postHandShakeModel.CampaignId;
                outsite.FromDisplayName = userAccount.Profile.DisplayName;
                outsite.Type = EnumNotificationType.NotifyInvitedHandshakeOutsite;
                outsite.Description = postHandShakeModel.posthandshakecomment;
                try
                {
                    string OutsiteId = _outsiteBusinessLogic.InsertOutsite(outsite);
                    var emailTemplate = string.Empty;
                    if (System.Web.HttpContext.Current != null)
                    {
                        emailTemplate =
                            HttpContext.Current.Server.MapPath(
                                "/Content/EmailTemplates/EmailTemplate_HandShakeOutsite.html");
                    }
                    else
                    {
                        var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        emailTemplate = Path.Combine(appDir,
                            @"Content\EmailTemplates\EmailTemplate_HandShakeOutsite.html");
                    }
                    string emailContent = string.Empty;
                    string _email = outsite.Email;
                    if (File.Exists(emailTemplate) && !string.IsNullOrEmpty(_email))
                    {
                        emailContent = File.ReadAllText(emailTemplate);
                        var CompanyName = userAccount.CompanyDetails.CompanyName;
                        var fullName = userAccount.Profile.DisplayName;
                        emailContent = emailContent.Replace("[username]", fullName);

                        var baseUrl = GH.Util.UrlHelper.GetCurrentBaseUrl();
                        var callbackLink = String.Format("{0}/User/SignUpEx?id={1}", baseUrl, OutsiteId);
                        emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                        emailContent = emailContent.Replace("[comment]", outsite.Description);
                        IMailService mailService = new MailService();
                        mailService.SendMailAsync(new NotificationContent
                        {
                            Title = "Notification from Regit",
                            Body = string.Format(emailContent, ""),
                            SendTo = new[] { _email }
                        });
                    }
                }
                catch
                {
                }
            }
            // End Out site
            var response = Request.CreateResponse<PostHandShakeListModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        [HttpPost]
        [Route("BusAcknowledgeHandshake")]
        public HttpResponseMessage BusAcknowledgeHandshake(HttpRequestMessage request,
            PostHandShakeModel postHandShakeModel)
        {
            if (postHandShakeModel == null)
                postHandShakeModel = new PostHandShakeModel();
            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                Account userfrom = busAccount;
                Account userto = new Account();
                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
                        postHandShakeModel.CampaignId);
                if (busAccount.AccountType == AccountType.Personal)
                    userto = _accountService.GetByAccountId(posthandshake.BusId);
                else
                    userto = _accountService.GetByAccountId(postHandShakeModel.Userid);

                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyAcknowledgeHandshake;
                notificationMessage.FromAccountId = userfrom.AccountId;
                notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                notificationMessage.ToAccountId = userto.AccountId;
                notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                // notificationMessage.Content = delegationMessage.Message;
                notificationMessage.PreserveBag = postHandShakeModel.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
                new PostHandShakeBusinessLogic().AcknowledgeHandshake(postHandShakeModel.CampaignId,
                    postHandShakeModel.Userid, false);
                postHandShakeModel.ReturnStatus = true;
                postHandShakeModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                postHandShakeModel.ReturnStatus = false;
                postHandShakeModel.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<PostHandShakeModel>(HttpStatusCode.OK, postHandShakeModel);
            return response;
        }

        #endregion

        #region PushVault

        [Route("GetPushVaultByUser")]
        [HttpGet]
        public HttpResponseMessage GetPushVaultByUser(HttpRequestMessage request)
        {
            var vm = new List<PushVaultViewModel>();
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            try
            {
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                vm = CampaignBusinessLogic.GetAllPushVaultByUser(userAccount.AccountId);
            }
            catch
            {
                response = Request.CreateResponse<object>(HttpStatusCode.BadRequest, vm);
            }
            response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [HttpGet, Route("GetPushVaultUser")]
        public async Task<BusinessProfileViewModel> GetPushVaultUser()
        {
            var rs = new BusinessProfileViewModel();
            var vm = new List<PushVaultViewModel>();
            try
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                vm = CampaignBusinessLogic.GetAllPushVaultActiveByUser(currentUser.AccountId);
                var newFeedList = new List<NewFeedsViewModel>();
                foreach (var item in vm)
                {
                    var newfeed = new NewFeedsViewModel();
                    newfeed.UserId = currentUser.AccountId;
                    newfeed.CampaignId = item.CampaignId;
                    newfeed.CampaignType = item.Type;
                    newfeed.Name = item.Name;
                    newfeed.CampaignName = item.Name;
                    newfeed.Description = item.Description;
                    newFeedList.Add(newfeed);
                }
                rs.Id = currentUser.AccountId;
                rs.DisplayName = currentUser.Profile.DisplayName;
                rs.ListPushToVault = newFeedList;
            }
            catch
            {
            }
            return rs;
        }

        [Route("SavePushVault")]
        [HttpPost]
        public HttpResponseMessage SavePushVault(HttpRequestMessage request, PushVaultViewModel vm)
        {
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            try
            {
                if (vm.Created == null)
                    vm.Created = DateTime.Now;

                var campaign = FieldVaultToBsonArray(vm);
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                vm.Type = EnumCampaignType.PushToVault;
                vm.CampaignId = CampaignBusinessLogic.InsertCampaign(userAccount.AccountId, campaign);
            }
            catch
            {
                response = Request.CreateResponse<object>(HttpStatusCode.BadRequest, vm);
            }
            return response;
        }

        [Route("UpdatePushVault")]
        [HttpPost]
        public HttpResponseMessage UpdatePushVault(HttpRequestMessage request, PushVaultViewModel vm)
        {
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            try
            {
                var campaign = FieldVaultToBsonArray(vm);
                var userAccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                CampaignBusinessLogic.UpdateCampaign(vm.CampaignId, campaign);
            }
            catch
            {
                response = Request.CreateResponse<object>(HttpStatusCode.BadRequest, vm);
            }
            return response;
        }

        [HttpPost, Route("UpdateStatusPushVaultUser")]
        public HttpResponseMessage UpdateStatusPushVaultUser(PushVaultViewModel pushVault)
        {
            var response = Request.CreateResponse<object>(HttpStatusCode.OK, pushVault.Status);
            var campaignLogic = new CampaignBusinessLogic();
            try
            {
                campaignLogic.UpdateCampaignStatus(pushVault.CampaignId, pushVault.Status);
            }
            catch
            {
                response = Request.CreateResponse<object>(HttpStatusCode.BadRequest, pushVault.Status);
            }


            return response;
        }

        public BsonDocument FieldVaultToBsonArray(PushVaultViewModel vm)
        {
            var campaignContent = new BsonDocument();
            try
            {
                var bsArray = new BsonArray();
                foreach (var field in vm.Fields)
                {
                    var opArr = new BsonArray();
                    try
                    {
                        if (field.options.Count > 0)
                        {
                            foreach (var option in field.options)
                            {
                                opArr.Add(option);
                            }
                        }
                    }
                    catch
                    {
                    }
                    var bs = new BsonDocument
                    {
                        {"displayName", field.displayName ?? ""},
                        {"displayName2", field.displayName2 ?? ""},
                        {"id", field.id ?? ""},
                        {"jsPath", field.jsPath ?? ""},
                        {"label", field.label ?? ""},
                        {"optional", field.optional},
                        {"optional2", field.optional2},
                        {"type", field.type ?? ""},
                        {"options", opArr}
                    };
                    if (bs != null)
                        bsArray.Add(bs);
                }
                campaignContent = new BsonDocument
                {
                    {"type", EnumCampaignType.PushToVault},
                    {"status", vm.Status},
                    {"name", vm.Name},
                    {"description", vm.Description},
                    {"created", vm.Created},
                    {"fields", bsArray}
                };
            }
            catch
            {
            }

            return campaignContent;
        }

        #endregion PushVault

        #region SRFI

        [Route("GetSRFI")]
        [HttpPost]
        public HttpResponseMessage GetSRFI()
        {
            var rs = new SRFIViewModel();

            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                rs.BusId = busAccount.AccountId;
                rs.Avatar = busAccount.Profile.PhotoUrl;
                rs.DisplayName = busAccount.Profile.DisplayName;
                rs.ListCampaign =
                    CampaignBusinessLogic.GetCampaignForBusinessByType(busAccount.AccountId, EnumCampaignType.SRFI);
            }
            catch (Exception ex)
            {
                rs.ReturnStatus = false;

                rs.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<SRFIViewModel>(HttpStatusCode.OK, rs);
            return response;
        }

        [Route("GetSRFIActive")]
        [HttpPost]
        public HttpResponseMessage GetSRFIActive()
        {
            var rs = new SRFIViewModel();

            try
            {
                var busAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                rs.BusId = busAccount.AccountId;
                rs.Avatar = busAccount.Profile.PhotoUrl;
                rs.DisplayName = busAccount.Profile.DisplayName;
                rs.ListCampaign =
                    CampaignBusinessLogic.GetCampaignActiveForBusinessByType(busAccount.AccountId,
                        EnumCampaignType.SRFI);
            }
            catch (Exception ex)
            {
                rs.ReturnStatus = false;

                rs.ReturnMessage = new string[] { ex.ToString() }.ToList();
            }

            var response = Request.CreateResponse<SRFIViewModel>(HttpStatusCode.OK, rs);
            return response;
        }


        [Route("GetPostSRFI")]
        [HttpGet]
        public IHttpActionResult GetPostSRFI(string campaignId)
        {
            var rs = new Post();
            var postType = EnumCampaignType.SRFI;
            var user = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var post = new PostBusinessLogic();
            rs = post.GetPost(postType, campaignId);

            return Ok(rs);
        }

        [Route("InviteSRFI")]
        [HttpPost]
        public IHttpActionResult InviteSRFI(InviteViewModel invite)
        {
            var lstMailOutSite = new List<string>();
            var lstMailInSite = new List<string>();

            try
            {
                if (invite.InviteType.ToLower() == EnumSRFIInviteType.Email.ToLower())
                {
                    for (var i = 0; i < invite.ListEmail.Count; i++)
                    {
                        var userFromEmail = _accountService.GetByEmail(invite.ListEmail[i]);
                        if (userFromEmail != null)
                        {
                            invite.ToUserId = userFromEmail.AccountId;

                            SendInviteSRFI(invite);
                            lstMailInSite.Add(invite.ListEmail[i]);
                        }
                        else
                        {
                            lstMailOutSite.Add(invite.ListEmail[i]);
                            SendInviteSRFIOutSite(invite, invite.ListEmail[i]);
                        }
                    }
                }
                else
                {
                    SendInviteSRFI(invite);
                }
            }
            catch (Exception ex)
            {
                log.Error("UserId: " + invite.FromUserId + " invite SRFI to UserId: " + invite.ToUserId +
                          ex.ToString());
            }
            invite.ListEmailInSite = lstMailInSite;
            invite.ListEmailOutSite = lstMailOutSite;

            return Ok(invite);
        }

        public void SendInviteSRFI(InviteViewModel invite)
        {
            var rs = "";
            try
            {
                var campaign = new CampaignBusinessLogic();
                var user = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                var fol = new Follower();
                fol.UserId = invite.ToUserId;
                fol.Comment = invite.Comment;
                fol.Status = EnumSRFIStatus.Invite;
                fol.FollowedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var post = new PostBusinessLogic();
                rs = post.AddPost(invite.CampaignType, invite.CampaignId, invite.FromUserId, fol);

                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyInviteSRFI;
                notificationMessage.FromAccountId = invite.FromUserId;
                notificationMessage.FromUserDisplayName = invite.FromDisplayName;
                notificationMessage.ToAccountId = invite.ToUserId;
                notificationMessage.ToUserDisplayName = invite.ToDisplayName;

                notificationMessage.PreserveBag = invite.CampaignId;
                var notificationBus = new NotificationBusinessLogic();
                notificationBus.SendNotification(notificationMessage);
            }
            catch (Exception ex)
            {
                log.Error("UserId: " + invite.FromUserId + "Send invite SRFI to UserId: " + invite.ToUserId +
                          ex.ToString());
            }
        }

        private void SendInviteSRFIOutSite(InviteViewModel invite, string email)
        {
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate =
                    HttpContext.Current.Server.MapPath(
                        "/Content/EmailTemplates/EmailTemplate_HandShakeOutsite.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir,
                    @"Content\EmailTemplates\EmailTemplate_HandShakeOutsite.html");
            }
            var baseUrl = GH.Util.UrlHelper.GetCurrentBaseUrl();
            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
            }

            try
            {
                var outsite = new Outsite();
                outsite.DateCreate = DateTime.Now;
                outsite.Email = email;
                outsite.FromUserId = invite.FromUserId;
                outsite.CompnentId = invite.CampaignId;
                outsite.FromDisplayName = invite.FromDisplayName;
                outsite.Type = EnumNotificationType.NotifyInviteSRFI;
                outsite.Description = invite.Comment;
                string OutsiteId = _outsiteBusinessLogic.InsertOutsite(outsite);
                string _email = outsite.Email;
                if (!string.IsNullOrEmpty(emailContent) && !string.IsNullOrEmpty(_email))
                {
                    var fullName = invite.FromDisplayName;
                    emailContent = emailContent.Replace("[username]", fullName);
                    var callbackLink = String.Format("{0}/User/SignUpEx?id={1}", baseUrl, OutsiteId);
                    emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                    emailContent = emailContent.Replace("[comment]", outsite.Description);
                    IMailService mailService = new MailService();
                    mailService.SendMailAsync(new NotificationContent
                    {
                        Title = "Notification from Regit",
                        Body = string.Format(emailContent, ""),
                        SendTo = new[] { _email }
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("UserId: " + invite.FromUserId + "Send invite SRFI to email: " + email + ex.ToString());
            }
        }

        [Route("RegisSRFI")]
        [HttpPost]
        public IHttpActionResult RegisSRFI(RegisViewModel regis)
        {
            var rs = "";
            if (regis == null)
                return BadRequest();
            try
            {
                var info = new InfomationVaultBusinessLogic();
                var post = new PostBusinessLogic();
                var po = new Follower();
                if (regis != null)
                {
                    if (string.IsNullOrEmpty(regis.ToUserId))
                        regis.ToUserId = HttpContext.Current.User.Identity.GetUserId();

                    var user = _accountService.GetByAccountId(regis.ToUserId);
                    if (user != null)
                    {
                        po.UserId = regis.ToUserId;
                        po.Comment = regis.Comment;
                        po.CountryName = user.Profile.Country;
                        po.Gender = user.Profile.Gender;
                        po.Name = user.Profile.DisplayName;
                        po.Status = EnumSRFIStatus.Accept;
                        po.FollowedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        for (var i = 0; i < regis.Fields.Count(); i++)
                        {
                            if (regis.Fields[i].optional == false)
                                regis.Fields[i].options = null;
                        }
                        po.fields = regis.Fields;
                        foreach (var field in po.fields)
                        {
                            try
                            {
                                if (field.modelarrays == null)
                                {
                                    field.modelarrays = "{}";
                                }
                                switch (field.type)
                                {
                                    case "history":
                                    case "range":
                                        var str = JsonConvert.SerializeObject(field.modelarrays);
                                        field.modelarraysstr = str;
                                        field.modelarrays = null;
                                        break;
                                    case "doc":
                                        str = JsonConvert.SerializeObject(field.modelarrays);
                                        field.modelarraysstr = str;
                                        field.modelarrays = null;
                                        break;
                                    case "qa":
                                        str = ""; //JsonConvert.SerializeObject(field.modelarrays);
                                        field.modelarraysstr = str;
                                        field.modelarrays = null;
                                        field.options = null;
                                        break;
                                    case "radio":
                                        str = JsonConvert.SerializeObject(field.options);
                                        field.modelarraysstr = str;
                                        field.options = null;
                                        break;
                                    case "select":
                                        str = JsonConvert.SerializeObject(field.options);
                                        field.modelarraysstr = str;
                                        field.options = null;
                                        break;
                                    default:
                                        try
                                        {
                                            str = JsonConvert.SerializeObject(field.options);
                                            field.modelarraysstr = str;
                                            field.options = null;
                                        }
                                        catch
                                        {
                                        }
                                        try
                                        {
                                            str = JsonConvert.SerializeObject(field.modelarrays);
                                            field.modelarraysstr = str;
                                            field.modelarrays = null;
                                        }
                                        catch
                                        {
                                        }
                                        break;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(po.UserId))
                {
                    var businessUserAccount = _accountService.GetByAccountId(po.UserId);
                    rs = post.RegisPost(regis.CampaignId, po, businessUserAccount);
                    info.UpdateInformationVaultById(regis.ToUserId, regis.Fields);
                }
                if (!string.IsNullOrEmpty(regis.NotificationId) && !string.IsNullOrEmpty(rs))
                {
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.MarkRead(regis.NotificationId);
                }
            }
            catch (Exception ex)
            {
                log.Error("RegisSRFI UserId: " + regis.ToUserId + ex.ToString());
            }

            return Ok(rs);
        }

        [Route("GetSRFIForRegis")]
        [HttpGet]
        public IHttpActionResult GetSRFIForRegis(string campaignId, string regisUserId = null)
        {
            var rs = new CampaignVM();

            if (string.IsNullOrEmpty(regisUserId))
                regisUserId = HttpContext.Current.User.Identity.GetUserId();
            rs = CampaignBusinessLogic.CampaignById(campaignId);
            var acc = _accountService.GetByAccountId(rs.UserId);
            rs.DisplayName = acc.Profile.DisplayName;
            rs.Avatar = acc.Profile.PhotoUrl;
            var post = new PostBusinessLogic();
            var fol = post.GetFollowerPost(regisUserId, campaignId);
            if (fol != null)
            {
                rs.Comment = fol.Comment;
                rs.Status = fol.Status;
            }
            return Ok(rs);
        }

        [Route("GetSRFIForRegis")]
        [HttpPost]
        public IHttpActionResult GetValueFieldVault(RegisViewModel regis)
        {
            var rs = new List<FieldinformationVault>();
            if (string.IsNullOrEmpty(regis.ToUserId))
                regis.ToUserId = HttpContext.Current.User.Identity.GetUserId();
            var infoVault = new InfomationVaultBusinessLogic();
            rs = infomationVaultBusinessLogic.getInformationvaultforcampaign(regis.ToUserId, regis.CampaignId);

            return Ok(rs);
        }


        #endregion SRFI

        #region UserRequestHandShake
        [HttpGet, Route("requesthandshake/user")]
        public IHttpActionResult GetHandshakeRequestByUser(string fromUserId = null)
        {
            if (string.IsNullOrEmpty(fromUserId))
            {
                fromUserId = HttpContext.Current.User.Identity.GetUserId();
            }
            var rs = new List<RequestViewModel>();
            var requestBus = new RequestBusinessLogic();
            var lstRequest = requestBus.GetListByFromUserId(fromUserId);
            for (var i = 0; i < lstRequest.Count; i++)
            {
                var rq = new RequestViewModel();
                rq = RequestAdapter.RequestToRequestViewModel(lstRequest[i]);
                if (rq != null)
                {
                    if (rq.Type == null)
                    {
                        var acc = _accountService.GetByAccountId(rq.ToUserId);

                        rq.ToDisplayName = acc.Profile.DisplayName;
                        rq.ToAvatarUrl = acc.Profile.PhotoUrl;
                        rq.ToObjectUserId = acc.Id.ToString();
                        if (string.IsNullOrEmpty(rq.ToDisplayName))
                            rq.ToDisplayName = acc.CompanyDetails.CompanyName;
                    }

                    rs.Add(rq);
                }
            }
            return Ok(rs);
        }

        #endregion UserRequestHandShake
    }
}