using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GH.Core.Models;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;

namespace GH.Web.Areas.User.Controllers
{
    public class BusinessController : BaseController
    {

        public BusinessController()
        {
            _roleService = new RoleService();
        }
        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

             ViewBag.NgApp = "myApp";
            base.OnActionExecuting(filterContext);

        }
    }
}