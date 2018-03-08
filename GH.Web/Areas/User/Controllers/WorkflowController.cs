using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GH.Core.Models;
using GH.Core.Services;
using Microsoft.AspNet.Identity;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class WorkflowController : BusinessController
    {
        
        public WorkflowController()
        {
           
        }
        // GET: Workflow/
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult Permissions()
        {
            ViewBag.Resources = _resourceService.GetAllResources()
                .Select(r => new Resource {Name = r.Name, Permissions = new Permissions()
                {
                    Admin = r.Permissions.Admin.Replace("-","X"),
                    Editor = r.Permissions.Editor.Replace("-","X"),
                    Approver = r.Permissions.Approver.Replace("-","X"),
                  
                } });
            return View();
        }

    }
}