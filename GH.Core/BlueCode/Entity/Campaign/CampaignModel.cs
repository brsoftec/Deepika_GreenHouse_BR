using System;
using System.Collections.Generic;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.BlueCode.Entity.Campaign
{
    public class CampaignModel
    {
        public CampaignModel()
        {

        }
        [BsonId]
        public ObjectId Id { get; set; }
        public string CampaignId { get; set; }
        public string BusinessId { get; set; }
        public string CampaignName { get; set; }
        public string DataType { get; set; }
        public string CampaignType { get; set; }
        public string Description { get; set; }
        public bool Gender { get; set; }

        public int FromAge { get; set; }
        public int ToAge { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int LocationType { get; set; }
        public int ContinentId { get; set; }
        public string ContinentName { get; set; }

        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal Budget { get; set; }
        public string UnitBudget { get; set; }
        public decimal FlashCost { get; set; }
        public bool IsFlash { get; set; }

        public int People { get; set; }
        public int MaxPeople { get; set; }
        public bool DisplayOnBuzFeed { get; set; }
        public bool AllowCreateQrCode { get; set; }
        public string ImagePath { get; set; }
        public string UrlLink { get; set; }
        public List<string> Tags { get; set; }
        public string CampaignStatus { get; set; }

        public List<Like> Like { get; set; }
        public List<Comment> Comment { get; set; }
        public int Share { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}
