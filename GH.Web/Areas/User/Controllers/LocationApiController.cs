using System.Net.Http;
using GH.Web.Areas.User.ViewModels;

using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using System.Net;
using System.Linq;
using GH.Core.Services;


namespace GH.Web.Areas.User.Controllers
{
    [RoutePrefix("api/LocationService")]
    public class LocationApiController : ApiController
    {
        public ILocationBusinessLogic locationBusinessLogic { get; set; }
        public IAccountService _accountService;

        public LocationApiController()
        {
            locationBusinessLogic = new LocationBusinessLogic();
            _accountService = new AccountService();
        }

        [Route("GetAllCountries")]
        [HttpPost]
        public HttpResponseMessage GetAllCountries(HttpRequestMessage request, LocationModelView locationModelView)
        {
            if (locationModelView == null)
                locationModelView = new LocationModelView();
            var listcountries = locationBusinessLogic.GetAllContries().ToList().Where(x=>!string.IsNullOrEmpty(x.Name)).ToList();
            locationModelView.Countries = listcountries;
            if (!string.IsNullOrEmpty(locationModelView.CountryName)  && ( listcountries.Count>0))
            {

                var CountryCode = locationModelView.Countries.FirstOrDefault(x => x.Name == locationModelView.CountryName).Code;
                locationModelView.Cities =
                    locationBusinessLogic.GetCityByCountryCode(CountryCode).ToList().Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            }
            var response = Request.CreateResponse<LocationModelView>(HttpStatusCode.OK, locationModelView);
            return response;

        }

        [Route("GetCitiesById")]
        [HttpPost]
        public HttpResponseMessage GetCitiesById(HttpRequestMessage request, LocationModelView locationModelView)
        {
            if (locationModelView == null)
                locationModelView = new LocationModelView();

            var listcities = locationBusinessLogic.GetCityByCountryCode(locationModelView.CountryCode).ToList().Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            locationModelView.Cities = listcities;
            var response = Request.CreateResponse<LocationModelView>(HttpStatusCode.OK, locationModelView);
            return response;
        }

        [Route("counties")]
        [HttpGet]
        public IHttpActionResult GetCounties()
        {
            var result = locationBusinessLogic.GetAllContries();

            return Ok(result.Select(x=> new
            {
                code = x.Code,
                name = x.Name
            }));
        }

        [Route("cities/{countryCode}")]
        [HttpGet]
        public IHttpActionResult GetCities(string countryCode)
        {
            return Ok(locationBusinessLogic.GetCityByCountryCode(countryCode));
        }
    }
}