using GH.Core.Models;
using GH.Core.Services;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using GH.Web.Areas.User.Controllers;

namespace GH.Web.Areas.Search.Controllers
{
    public class SearchController : BaseController
    {
        public SearchController()
        {
        }

        // GET: Search/?query=

        public ActionResult Index(string query)
        {
            var userAccount = new AccountService().GetByAccountId(User.Identity.GetUserId());
            if (ViewBag.IsBusinessView)
            {
                return View("BusinessSearch", (object) query);
            }
            else
            {
                return View("UserSearch", (object) query);
            }
        }

        public ActionResult UserSearch(string query)
        {
            return View((object) query);
        }

        public ActionResult BusinessSearch(string query)
        {
            return View((object) query);
        }
        // GET: Search/Public/?query=

        public ActionResult Public(string query)
        {
            return View((object) query);
        }
    }
}