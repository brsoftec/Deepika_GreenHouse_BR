using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Request;
using GH.Core.Exceptions;
using GH.Core.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class RequestBusinessLogic : IRequestBusinessLogic
    {
        private MongoRepository<Request> _repository;
        private Logger log = LogManager.GetCurrentClassLogger();
        public RequestBusinessLogic()
        {
            _repository = new MongoRepository<Request>();
        }

        public Request GetById(string id)
        {
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                var filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                return filterQuery;
            }
            catch (Exception ex)
            {
              
                throw new CustomException(ex);
            }
        }
        public List<Request> GetList(string type = null, string status = null)
        {
            var rs = new List<Request>();
            try
            {
                if (string.IsNullOrEmpty(type) && string.IsNullOrEmpty(status))
                    rs = _repository.Many(l => !string.IsNullOrEmpty(l.Type)).ToList();
                else if (string.IsNullOrEmpty(status))
                    rs = _repository.Many(l => l.Type.Equals(type)).ToList();
                else
                    rs = _repository.Many(l => l.Type.Equals(type) && l.Status.Equals(status)).ToList();
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }
            return rs;
        }
        public List<Request> GetListByEmail(string email)
        {

            try
            {
                return _repository.Many(l => l.Email.Equals(email)).ToList();
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }

        }
        public List<Request> GetListByFromUserId(string fromUserId, string status= null)
        {

            try
            {
                if (string.IsNullOrEmpty(status))
                    return _repository.Many(l => l.FromUserId.Equals(fromUserId) & !l.Status.Equals(EnumRequest.StatusDelete)).ToList();
                else
                    return _repository.Many(l => l.FromUserId.Equals(fromUserId) & l.Status.Equals(status)).ToList();
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }
        }
        public List<Request> GetListByToUserId(string toUserId)
        {
            try
            {
                return _repository.Many(l => l.ToUserId.Equals(toUserId)).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
        }
        public List<Request> GetListByToUserIdStatus(string toUserId, string status = null)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return _repository.Many(l => l.ToUserId.Equals(toUserId) & !l.Status.Equals(EnumRequest.StatusDelete)).ToList();
                else
                    return _repository.Many(l => l.ToUserId.Equals(toUserId) & l.Status.Equals(status)).ToList();
               
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
        }
        public string Insert(Request request)
        {
            try
            {
                var exist = _repository.Many(l => l.FromUserId.Equals(request.FromUserId) && l.ToUserId.Equals(request.ToUserId)).FirstOrDefault();
                if (exist != null)
                {
                    request.Id = exist.Id;
                    _repository.Update(request);
                }
                else
                {
                    _repository.Add(request);
                }
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }
            return request.Id.ToString();
        }
        public string Update(Request request)
        {
            try
            {
                _repository.Update(request);
                return request.Id.ToString();
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }
        }
      
        public void DeleteById(string id)
        {
            try
            {
                _repository.Delete(l => l.Id.Equals(id));
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
        }

    }
}