using RegitSocial.Cache;
using RegitSocial.Data.MongoDB;
using RegitSocial.Entity.MongoDB;
using RegitSocial.Entity.MongoDB.Profile;
using RegitSocial.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegitSocial.Business.Profile.Location
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
        public IList<Country> GetAllContries()
        {
            IList<Country> countries = cacheContainer.Get<IList<Country>>(Common.CountriesKey);
            if (countries == null)
            {
                countries = repositoryCountry.Many(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList().ToList();
                cacheContainer.Add(Common.CountriesKey, countries, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return countries;
        }
        public IList<Region> GetRegionByCountryCode(string countrycode)
        {
            IList<Region> regions = cacheContainer.Get<IList<Region>>(Common.RegionsKey);
            if (regions == null)
            {
                regions = repositoryRegion.Many(x => true).ToList();
                cacheContainer.Add(Common.RegionsKey, regions, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }

            return regions.Where(x => x.Country== countrycode && !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Code)).ToList();
          
        }
        public IList<City> GetCityByRegionCode(string regionCode,string countrycode)
        {
            IList<City> cities = cacheContainer.Get<IList<City>>(Common.CitiesKey);
            if (cities == null)
            {
                cities = repositoryCity.Many(x => true).ToList();
                cacheContainer.Add(Common.CitiesKey, cities, new CachePolicy { Duration = 30, DurationType = CacheDurationType.Day });
            }
            return cities.Where(x => x.Region == regionCode   && !string.IsNullOrEmpty(x.Name)).ToList();          
        }
    }
    public interface ILocationBusinessLogic
    {
        IList<Country> GetAllContries();
        IList<Region>  GetRegionByCountryCode(string countrycode);
        IList<City> GetCityByRegionCode(string regionCode, string countrycode);
    }
}
