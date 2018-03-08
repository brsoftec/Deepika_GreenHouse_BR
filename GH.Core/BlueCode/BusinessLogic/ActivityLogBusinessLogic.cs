using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ActivityLog;
using GH.Core.Exceptions;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class ActivityLogBusinessLogic : IActivityLogBusinessLogic
    {
        //private IMongoCollection<ActivityLog> _activityLogCollection;
        private IAccountService _accountService;
        private MongoRepository<ActivityLog> _repository;
    
        public ActivityLogBusinessLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<ActivityLog>();
        }
       
        public void WriteActivityLogFromAcc(string id, string title, string activityType = "", string businessId = "")
        {     
            var acc = _accountService.GetByAccountId(id);
            var activityLogBus = new ActivityLogBusinessLogic();
            var activityLog = new ActivityLog();
            activityLog.Id = ObjectId.GenerateNewId();
            activityLog.DateTime = DateTime.Now;
            activityLog.ActivityType = !string.IsNullOrEmpty(activityType) ? activityType : string.Empty;
            activityLog.FromUserId = acc.Profile == null ? null : acc.AccountId.ToString();
            activityLog.FromUserName = acc.Profile == null ? string.Format("{0} {1}", acc.Profile.FirstName, acc.Profile.LastName) : " ";
            activityLog.ToUserId = businessId;

            activityLog.Title = title;
            _repository.Add(activityLog);
        }

        public void DeleteActivityLog(ActivityLog activityLog)

        {
            ActivityLog filterQuery = _repository.Single(l => l.FromUserId.Equals(activityLog.FromUserId) && l.Id.Equals(ObjectId.Parse(activityLog.ActivityId)));
            _repository.Delete(filterQuery);
        }


        public void DeleteActivityLogByUserId(string userId)
        {
            var filterQuery = _repository.Many(l => l.FromUserId.Equals(userId));
            _repository.Delete(filterQuery);
        }
        public List<ActivityLog> LoadActivityLog(string accountId, string activityLogType = null, int start = 0, int take = 10)
        {
            try
            {
               // System.Threading.Thread.Sleep(10000);
                //var filterQuery = _repository.Many(l => l.FromUserId.Equals(accountId)).OrderByDescending(s=> s.DateTime).Skip(start).Take(take).ToList();
       
                if (!string.IsNullOrEmpty(activityLogType))
                {
                    var listtest = _repository.Many(l => l.FromUserId.Equals(accountId) && (l.ActivityType.Equals(activityLogType))).OrderByDescending(s => s.DateTime).ToList();
                    var filterQuery = listtest.Skip(start).Take(take).ToList();
                    return filterQuery;
                }
                else
                {
                   var filterQuery = _repository.Many(l => l.FromUserId.Equals(accountId)).OrderByDescending(s => s.DateTime).Skip(start).Take(take).ToList();
                    return filterQuery;
                }
                    

                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
    }

}