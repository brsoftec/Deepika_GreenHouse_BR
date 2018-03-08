using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/DataToAdmin")]
    public class DataToAdminController : ApiController
    {
        private IAccountService _accountService;
        public DataToAdminController()
        {
            _accountService = new AccountService();
        }
       

    }
}
