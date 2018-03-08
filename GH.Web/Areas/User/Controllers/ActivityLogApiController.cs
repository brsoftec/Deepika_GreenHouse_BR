
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.Exceptions;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/ActivityLogService")]
    public class ActivityLogApiController : ApiController
    {
        private IAccountService _accountService;
        private IActivityLogBusinessLogic _activityLogBusinessLogic;
        public ActivityLogApiController()
        {
            _accountService = new AccountService();
            _activityLogBusinessLogic = new ActivityLogBusinessLogic();
        }
       
        [HttpGet, Route("GetActivityLogs")]
        public async Task<List<ActivityLogViewModel>> GetActivityLogs(string activityLogType = null, int start = 0, int take = 10)
        {
      
            try
            {
                string accountId = ValidateCurrentUser();
                if (activityLogType == "all")
                    activityLogType = null;

                var lst =  _activityLogBusinessLogic.LoadActivityLog(accountId, activityLogType, start, take);
                var rs = new List<ActivityLogViewModel>();
                for(int i =0; i< lst.Count; i++)
                {
                    var act = new ActivityLogViewModel();
                  
                    act.ActivityId = lst[i].Id.ToString();
                    if(!string.IsNullOrEmpty(lst[i].DateTime.ToString()))
                    {
                        act.DateTime = lst[i].DateTime.ToString();
                    }
                  
                    act.ActivityType = lst[i].ActivityType;
                    act.Title = lst[i].Title;
                    act.Content = lst[i].Content;
                    act.FromUserId = lst[i].FromUserId;
                    act.FromUserName = lst[i].FromUserName;
                    act.FromUserLink = lst[i].FromUserLink;
                    act.ToUserId = lst[i].ToUserId;
                    act.ToUserName = lst[i].ToUserName;
                    act.ToUserProfileLink = lst[i].ToUserProfileLink;
                    act.TargetOjectId = lst[i].TargetOjectId;
                    act.TargetObjectName = lst[i].TargetObjectName;
                    act.TargetObjectLink = lst[i].TargetObjectLink;

                    rs.Add(act);
                }
                                      
                return rs;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpPost, Route("DeleteActivityLog")]
        public async Task DeleteActivityLog(ActivityLog model)
        {       
            _activityLogBusinessLogic.DeleteActivityLog(model);
        }
       
        // DeleteActivityLogByUserId
        [HttpPost, Route("DeleteActivityLogByUserId")]
        public async Task DeleteActivityLogByUserId()
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            string userId = account.AccountId;
            _activityLogBusinessLogic.DeleteActivityLogByUserId(userId);
        }
     

        private string ValidateCurrentUser()
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(accountId))
                throw new CustomException("Can not find current user.");
            return accountId;
        }
    }
}