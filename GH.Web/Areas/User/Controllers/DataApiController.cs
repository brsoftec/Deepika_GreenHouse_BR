using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{

    [AllowAnonymous]
    [RoutePrefix("Api/Data")]
    public class DataApiController : ApiController
    {
        private IDataService _dataService;
        public DataApiController()
        {
            _dataService = new DataService();
        }

        [HttpGet, Route("Country")]
        public IHttpActionResult GetAllCountry()
        {
            var rawCountries = _dataService.GetAllCountry();
            List<object> countries = new List<object>();
            foreach (var country in rawCountries)
            {
                countries.Add( new {
                    Code = country.Code,
                    Name = country.Name,
                    Code3 = country.Code3,
                    NumCode = country.NumCode,
                    PhoneCode = country.PhoneCode,
                    FlagImage = $"/Content/img/flags/{country.Code}.png"
                });
            }
            return Json(new { success = true, Countries = countries });
        }

        [HttpGet, Route("Country/City")]
        public IHttpActionResult GetCityByCountry(string countryCode)
        {
            var cities = _dataService.GetCityByCountry(countryCode);
            return Json(new { Error = false, Cities = cities });
        }

        [HttpGet, Route("Region/City")]
        public IHttpActionResult GetCityByRegion(string regionCode)
        {
            var cities = _dataService.GetCityByRegion(regionCode);
            return Json(new { Error = false, Cities = cities });
        }

        [HttpGet, Route("Country/Region")]
        public IHttpActionResult GetRegionByCountry(string countryCode)
        {
            var regions = _dataService.GetRegionByCountry(countryCode);
            return Json(new { Error = false, Regions = regions });
        }
    }
}