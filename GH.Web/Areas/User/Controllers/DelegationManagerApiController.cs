using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Delegation;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using GH.Core.Services;
using GH.Core.Models;
using GH.Web.Areas.User.ViewModels;
using GH.Core.BlueCode.Entity.Post;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/DelegationManager")]
    public class DelegationManagerApiController : ApiController
    {
        public IDelegationBusinessLogic delegationBusinessLogic = new DelegationBusinessLogic();
        public IProfileBusinessLogic profileBusinessLogic = new ProfileBusinessLogic();
        public IFriendBusinessLogic friendBusinessLogic = new FriendBusinessLogic();
        private INetworkService _networkService;
        private IAccountService _accountService;
        private Logger log = LogManager.GetCurrentClassLogger();

        public DelegationManagerApiController()
        {
            _networkService = new NetworkService();
            _accountService = new AccountService();
        }
        
        [Route("GetListDelegation")]
        [HttpPost]
        public HttpResponseMessage GetListDelegation(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                IList<DelegationItemTemplate> result = delegationBusinessLogic.GetDelegationList(delegationModelView.AccountId, delegationModelView.Direction).ToList();
                var listDelegation = new List<DelegationItemTemplate>();
                foreach (var item in result)
                {
                    var fromDisplayName = item.FromUserDisplayName;
                    item.FromUserDisplayName = fromDisplayName;
                    var checkDelegate = true;
                    try
                    {
                        if (!string.IsNullOrEmpty(item.EffectiveDate))
                        {
                            DateTime tempDate = DateTime.Parse(item.EffectiveDate);
                            if (tempDate > DateTime.Now)
                                checkDelegate = false;
                        }
                        if (!string.IsNullOrEmpty(item.ExpiredDate) && item.ExpiredDate != "Indefinite")
                        {
                            DateTime tempDate2 = DateTime.Parse(item.ExpiredDate);
                            if (tempDate2 < DateTime.Now)
                                checkDelegate = false;
                        }
                        if(item.DelegationRole == "Emergency")
                            checkDelegate = false;
                    }
                    catch { }
                    if (checkDelegate)
                        listDelegation.Add(item);
                }
                delegationModelView.Listitems = listDelegation;
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("GetListDelegationFull")]
        [HttpPost]
        public HttpResponseMessage GetListDelegationFull(HttpRequestMessage request, DelegationFullViewModel delegationFullViewModel)
        {
            if (delegationFullViewModel == null)
                delegationFullViewModel = new DelegationFullViewModel();
            try
            {
                delegationFullViewModel.AccountId = User.Identity.GetUserId();
                IList<DelegationItemTemplate> result = delegationBusinessLogic.GetDelegationList(delegationFullViewModel.AccountId, delegationFullViewModel.Direction).ToList();
                var lstItems = new List<DelegationItemViewModel>();
                if (result != null)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        var item = new DelegationItemViewModel();
                        var tPhotoUrl = "";
                        var fPhotoUrl = "";
                        var tId = "";
                        var fId = "";

                        var toAccountId = result[i].ToAccountId;
                        try
                        {
                            Account toAccount;
                            if (toAccountId == null)
                            {
                                toAccount = _accountService.GetByEmail(result[i].InvitedEmail);
                                toAccountId = toAccount.AccountId;
                            }
                            else
                            {
                                toAccount = _accountService.GetByAccountId(toAccountId);
                            }

                            tPhotoUrl = toAccount?.Profile.PhotoUrl;
                            tId = toAccount?.Id.ToString();
                            var fAccount = _accountService.GetByAccountId(result[i].FromAccountId);
                            fPhotoUrl = fAccount?.Profile.PhotoUrl;
                            fId = fAccount?.Id.ToString();
                        }
                        catch 
                        { }
                        item.DelegationId = result[i].DelegationId;
                        item.GroupVaultsPermission = result[i].GroupVaultsPermission;
                        item.FromAccountId = result[i].FromAccountId;
                        item.FromObjectId = fId;
                        item.FromUserDisplayName = result[i].FromUserDisplayName;
                        item.ToAccountId = toAccountId;
                        item.ToUserDisplayName = result[i].ToUserDisplayName;
                        item.ToObjectId = tId;
                        item.FromPhotoUrl = fPhotoUrl;
                        item.ToPhotoUrl = tPhotoUrl;
                        item.InvitedEmail = result[i].InvitedEmail;
                        item.Image = result[i].Image;
                        item.Direction = result[i].Direction;
                        item.Message = result[i].Message;
                        item.Status = result[i].Status;
                        item.DelegationRole = result[i].DelegationRole;
                        item.EffectiveDate = result[i].EffectiveDate;
                        item.ExpiredDate = result[i].ExpiredDate;
                        lstItems.Add(item);
                    }
              }
                delegationFullViewModel.Listitems = lstItems;
            }
            catch
            {
                delegationFullViewModel.ReturnStatus = false;
                delegationFullViewModel.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<DelegationFullViewModel>(HttpStatusCode.OK, delegationFullViewModel);
            return response;
        }

        [Route("GetDelegationItemTemplateFullById")]
        [HttpPost]
        public HttpResponseMessage GetDelegationItemTemplateFullById(HttpRequestMessage request, DelegationFullViewModel delegationFullViewModel)
        {
            if (delegationFullViewModel == null)
                delegationFullViewModel = new DelegationFullViewModel();
            try
            {
                delegationFullViewModel.AccountId = User.Identity.GetUserId();
                var deItem = new DelegationItemTemplate();
                deItem = delegationBusinessLogic.GetDelegationById(delegationFullViewModel.AccountId, delegationFullViewModel.DelegationId);
                var item = new DelegationItemViewModel();
                var tPhotoUrl = "";
                var fPhotoUrl = "";
                var tId = "";
                var fId = "";
                try
                {
                    var toAccount = _accountService.GetByAccountId(deItem.ToAccountId);
                    tPhotoUrl = toAccount.Profile.PhotoUrl;
                    tId = toAccount.Id.ToString();
                    var fAccount = _accountService.GetByAccountId(deItem.FromAccountId);
                    fPhotoUrl = fAccount.Profile.PhotoUrl;
                    fId = fAccount.Id.ToString();
                }
                catch { }

                item.DelegationId = deItem.DelegationId;
                item.GroupVaultsPermission = deItem.GroupVaultsPermission;
                item.FromAccountId = deItem.FromAccountId;
                item.FromObjectId = fId;
                item.FromUserDisplayName = deItem.FromUserDisplayName;
                item.ToAccountId = deItem.ToAccountId;
                item.ToUserDisplayName = deItem.ToUserDisplayName;
                item.ToObjectId = tId;
                item.FromPhotoUrl = fPhotoUrl;
                item.ToPhotoUrl = tPhotoUrl;
                item.InvitedEmail = deItem.InvitedEmail;
                item.Image = deItem.Image;
                item.Direction = deItem.Direction;
                item.Message = deItem.Message;
                item.Status = deItem.Status;
                item.DelegationRole = deItem.DelegationRole;
                item.EffectiveDate = deItem.EffectiveDate;
                item.ExpiredDate = deItem.ExpiredDate;
                delegationFullViewModel.DelegationItemTemplate = item;
                delegationFullViewModel.ReturnStatus = true;
                delegationFullViewModel.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                delegationFullViewModel.ReturnStatus = false;
                delegationFullViewModel.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<DelegationFullViewModel>(HttpStatusCode.OK, delegationFullViewModel);
            return response;
        }

        [Route("GetListDelegationForVault")]
        [HttpPost]
        public HttpResponseMessage GetListDelegationForVault(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                IList<DelegationItemTemplate> result = delegationBusinessLogic.GetDelegationList(delegationModelView.AccountId, delegationModelView.Direction).ToList().Where
                    (x =>  x.DelegationRole =="Super" || x.DelegationRole == "Normal").ToList();

                result.Insert(0, new DelegationItemTemplate
                {
                    FromAccountId = User.Identity.GetUserId(),
                    FromUserDisplayName = "My Self",
                    Status = "Accepted"
                });
                delegationModelView.Listitems = result;
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("GetDelegationItemTemplate")]
        [HttpPost]
        public HttpResponseMessage GetDelegationItemTemplate(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                BsonDocument result = delegationBusinessLogic.GetDelegationItemTemplate();
                var strresult = result.ToJson();
                delegationModelView.DelegationItemTemplate = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(strresult);
                delegationModelView.DelegationItemTemplate = result;
                var userId = HttpContext.Current.User.Identity.GetUserId();
                var users = _networkService.GetFriends(userId);
                var lstFriend = new List<MyFriend>();
                    foreach(Account acc in users)
                    {
                        var myFriend = new MyFriend();
                    try
                    {
                        myFriend.UserId = acc.AccountId.ToString();
                        myFriend.DisplayName = acc.Profile.DisplayName;
                    }
                    catch { }
                        if(!string.IsNullOrEmpty(myFriend.DisplayName))
                        lstFriend.Add(myFriend);
                    }
                delegationModelView.ListFriend = lstFriend;
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("GetDelegationItemTemplateById")]
        [HttpPost]
        public HttpResponseMessage GetDelegationItemTemplateById(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                delegationModelView.DelegationItemTemplate = delegationBusinessLogic.GetDelegationById(delegationModelView.AccountId, delegationModelView.DelegationId);

                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch (Exception ex)
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("SaveDelegationItemTemplate")]
        [HttpPost]
        public HttpResponseMessage SaveDelegationItemTemplate(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                var userId = User.Identity.GetUserId();
                if (delegationModelView.DelegationItemTemplateInsert!= null && !string.IsNullOrEmpty(delegationModelView.DelegationItemTemplateInsert.InvitedEmail))
                {
                    var isInvited = delegationBusinessLogic.checkIfInvitedEmailForDelegation(userId, delegationModelView.DelegationItemTemplateInsert.InvitedEmail);
                    if(isInvited)
                    {
                        return Request.CreateResponse<string>(HttpStatusCode.OK, "INVITED");
                    }
                }
                delegationModelView.DelegationItemTemplateInsert.FromAccountId = userId;
                delegationBusinessLogic.RequestDelegation(userId, delegationModelView.DelegationItemTemplateInsert);
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Get successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            //WRITE LOG
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordDelegation)
            {
                string title = "You registered on behalf of a delegator.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("AcceptDelegationItemTemplate")]
        [HttpPost]
        public HttpResponseMessage AcceptDelegationItemTemplate(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                delegationBusinessLogic.AcceptDelegation(delegationModelView.AccountId, delegationModelView.DelegationId);
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Accept successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordDelegation)
            {
                string title = "You accepted a delegation.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        [Route("ActivatedDelegation")]
        [HttpPost]
        public HttpResponseMessage ActivatedDelegation(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                delegationBusinessLogic.ActivatedDelegation(delegationModelView.AccountId, delegationModelView.DelegationId);
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Activated successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            //WRITE LOG
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordDelegation)
            {
                string title = "You activated a delegation.";
                string type = "interactions";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="delegationModelView"></param>
        /// <returns></returns>
        [Route("DeniedDelegationItemTemplate")]
        [HttpPost]
        public HttpResponseMessage DeniedDelegationItemTemplate(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                delegationBusinessLogic.DenyDelegation(delegationModelView.AccountId, delegationModelView.DelegationId);
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Denied successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            //WRITE LOG
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordDelegation)
            {
                string title = "You denied a delegation.";
                string type = "delegation";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }
        
        [Route("RemoveDelegation")]
        [HttpPost]
        public HttpResponseMessage RemoveDelegation(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                delegationBusinessLogic.RemoveDelegation(delegationModelView.AccountId, delegationModelView.DelegationId);
                delegationModelView.ReturnStatus = true;
                delegationModelView.ReturnMessage = new string[] { "Removed successfully" }.ToList();
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }

            //WRITE LOG
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account.AccountActivityLogSettings.RecordDelegation)
            {
                string title = "You removed a delegation.";
                string type = "delegation";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        // Delegate Emergency
        [Route("GetListDelegateEmergency")]
        [HttpPost]
        public HttpResponseMessage GetListDelegateEmergency(HttpRequestMessage request, DelegationModelView delegationModelView)
        {
            if (delegationModelView == null)
                delegationModelView = new DelegationModelView();
            try
            {
                delegationModelView.AccountId = User.Identity.GetUserId();
                IList<DelegationItemTemplate> result = delegationBusinessLogic.GetDelegationList(delegationModelView.AccountId, delegationModelView.Direction).ToList().Where
                (x => x.DelegationRole == "Emergency").ToList();
                delegationModelView.Listitems = result;
            }
            catch
            {
                delegationModelView.ReturnStatus = false;
                delegationModelView.ReturnMessage = new string[] { "fail" }.ToList();
            }
            var response = Request.CreateResponse<DelegationModelView>(HttpStatusCode.OK, delegationModelView);
            return response;
        }

        // working
        [Route("GetFollowerByCampaignId")]
        [HttpPost]
        public List<Follower> GetFollowerByCampaignId(HttpRequestMessage request, CampaignViewModel campaign)
        {
            var followers = new List<Follower>();
            
            try
            {
                var post = new PostBusinessLogic();
                followers = post.GetFollowersByCampaignId(campaign.CampaignId);
            }
            catch
            {
            }
            return followers;
        }

        // working
        [Route("CheckRegisterByCampaignId")]
        [HttpPost]
        public bool CheckRegisterByCampaignId(HttpRequestMessage request, CampaignViewModel campaign)
        {
            bool rs = false;
            try
            {
                var userId = User.Identity.GetUserId();
                var followers = new List<Follower>();
                var post = new PostBusinessLogic();
                followers = post.GetFollowersByCampaignId(campaign.CampaignId);
                if (followers.Exists(value => value.UserId == userId && value.Status != "Invite"))
                    rs = true;
            }
            catch
            {
            }
            return rs;
        }
    }
}
