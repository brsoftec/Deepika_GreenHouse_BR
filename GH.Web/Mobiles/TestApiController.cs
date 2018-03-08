using GH.Core.Exceptions;
using GH.Core.Models;
using GH.Web.Mobiles.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GH.Web.Mobiles
{
    [RoutePrefix("api/mobile")]
    public class TestApiController : ApiController
    {
       
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("RegisterTest")]
        public async Task<string> RegisterTest(AccountTestViewModel model)
        {
            var user = new ApplicationUser() { UserName = model.Email.ToLower(), Email = model.Email.ToLower() };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new CustomException("Test errors");
            }

            return "Test Success";

        }

    }
}
