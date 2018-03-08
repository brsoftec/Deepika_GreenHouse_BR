namespace GH.API.API
{
    using Core.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    [RoutePrefix("api/tests")]
    public class TestController : ApiController
    {
        private readonly IAccountService _accountService;

        public TestController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok();
        }
    }
}
