using GH.Core.Services;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GH.Core.BlueCode.BusinessLogic;
using GH.Web.Areas.User.ViewModels;
using GH.Core.Models;
using System.Linq;
using System.Collections.Generic;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class EventCalendarController : BaseController
    {
        
        // GET: User/BusinessAccount
        public ActionResult Calendar()
        {            
            return View();
        }
        public ActionResult CreateEvent()
        {
            return View();
        }

    }
}