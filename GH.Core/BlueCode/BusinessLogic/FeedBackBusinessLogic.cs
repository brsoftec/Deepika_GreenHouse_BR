using GH.Core.BlueCode.DataAccess;
using GH.Core.Exceptions;
using GH.Core.BlueCode.Entity.FeedBack;
using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using System.Reflection;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class FeedBackBusinessLogic : IFeedBackBusinessLogic
    {
        //private IMongoCollection<ActivityLog> _activityLogCollection;
        private IAccountService _accountService;
        private MongoRepository<FeedBackEntity> _repository;
        public FeedBackBusinessLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<FeedBackEntity>();
        }

        public void InsertFeedBack(FeedBackEntity feedBack)
        {
            var acc = _accountService.GetByAccountId(feedBack.UserId);
            var feedBackEntityBus = new FeedBackBusinessLogic();
            feedBack.Id = ObjectId.GenerateNewId();
            feedBack.Device = feedBack.Device + "_" + Assembly.GetExecutingAssembly().GetName().Version;
            feedBack.DateCreate = DateTime.Now;
            _repository.Add(feedBack);
        }


        public void DeleteFeedBack(FeedBackEntity feedBack)
        {
          
            FeedBackEntity filterQuery = _repository.Many(l => l.Name.Equals(feedBack.Name) && l.DateCreate.Equals(feedBack.DateCreate)).FirstOrDefault();
            _repository.Delete(filterQuery);
        }
        public List<FeedBackEntity> LoadFeedBack(string accountId)
        {
            try
            {
                var filterQuery = _repository.Many(l => l.Status.Equals("")).OrderByDescending(s => s.DateCreate).ToList();
                return filterQuery;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
    }
}