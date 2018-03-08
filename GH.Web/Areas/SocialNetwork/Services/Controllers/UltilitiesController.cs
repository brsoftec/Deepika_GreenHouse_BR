using GH.Web.Areas.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GH.Web.Areas.Services.Controllers
{
    [RoutePrefix("Api/Utilities")]
    public class UltilitiesController : ApiController
    {
        [HttpGet, Route("IsValidUrl")]
        public bool IsValidUrl([FromUri]ValidateUrlModel model)
        {
            return ModelState.IsValid;
        }
    }
}
