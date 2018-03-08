using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;
using MongoDB.Driver;
using GH.Core.Extensions;
using MongoDB.Bson;

namespace GH.Core.Services
{
    public class DataService : IDataService
    {
        private IMongoCollection<Country> _countryCollection;
        private IMongoCollection<City> _cityCollection;
        private IMongoCollection<Region> _regionCollection;

        public DataService()
        {
            var mongoDb = MongoContext.Db;
            _countryCollection = mongoDb.Countries;
            _cityCollection = mongoDb.Cities;
            _regionCollection = mongoDb.Regions;
        }

        public List<Country> GetAllCountry()
        {
            return _countryCollection.Find(new BsonDocument()).ToList();
        }

        public List<City> GetCityByCountry(string countryCode)
        {
            return _cityCollection.Find(t => t.CountryCode == countryCode).ToList();
        }

        public List<City> GetCityByRegion(string regionCode)
        {
            return _cityCollection.Find(t => t.RegionCode == regionCode).ToList();
        }

        public List<Region> GetRegionByCountry(string countryCode)
        {
            return _regionCollection.Find(t => t.Code == countryCode).ToList();
        }
    }
}