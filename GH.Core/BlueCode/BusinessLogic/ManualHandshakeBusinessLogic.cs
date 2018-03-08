using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ManualHandshake;
using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class ManualHandshakeBusinessLogic : IManualHandshakeBusinessLogic
    {
        private MongoRepository<ManualHandshake> _repository;
        private Logger log = LogManager.GetCurrentClassLogger();

        public ManualHandshakeBusinessLogic()
        {
            _repository = new MongoRepository<ManualHandshake>();
        }

        public string Insert(ManualHandshake manualHandshake)
        {
            try
            {
                var checkExist = _repository.Many(l => l.accountId.Equals(manualHandshake.accountId) && l.toEmail.Equals(manualHandshake.toEmail)).FirstOrDefault();
                if (checkExist != null)
                {
                    manualHandshake.Id = checkExist.Id;
                }

                if (!string.IsNullOrEmpty(manualHandshake.Id.ToString()))
                {
                    var exist = _repository.Many(l => l.Id.Equals(manualHandshake.Id)).FirstOrDefault();
                    if (exist != null)
                    {
                        _repository.Update(manualHandshake);
                        if(manualHandshake.status == EnumManualHandshake.Active)
                            SendEmail(manualHandshake);
                        return manualHandshake.Id.ToString();
                    }
                }
                manualHandshake.Id = ObjectId.GenerateNewId();
                manualHandshake.CreatedDate = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(manualHandshake.accountId))
                {
                    _repository.Add(manualHandshake);
                    SendEmail(manualHandshake);
                }

                else
                    return String.Empty;
            }
            catch (Exception ex)
            {
                log.Debug("GetByTokenId Id = " + manualHandshake.Id + " exception " + ex.ToString());
                return String.Empty;
            }

            return manualHandshake.Id.ToString();
        }
      
        public List<ManualHandshake> GetActiveListByAccountId(string accountId)
        {
            try
            {
                return _repository.Many(l => l.accountId.Equals(accountId) && l.status == EnumManualHandshake.Active).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + accountId + " exception " + ex.ToString());
                return null;
            }
        }
        public List<ManualHandshake> GetActiveListPagingByAccountId(string accountId, int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.accountId.Equals(accountId) && l.status == EnumManualHandshake.Active).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + accountId + " exception " + ex.ToString());
                return null;
            }
        }
        public List<ManualHandshake> GetActiveListPagingByToAccountId(string toAccountId, int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.toAccountId.Equals(toAccountId) && l.status == EnumManualHandshake.Active).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + toAccountId + " exception " + ex.ToString());
                return null;
            }
        }

        public List<ManualHandshake> GetListPagingByAccountId(string accountId, int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.accountId.Equals(accountId)).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + accountId + " exception " + ex.ToString());
                return null;
            }
        }
        public List<ManualHandshake> GetListPagingByToAccountId(string toAccountId, int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.toAccountId.Equals(toAccountId)).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + toAccountId + " exception " + ex.ToString());
                return null;
            }
        }

        public List<ManualHandshake> GetActiveByEmail(string email, int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.email.Equals(email) && l.status == EnumManualHandshake.Active).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId AccountId = " + email + " exception " + ex.ToString());
                return null;
            }
        }
        public List<ManualHandshake> GetActiveByToEmail(string toEmail = "", int start = 0, int take = 10)
        {
            try
            {
                return _repository.Many(l => l.toEmail.Equals(toEmail) && l.status == EnumManualHandshake.Active).Skip(start).Take(take).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetActiveListByAccountId toEmail = " + toEmail + " exception " + ex.ToString());
                return null;
            }
        }

        public List<ManualHandshake> GetListByAccountId(string accountId)
        {
            try
            {

                return _repository.Many(l => l.accountId.Equals(accountId)).ToList();
            }
            catch (Exception ex)
            {
                log.Debug("GetListByAccountId AccountId = " + accountId + " exception " + ex.ToString());
                return null;
            }
        }
        private string SendEmail(ManualHandshake manualHandShake)
        {

            var informationVaultBus = new InfomationVaultBusinessLogic();
            if (string.IsNullOrEmpty(manualHandShake.accountId))
                return string.Empty;
            var mail = informationVaultBus.EmailInviteManualHandshake(manualHandShake);

            return mail;
        }

        public List<ManualHandshake> GetListByToEmail(string email)
        {
            throw new NotImplementedException();
        }

        public ManualHandshake GetById(string id)
        {
            throw new NotImplementedException();
        }
        public void DeleteById(string id)
        {
            throw new NotImplementedException();
        }
        public string Update(ManualHandshake manualHandshake)
        {
            throw new NotImplementedException();
        }
        public List<ManualHandshake> GetListByToAccountId(string toAccountId)
        {
            throw new NotImplementedException();
        }

    }
}