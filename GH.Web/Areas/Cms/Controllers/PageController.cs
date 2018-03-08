using GH.Core.Models;
using GH.Web.Areas.Cms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GH.Web.Areas.Cms.Controllers
{
    public class PageController : Controller
    {
        // GET: Cms/Page
        public ActionResult Index(string permanentLink)
        {
            var page = (CmsPagePublishedContentModel)System.Web.HttpContext.Current.Items["CmsPage"];
            return View(page);
        }
    }
}