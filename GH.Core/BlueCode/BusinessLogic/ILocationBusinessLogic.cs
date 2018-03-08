using GH.Core.BlueCode.Entity.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface ILocationBusinessLogic
    {
        IList<Country> GetAllContries();
        IList<Region> GetRegionByCountryCode(string countryCode);
        IList<City> GetCityByRegionCode(string regionCode);
        IList<City> GetCityByCountryCode(string countryCode);
        Country GetCountryByCountryCode(string countryCode);
        Country GetCountryByName(string countryName);
    }
}
