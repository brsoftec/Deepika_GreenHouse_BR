﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.BlueCode.Entity.Campaign;
using NLog;
using GH.Core.BlueCode.DataAccess;
using GH.Core.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class CampaignService : ICampaignService
    {
        private MongoRepository<Campaign> _repository;
        private Logger log = LogManager.GetCurrentClassLogger();
        public CampaignService()
        {
            _repository = new MongoRepository<Campaign>();
        }

        public List<Campaign> GetCampaignActive(string userId =null, string type = null)
        {
            var rs = new List<Campaign>();
            try
            {
                if(!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(type))
                    rs = _repository.Many(l => l.userId.Equals(userId) && l.campaign.type.Equals(type) && l.campaign.status.Equals(EnumCampaignStatus.Active)).ToList();
                else if(!string.IsNullOrEmpty(userId))
                    rs = _repository.Many(l => l.userId.Equals(userId) && l.campaign.status.Equals(EnumCampaignStatus.Active)).ToList();
                else if (!string.IsNullOrEmpty(type))
                    rs = _repository.Many(l => l.campaign.type.Equals(type) && l.campaign.status.Equals(EnumCampaignStatus.Active)).ToList();
                else 
                    rs = _repository.Many(l => l.campaign.status.Equals("Active")).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            return rs;
        }

        public Campaign GetCampaignById(string id)
        {
            var rs = new Campaign();
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                rs = _repository.Single(l => l.Id.Equals(id));
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            return rs;
        }

        public List<Campaign> GetListByType(string type = null)
        {

            var rs = new List<Campaign>();
            try
            {
                 if (!string.IsNullOrEmpty(type))
                    rs = _repository.Many(l => l.campaign.type.Equals(type)).ToList();
                 else
                    rs = _repository.Many(l => !l.userId.Equals(string.Empty)).ToList();

            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            return rs;
        }
        public List<Campaign> GetListByUserId(string userId = null)
        {
          
            var rs = new List<Campaign>();
            
         
            try
            {
                if (!string.IsNullOrEmpty(userId))
                    rs = _repository.Many(l => l.userId.Equals(userId)).ToList();

            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            return rs;
        }
        public List<Campaign> GetListByStatus(string status = null)
        {
            var rs = new List<Campaign>();
            try
            {
                if (!string.IsNullOrEmpty(status))
                    rs = _repository.Many(l => l.campaign.status.Equals(status)).ToList();

            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            return rs;
        }

        public string InsertCampaign(Campaign campaignEntity)
        {
            try
            {
                var exist = _repository.Many(l => l.Id.Equals(campaignEntity.Id)).FirstOrDefault();
                if (exist != null)
                {
                    _repository.Update(campaignEntity);
                }
                else
                {
                    _repository.Add(campaignEntity);
                }
            }
            catch (Exception ex)
            {

                throw new CustomException(ex);
            }
            return campaignEntity.Id.ToString();
        }


        public string Update(Campaign campaignEntity)
        {
            _repository.Update(campaignEntity);
            return campaignEntity.Id.ToString();
        }
        public string DeleteCampaign(string id)
        {
            try
            {
                _repository.Delete(l => l.Id.Equals(id));
                return id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
        }
    }
}