using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ManageTokenDevice;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class ManageTokenDeviceBusinessLogic : IManageTokenDeviceBusinessLogic
    {
        private IAccountService _accountService;
        private MongoRepository<ManageTokenDevice> _repository;

        private Logger log = LogManager.GetCurrentClassLogger();
        public ManageTokenDeviceBusinessLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<ManageTokenDevice>();

        }
        public ManageTokenDevice GetByTokenId(string id)
        {
            var rs = new ManageTokenDevice();
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                rs = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();

            }
            catch (Exception ex)
            {
                log.Debug("GetByTokenId Id = " + id + " exception " + ex.ToString());
            }
            return rs;
        }

        public ManageTokenDevice GetByTokenDevice(string tokenDevice)
        {
            try
            {
                return _repository.Many(l => l.TokenDevice.Equals(tokenDevice)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                log.Debug("GetByTokenId tokenDevice = " + tokenDevice + " exception " + ex.ToString());
                throw ex;
            }
            
        }
        public List<ManageTokenDevice> GetListByTokenDevice(string tokenDevice)
        {
            try
            {
                var token = GetByTokenDevice(tokenDevice);
                return _repository.Many(l => l.TokenDevice.Equals(token.AccountId)).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetByTokenId tokenDevice = " + tokenDevice + " exception " + ex.ToString());
                return null;
            }

        }
        public List<ManageTokenDevice> GetListManageTokenDeviceByAccountId(string accountId)
        {
            var rs = new List<ManageTokenDevice>();
            try
            {
                 rs = _repository.Many(l => l.AccountId.Equals(accountId)).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetListManageTokenDeviceByAccountId Account Id = " + accountId + " exception " + ex.ToString());
            }
            return rs;
        }
        public ManageTokenDevice GetTokenDeviceByAcountIdAndTokenDevice(string accountId, string tokenDevice)
        {
            var rs = new ManageTokenDevice();
            try
            {
                rs = _repository.Many(l => l.AccountId.Equals(accountId) && l.TokenDevice.Equals(tokenDevice)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                log.Debug("GetListManageTokenDeviceByAccountId Account Id = " + accountId + " exception " + ex.ToString());
            }
            return rs;
        }
        public string Insert(ManageTokenDevice tokenDevice)
        {
          
            try
            {
                var token = GetTokenDeviceByAcountIdAndTokenDevice(tokenDevice.AccountId, tokenDevice.TokenDevice);
                if (token !=null)
                {
                    token.Status = tokenDevice.Status;
                    Update(token);
                }
                else
                {
                    tokenDevice.Id = ObjectId.GenerateNewId();
                    _repository.Add(tokenDevice);
                }
               
            }
            catch (Exception ex)
            {
                log.Debug("Insert token device Id = " + tokenDevice.Id.ToString() + " exception " + ex.ToString());
                return "Error inserting invite";
            }
            return tokenDevice.Id.ToString();
        }
       
        public void DeleteByTokenId(string id)
        {
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                ManageTokenDevice filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                _repository.Delete(filterQuery);
            }
            catch { }
        }
        public void Update(ManageTokenDevice tokenDivice)
        {
            var tokenDeviceCollection = MongoDBConnection.Database.GetCollection<ManageTokenDevice>(RegitTable.ManageTokenDevice);
            try
            {
                var builder = Builders<ManageTokenDevice>.Filter;
                var filter = builder.Eq(c => c.Id, tokenDivice.Id);
                tokenDeviceCollection.ReplaceOne(filter, tokenDivice);
            }
            catch {
                log.Debug("Update ManageTokenDevice Account Id = " + tokenDivice.ToString());
            }
        }
        public void UpdateStatus(string accountId, string tokenDivice, string status)
        {
            var tokenDeviceCollection = MongoDBConnection.Database.GetCollection<ManageTokenDevice>(RegitTable.ManageTokenDevice);
            try
            {
                var builder = Builders<ManageTokenDevice>.Filter;
                var filter = builder.Eq(c => c.TokenDevice, tokenDivice);
                filter &= builder.Eq(c => c.AccountId, accountId);
                var update = Builders<ManageTokenDevice>.Update
                    .Set(f => f.Status, status);

                tokenDeviceCollection.UpdateOne(filter, update);
              
            }
            catch
            {

                log.Debug("UpdateStatus Account Id = " + tokenDivice.ToString());
            }
        }
    }
}