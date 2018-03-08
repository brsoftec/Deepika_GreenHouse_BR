using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using GH.Core.BlueCode.Entity.InformationVault;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/BusinessUserSystemService")]
    public class BusinessUserSystemServiceController : ApiController
    {
        //public IPostBusinessLogic PostFollowerLogic { get; set; }
        public IPostBusinessLogic PostBusinessLogic { get; set; }
        public IBusinessMemberLogic BusinessMemberLogic { get; set; }
        public ICampaignBusinessLogic CampaignBusinessLogic { get; set; }
        public IProfileBusinessLogic ProfileBusinessLogic { get; set; }
        public IAccountService AccountService { get; set; }

        public BusinessUserSystemServiceController()
        {
            this.PostBusinessLogic = new PostBusinessLogic();
            this.BusinessMemberLogic = new BusinessMemberLogic();
            this.CampaignBusinessLogic = new CampaignBusinessLogic();
            this.ProfileBusinessLogic = new ProfileBusinessLogic();
            this.AccountService = new AccountService();
        }

        [Route("AddBusinessMemberAdsCampaign")]
        [HttpPost]
        public HttpResponseMessage AddBusinessMemberAdsCampaign(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            try
            {
                var accountUser = AccountService.GetByAccountId(User.Identity.GetUserId());
                var businessAccount = AccountService.GetByAccountId(vm.BusinessUserId);

                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Update successfully" }.ToList();

                var userList = BusinessMemberLogic.GetMembersOfBusiness(businessAccount.AccountId, EnumFollowType.Following);
                PostBusinessLogic.RegisterCampaign(accountUser, businessAccount, vm.CampaignId, vm.CampaignType);

                //WRITE LOG
                if (vm != null)
                {
                    if (accountUser.AccountActivityLogSettings.RecordInteraction)
                    {
                        var act = new ActivityLogBusinessLogic();
                        string title = "You became member of business: " + businessAccount.Profile.DisplayName;
                        string type = "interactions";
                        act.WriteActivityLogFromAcc(accountUser.AccountId, title, type, businessAccount.AccountId);
                    }

                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
                
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("AddBusinessMember")]
        [HttpPost]
        public HttpResponseMessage AddBusinessMember(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var userAccount = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var businessUserAccount = AccountService.GetByAccountId(vm.BusinessUserId);
            vm.UserId = userId;
            try
            {
                BusinessMemberLogic.AddBusinessMember(userAccount, businessUserAccount);
                AccountService.FollowBusiness(userAccount.Id, businessUserAccount.Id);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch(Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }
            //WRITE LOG
            if (userAccount.AccountActivityLogSettings.RecordInteraction)
            {
                var act = new ActivityLogBusinessLogic();
                string title = "You followed business: " + businessUserAccount.Profile.DisplayName;
                act.WriteActivityLogFromAcc(userAccount.AccountId, title, "interactions", businessUserAccount.AccountId);
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("RemoveBusinessFromUser")]
        [HttpPost]
        public HttpResponseMessage RemoveBusinessFromUser(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var userAccount = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var businessUserAccount = AccountService.GetByAccountId(vm.BusinessUserId);
            vm.UserId = userId;
            try
            {
                BusinessMemberLogic.RemoveBusinessMember(userAccount, businessUserAccount);
                AccountService.UnfollowBusiness(userAccount.Id, businessUserAccount.Id);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
                //Log data
                if (userAccount.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    string title = "You unfollowed business: " + businessUserAccount.Profile.DisplayName ;

                    act.WriteActivityLogFromAcc(userAccount.AccountId, title, "interactions", businessUserAccount.AccountId);
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();

               
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetUserListByCampaignForChart")]
        [HttpPost]
        public HttpResponseMessage GetUserListByCampaignForChart(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var businessUserAccount = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            vm.BusinessUserId = businessUserAccount.AccountId;
            try
            {
                var list = PostBusinessLogic.GetFollowersList(vm.CampaignId);
                if (list != null)
                {
                    vm.FollowerList = list.DataOfCurrentPage;
                }
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
                //WRITE LOG
                if (businessUserAccount.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    string title = "You have retrieved customer list.";
                    act.WriteActivityLogFromAcc(vm.BusinessUserId, title, "interactions");
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetFollowerListForChart")]
        [HttpPost]
        public HttpResponseMessage GetFollowerListForChart(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var businessUserAccount = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            vm.BusinessUserId = businessUserAccount.AccountId;
            try
            {
                var list = BusinessMemberLogic.GetMembersOfBusiness(vm.BusinessUserId, EnumFollowType.Following);
                if (list != null)
                {
                    vm.FollowerList = list.ToList();
                }
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
                //WRITE LOG
                if (businessUserAccount.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    string title = "You have retrieved followers.";
                    act.WriteActivityLogFromAcc(vm.BusinessUserId, title, "interactions");
                }

            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
               
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetBusinessListByUser")]
        [HttpPost]
        public HttpResponseMessage GetBusinessListByUser(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            vm.UserId = userId;
            try
            {
                var list = BusinessMemberLogic.GetFollowingBusinesses(vm.UserId);
                var businessProfileList = new List<BusinessProfileViewModel>();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var businessItem = AccountService.GetByAccountId(item.Id);
                        if (businessItem != null)
                        {
                            var businessVm = new BusinessProfileViewModel();
                            businessVm.Id = businessItem.AccountId;
                            businessVm.DisplayName = businessItem.Profile.DisplayName;
                            businessVm.Avatar = businessItem.Profile.PhotoUrl;
                            businessVm.Description = businessItem.Profile.Description;

                            businessProfileList.Add(businessVm);
                        }
                    }
                }
                vm.BusinessProfileList = businessProfileList;
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch(Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
               
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

        [Route("GetTransactionByBusinessId")]
        [HttpPost]
        public HttpResponseMessage GetTransactionByBusinessId(HttpRequestMessage request, FollowerViewModel vm)
        {
            if (vm == null)
                vm = new FollowerViewModel();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var account = AccountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            vm.UserId = userId;
            try
            {
                if (account.AccountActivityLogSettings.RecordInteraction)
                {
                    var act = new ActivityLogBusinessLogic();
                    var activityList = act.LoadActivityLog(userId);
                    List<ActivityLog> list = activityList.ToList().Where(x => x.ToUserId == vm.BusinessUserId).ToList();
                    vm.ActivityLogList = list;
                    
                }
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, vm);
            return response;
        }

     
        [Route("GetChartDataByCampaign")]
        [HttpPost]
        public HttpResponseMessage GetChartDataByCampaign(HttpRequestMessage request, ChartDataCampaignModel vm)
        {
            if (vm == null)
                vm = new ChartDataCampaignModel();
            try
            {
                vm.Data = PostBusinessLogic.GetDataChartByCampaignByTime(vm.CamapignId, vm.Startdate);
                var post = new PostBusinessLogic();
                for (int i = 0; i < vm.Data.Count; i++)
                {
                    vm.Data[i].ListFieldsRegis = post.checkQA(vm.CamapignId, vm.Data[i].userid, vm.Data[i].ListFieldsRegis);
                }
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<ChartDataCampaignModel>(HttpStatusCode.OK, vm);
            return response;
        }

    
        [Route("GetChartDataByBus")]
        [HttpPost]
        public HttpResponseMessage GetChartDataByBus(HttpRequestMessage request, ChartDataBusIdModel vm)
        {
            if (vm == null)
                vm = new ChartDataBusIdModel();

            try
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                vm.Data = PostBusinessLogic.GetDataChartByBusidByTime(userId, vm.Startdate);
              
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<ChartDataBusIdModel>(HttpStatusCode.OK, vm);
            return response;
        }

    }
}
