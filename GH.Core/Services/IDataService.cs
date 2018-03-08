using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface IDataService
    {
        List<Country> GetAllCountry();
        List<City> GetCityByCountry(string countryCode);
        List<City> GetCityByRegion(string regionCode);
        List<Region> GetRegionByCountry(string countryCode);
    }
}