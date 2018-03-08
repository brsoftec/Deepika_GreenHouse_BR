using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Profile;
using GH.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Core.BlueCode.BusinessLogic

{
    public class LocationBusinessLogic: ILocationBusinessLogic
    {

        IRepository<Country> repositoryCountry; IRepository<Region> repositoryRegion; IRepository<City> repositoryCity;
        private CacheContainer cacheContainer = CacheManager.CreateCacheContainer(typeof(LocationBusinessLogic).Name);
        public LocationBusinessLogic(IRepository<Country> repositoryCountry, IRepository<Region> repositoryRegion, IRepository<City> repositoryCity)
        {
            this.repositoryCountry = repositoryCountry;
            this.repositoryRegion = repositoryRegion;
            this.repositoryCity = repositoryCity;
        }

        public LocationBusinessLogic()
        {
            this.repositoryCountry = new MongoRepository<Country>();
            this.repositoryRegion = new MongoRepository<Region>();
            this.repositoryCity = new MongoRepository<City>();
        }
    
        public IList<Country> GetAllContries()
        {
            IList<Country> countries = cacheContainer.Get<IList<Country>>(GH.Util.Common.CountriesKey);
            if (countries == null)
            {
                countries = repositoryCountry.Many(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList();
                cacheContainer.Add(GH.Util.Common.CountriesKey, countries, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return countries;
        }
        public IList<Region> GetRegionByCountryCode(string countryCode)
        {
            IList<Region> regions = cacheContainer.Get<IList<Region>>(Util.Common.RegionsKey);
            if (regions == null)
            {
                regions = repositoryRegion.Many(x => true).ToList();
                cacheContainer.Add(Util.Common.RegionsKey, regions, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }

            return regions.Where(x => x.CountryCode.Equals(countryCode) && !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList();
          
        }
        public IList<City> GetCityByRegionCode(string regionCode)
        {
            IList<City> cities = cacheContainer.Get<IList<City>>(Util.Common.CitiesKey);
            if (cities == null)
            {
                cities = repositoryCity.Many(x => true).ToList();
                cacheContainer.Add(Util.Common.CitiesKey, cities, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return cities.Where(x => x.RegionCode.Equals(regionCode ) && !string.IsNullOrEmpty(x.Name)).ToList();          
        }

        public IList<City> GetCityByCountryCode(string countryCode)
        {
            IList<City> cities = cacheContainer.Get<IList<City>>(Util.Common.CitiesKey);
            if (cities == null)
            {
                cities = repositoryCity.Many(x => true).ToList();
                cacheContainer.Add(Util.Common.CitiesKey, cities, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return cities.Where(x => x.CountryCode.Equals(countryCode) && !string.IsNullOrEmpty(x.Name)).ToList();
        }      
        public Country GetCountryByCountryCode(string countryCode)
        {
            countryCode = countryCode.ToLower();
            IList<Country> countries = cacheContainer.Get<IList<Country>>(GH.Util.Common.CountriesKey);
            if (countries == null)
            {
                countries = repositoryCountry.Many(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList();
                cacheContainer.Add(GH.Util.Common.CountriesKey, countries, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return countries.FirstOrDefault(x => x.Code.ToLower().Equals(countryCode));
        }        
        public Country GetCountryByName(string countryName)
        {
            countryName = countryName.ToLower();
            if (countryName == "vietnam") countryName = "viet nam";
            IList<Country> countries = cacheContainer.Get<IList<Country>>(GH.Util.Common.CountriesKey);
            if (countries == null)
            {
                countries = repositoryCountry.Many(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList();
                cacheContainer.Add(GH.Util.Common.CountriesKey, countries, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return countries.FirstOrDefault(x => x.Name.ToLower().Equals(countryName));
        }
    }
}
