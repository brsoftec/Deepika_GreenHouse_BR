using GH.Core.BlueCode.Entity.ActivityLog;
using System.Collections.Generic;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IActivityLogBusinessLogic
    {
        List<ActivityLog> LoadActivityLog(string accountId, string activityLogType = null, int start = 0, int take = 10);
        void DeleteActivityLog(ActivityLog activityLog);
        void DeleteActivityLogByUserId(string userId);
  
    }
}
