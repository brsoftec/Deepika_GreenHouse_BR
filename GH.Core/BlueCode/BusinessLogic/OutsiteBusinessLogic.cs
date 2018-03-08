using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.Exceptions;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.BlueCode.Entity.Invite;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class OutsiteBusinessLogic : IOutsiteBusinessLogic
    {
        private IAccountService _accountService;
        private MongoRepository<Outsite> _repository;

        private Logger log = LogManager.GetCurrentClassLogger();
        public OutsiteBusinessLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<Outsite>();

        }
        


        public string InsertOutsite(Outsite outsite)
        {
            outsite.Id = ObjectId.GenerateNewId();
            try
            {
                _repository.Add(outsite);
            }
            catch (Exception ex)
            {
                log.Debug("Error Insert outsite Id = " + outsite.Id.ToString() + " exception " + ex.ToString());
               return "Error inserting invite";
            }
            return outsite.Id.ToString();
        }
        public Outsite GetOutsiteById(string id)
        {
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                var filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                return filterQuery;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
        public Outsite GetOutsiteByUserId(string userId, string type = null)
        {
            var rs = new Outsite();
            try
            {
                if(type != null)
                    rs = _repository.Many(l => l.FromUserId.Equals(userId) && l.Type.Equals(type)).FirstOrDefault();
                else
                    rs = _repository.Many(l => l.FromUserId.Equals(userId)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
            return rs;
        }
        public List<Outsite> GetListOutsiteByUserId(string userId, string type = null, string compnentId = null)
        {
            var rs = new List<Outsite>();
            try
            {
                if (type != null && compnentId !=null)
                    rs = _repository.Many(l => l.FromUserId.Equals(userId) && l.Type.Equals(type) && l.CompnentId.Equals(compnentId)).OrderByDescending(s => s.DateCreate).ToList();
                else if(type != null && compnentId == null)
                {
                    rs = _repository.Many(l => l.FromUserId.Equals(userId) && l.Type.Equals(type)).ToList();
                }
                else
                    rs = _repository.Many(l => l.FromUserId.Equals(userId)).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
            return rs;
        }
        //Sync Email Notification
        public Outsite GetOutsiteByCompnentId(string compnentId = null, string type = null)
        {
            var rs = new Outsite();
           
            try
            {
                rs = _repository.Many(l => l.Type.Equals(type) && l.CompnentId.Equals(compnentId)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
            return rs;
        }
        public string UpdateOutsite(Outsite outsite)
        {
           var exist = _repository.Many(l => l.Id.Equals(outsite.Id)).FirstOrDefault();
            if (exist != null)
                _repository.Update(outsite);
            else
                _repository.Add(outsite);
                           
            return outsite.Id.ToString();
        }
        public void DeleteOutsiteById(string id)
        {
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                Outsite filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                _repository.Delete(filterQuery);
            }
            catch { }
        }

        public List<Outsite> LoadOutsiteByEmail(string email)
        {
            try
            {
                var filterQuery = _repository.Many(l => l.Email.Equals(email)).OrderByDescending(s => s.DateCreate).ToList();
                return filterQuery;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
        public List<Outsite> LoadOutsiteByFromUserId(string fromUserId)
        {
            try
            {
                var filterQuery = _repository.Many(l => l.FromUserId.Equals(fromUserId)).OrderByDescending(s => s.DateCreate).ToList();
                return filterQuery;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

    }
}
