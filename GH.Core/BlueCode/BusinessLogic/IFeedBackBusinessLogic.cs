using GH.Core.BlueCode.Entity.FeedBack;
using System.Collections.Generic;


namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IFeedBackBusinessLogic
    {
            void InsertFeedBack(FeedBackEntity feedBack);
            List<FeedBackEntity> LoadFeedBack(string accountId);
            void DeleteFeedBack(FeedBackEntity feedBack);
      
    }
}