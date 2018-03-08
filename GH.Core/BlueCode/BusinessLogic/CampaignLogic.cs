using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Campaign;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Driver.Builders;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class CampaignLogic : ICampaignLogic
    {
        public CampaignModel GetCampaignById(CampaignModel model)
        {
            var result = new CampaignModel();
            try
            {
                var campaignCollection = MongoDBConnection.Database.GetCollection<CampaignModel>(RegitTable.Campaign);
                result = campaignCollection.Find(x => x.Id == model.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return result;

        }
        public List<CampaignModel> GetCampaignList(CampaignModel model)
        {
            var result = new List<CampaignModel>();
            try
            {
                var campaignCollection = MongoDBConnection.Database.GetCollection<CampaignModel>(RegitTable.Campaign);
                var value = campaignCollection.Find(x => x.BusinessId == model.BusinessId);
                if (value != null)
                    result = value.ToList();
            }
            catch (Exception ex)
            {

            }
            return result;

        }
        public int InsertCampaign(CampaignModel model)
        {
            try
            {

                var insertOption = new InsertOneOptions();
                var cancelToken = new CancellationToken();
                var campaignCollection = MongoDBConnection.Database.GetCollection<CampaignModel>(RegitTable.Campaign);

                campaignCollection.InsertOne(model, cancellationToken: cancelToken);
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
            
        }
        public int UpdateCampaignById(CampaignModel model)
        {
            var result = new List<CampaignModel>();
            try
            {
                var campaignCollection = MongoDBConnection.Database.GetCollection<CampaignModel>(RegitTable.Campaign);

                var builder = Builders<CampaignModel>.Filter;
                var filter = builder.Eq(c => c.Id, model.Id) & 
                             builder.Eq(c => c.BusinessId, model.BusinessId);
                
                var query = Query.And(Query<CampaignModel>.EQ(c => c.Id, model.Id), 
                                        Query<CampaignModel>.EQ(c => c.BusinessId, model.BusinessId));
                
                var existObject = campaignCollection.Find(filter).FirstOrDefault();
                if (existObject != null)
                {

                    var update = Builders<CampaignModel>.Update
                        .Set(a => a.CampaignName, model.CampaignName)
                        .Set(a => a.DataType, model.DataType)
                        .Set(a => a.CampaignType, model.CampaignType)
                        .Set(a => a.Description, model.Description)
                        .Set(a => a.Gender, model.Gender)
                        .Set(a => a.FromAge, model.FromAge)
                        .Set(a => a.ToAge, model.ToAge)
                        .Set(a => a.StartDate, model.StartDate)
                        .Set(a => a.EndDate, model.EndDate)
                        
                        .Set(a => a.LocationType, model.LocationType)
                        .Set(a => a.ContinentId, model.ContinentId)
                        .Set(a => a.ContinentName, model.ContinentName)
                        .Set(a => a.CountryId, model.CountryId)
                        .Set(a => a.CountryName, model.CountryName)
                        .Set(a => a.RegionId, model.RegionId)
                        .Set(a => a.RegionName, model.RegionName)
                        .Set(a => a.CityId, model.CityId)
                        .Set(a => a.CityName, model.CityName)
                        .Set(a => a.Latitude, model.Latitude)
                        .Set(a => a.Longitude, model.Longitude)

                        .Set(a => a.Budget, model.Budget)
                        .Set(a => a.UnitBudget, model.UnitBudget)
                        .Set(a => a.FlashCost, model.FlashCost)
                        .Set(a => a.IsFlash, model.IsFlash)

                        .Set(a => a.People, model.People)
                        .Set(a => a.MaxPeople, model.MaxPeople)
                        .Set(a => a.DisplayOnBuzFeed, model.DisplayOnBuzFeed)
                        .Set(a => a.AllowCreateQrCode, model.AllowCreateQrCode)
                        .Set(a => a.ImagePath, model.ImagePath)
                        .Set(a => a.UrlLink, model.UrlLink)

                        .Set(a => a.CampaignStatus, model.CampaignStatus)
                        .Set(a => a.ModifiedDate, model.ModifiedDate);

                    campaignCollection.UpdateOne(filter, update);
                    return 1;
                }
                else
                    return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
            
        }
        
    }

}
