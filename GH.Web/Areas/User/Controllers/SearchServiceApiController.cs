using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.Models;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.Search;
using GH.Core.BlueCode.Entity.ProfilePrivacy;
using System.Web.UI.WebControls;
using GH.Core.BlueCode.Entity.Campaign;
using System;

namespace GH.Web.Areas.User.Controllers
{

    [RoutePrefix("api/SearchService")]
    public class SearchServiceApiController : ApiController
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

        public SearchServiceApiController()
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
        }
        [Authorize]
        [Route("SearchMainUser")]
        [HttpPost]
        public HttpResponseMessage SearchMainUser(HttpRequestMessage request, SearchModelView vm)
        {
            if (vm == null)
                vm = new SearchModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                vm.isbus = userAccount.AccountType == AccountType.Business ? true : false;
                DataList<UserSearchResult> dataresult = new DataList<UserSearchResult>();
                if (vm.isbus)
                {
                    dataresult = new SearchBusinessLogic().SearchMainBusUser(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId);
                }

                else
                    dataresult = new SearchBusinessLogic().SearchMainUserUser(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId);

                if (dataresult.DataOfCurrentPage.Count > 0)
                {
                    vm.TotalPages = dataresult.TotalItems;
                    vm.results = dataresult.DataOfCurrentPage;
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<SearchModelView>(HttpStatusCode.OK, vm);
            return response;

        }

        [Authorize]
        [Route("SearchMainBus")]
        [HttpPost]
        public IHttpActionResult SearchMainBus(HttpRequestMessage request, SearchModelView vm)
        {
            var lstUserBusinessResult = new List<UserSearchResult>();
            if (vm == null)
                vm = new SearchModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                vm.isbus = userAccount.AccountType == AccountType.Business ? true : false;
                DataList<UserSearchResult> dataresult = new DataList<UserSearchResult>();

                if (!vm.isbus)
                    dataresult = new SearchBusinessLogic().SearchMainUserBus(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId);
                else
                    dataresult = new SearchBusinessLogic().SearchMainBusBus(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId);
             
                if (dataresult.DataOfCurrentPage.Count > 0)
                {
                    lstUserBusinessResult = CheckPrivacy(dataresult, userAccount);
                    vm.TotalPages = dataresult.TotalItems;
                    vm.results = lstUserBusinessResult;
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            return Ok(vm);

        }
        private List<UserSearchResult> CheckPrivacy(DataList<UserSearchResult> listUser, Account userAccount)
        {
            var result = new List<UserSearchResult>();
            ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
            foreach (var user in listUser.DataOfCurrentPage)
            {
                var hasLinkAccount = false;
                var userBusiness = new UserSearchResult();
                userBusiness.Userid = user.Userid;
                userBusiness.UserAcccountid = user.UserAcccountid;
                userBusiness.DisplayName = user.DisplayName;
                userBusiness.Email = user.Email;
                userBusiness.Description = user.Description;
                userBusiness.PhotoUrl = user.PhotoUrl;
                userBusiness.FirstName = user.FirstName;
                userBusiness.LastName = user.LastName;
                userBusiness.StatusFriend = user.StatusFriend;

                var rs = new ProfilePrivacy();
                try
                {
                    rs = _profiePrivacy.GetProfilePrivacyByAccountId(user.UserAcccountid);
                    if (rs != null)
                    {
                        foreach (var field in rs.ListField)
                        {
                            if (field.Field == "PhotoUrl" && field.Role != "public")
                                userBusiness.PhotoUrl = "";

                            if (field.Field == "Email" && field.Role != "public")
                                userBusiness.Email = "";

                            if (field.Field == "Profile" && field.Role != "public")
                            {
                                userBusiness.PhotoUrl = "";
                                userBusiness.Email = "";
                            }

                        }

                    }
                    var lstLink = _accountService.GetBusinessAccountsLinkWithPersonalAccount(userAccount.Id);
                    if (lstLink.Exists(value => value.AccountId == user.UserAcccountid))
                    {
                        hasLinkAccount = true;
                    }
                }
                catch { }

                if (!hasLinkAccount)
                    result.Add(userBusiness);
            }
            return result;
        }

        //Search public 
        [Route("SearchMainPublic")]
        [HttpPost]
        public IHttpActionResult SearchMainPublic(HttpRequestMessage request, SearchModelView vm)
        {
            if (vm == null)
                vm = new SearchModelView();
            try
            {
                DataList<UserSearchResult> dataresult = new DataList<UserSearchResult>();
                dataresult = new SearchBusinessLogic().SearchMainPublic(vm.keyword, vm.CurrentPageNumber, vm.PageSize, vm.isbus);
                if (dataresult.DataOfCurrentPage.Count > 0)
                {
                    vm.TotalPages = dataresult.TotalItems;
                    vm.results = dataresult.DataOfCurrentPage;
                }
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            return Ok(vm);
        }

        [Route("GetCampaignListByBusinessId")]
        [HttpPost]
        public IHttpActionResult GetCampaignListByBusinessId(CampaignViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.keyword))
                vm.keyword = "";
            var ca = new CampaignListItem();
            var campaignListAll = CampaignBusinessLogic.GetCampaignByBusinessUser("", "All", "All", true, 1, 100, true, true, vm.keyword, true, true, true).DataOfCurrentPage;
            var newFeedList = new List<NewFeedsViewModel>();
            if (campaignListAll.Count > 0)
                newFeedList = CheckCampaignForSearch(campaignListAll);

            vm.NewFeedsItemsList = newFeedList;

            return Ok(vm);
        }

     
        private List<NewFeedsViewModel> CheckCampaignForSearch(List<CampaignListItem> listCampaign)
        {
            var lstNewFeed = new List<NewFeedsViewModel>();
            var checkValid = true;
            foreach (var item in listCampaign.OrderByDescending(x => x.Id))
            {
                var newfeed = new NewFeedsViewModel();
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
               
                //

                DateTime starDay = Convert.ToDateTime(item.SpendEffectDate);
                DateTime endDay = Convert.ToDateTime(item.SpendEndDate);
                if (starDay > DateTime.Now || item.Status != "Active")
                    checkValid = false;
                if (endDay < DateTime.Now && item.TimeType != "Daily")
                    checkValid = false;

                if (checkValid)
                    lstNewFeed.Add(newfeed);
            }

            return lstNewFeed;
        }

        [Authorize]
        [Route("GetBusinessForUser")]
        [HttpPost]
        public HttpResponseMessage GetBusinessForUser(HttpRequestMessage request, SearchModelView vm)
        {
            var lstUserBusinessResult = new List<UserSearchResult>();
            if (vm == null)
                vm = new SearchModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

                DataList<UserSearchResult> dataresult = new DataList<UserSearchResult>();

                vm.keyword = userAccount.Profile.Country;

                dataresult = new SearchBusinessLogic().GetBusinessUsers(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId);

                if (dataresult.DataOfCurrentPage.Count > 0)
                {
                    lstUserBusinessResult = CheckPrivacy(dataresult, userAccount);
                    vm.TotalPages = dataresult.TotalItems;
                    vm.results = lstUserBusinessResult;
                }

            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] { "fail" }.ToList();
            }

            var response = Request.CreateResponse<SearchModelView>(HttpStatusCode.OK, vm);
            return response;

        }

        [Route("GetAdBusisness")]
        [HttpPost]
        public AdBusiness GetAdBusisness(SearchModelView vm)
        {
            var adBusinessResult = new AdBusiness();
            var listAdBusiness = new List<AdBusinessUser>();

            if (vm == null)
                vm = new SearchModelView();
            try
            {
                var userAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                vm.keyword = userAccount.Profile.Country;
                DataList<UserSearchResult> dataresult = new DataList<UserSearchResult>();
                dataresult = new SearchBusinessLogic().GetAdBusisness(vm.keyword, vm.CurrentPageNumber, vm.PageSize, userAccount.Id.ToString(), userAccount.AccountId, vm.results);
                var listUserSearch = dataresult.DataOfCurrentPage;
                ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
                foreach (var user in listUserSearch)
                {

                    var adBusiness = new AdBusinessUser();
                    adBusiness.following = false;
                    adBusiness.id = user.Userid;
                    adBusiness.AccountId = user.UserAcccountid;
                    adBusiness.displayName = user.DisplayName;
                    adBusiness.email = user.Email;
                    adBusiness.desc = user.Description;
                    adBusiness.avatar = user.PhotoUrl;
                    if (user.StatusFriend == "Followed")
                        adBusiness.following = true;

                    adBusiness.PrivacyProfile.PhotoUrl = "public";
                    adBusiness.PrivacyProfile.Email = "public";
                    adBusiness.PrivacyProfile.Profile = "public";

                    var rs = new ProfilePrivacy();
                    try
                    {
                        rs = _profiePrivacy.GetProfilePrivacyByAccountId(user.UserAcccountid);
                        if (rs != null)
                        {
                            foreach (var field in rs.ListField)
                            {
                                if (field.Field == "PhotoUrl")
                                {
                                    adBusiness.PrivacyProfile.PhotoUrl = field.Role;
                                    if (field.Role != "public")
                                        adBusiness.avatar = "";
                                }

                                if (field.Field == "Email")
                                {
                                    adBusiness.PrivacyProfile.Email = field.Role;
                                    if (field.Role != "public")
                                        adBusiness.email = "";
                                }

                                if (field.Field == "Profile")
                                {
                                    adBusiness.PrivacyProfile.Profile = field.Role;
                                    if (field.Role != "public")
                                    {
                                        adBusiness.email = "";
                                        adBusiness.avatar = "";

                                    }

                                }

                            }

                        }
                        //
                    }
                    catch { }
                    listAdBusiness.Add(adBusiness);

                }

                adBusinessResult.AdBusinesUsers = listAdBusiness;
                adBusinessResult.results = listUserSearch;
                adBusinessResult.TotalUser = dataresult.TotalItems;
            }
            catch
            {
            }

            return adBusinessResult;

        }



    }
}
