using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GH.Core.Models;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    public class BaseController : Controller
    {
        protected IAccountService _accountService;
        protected IResourceService _resourceService;
        protected IRoleService _roleService;


        protected List<Resource> Resources;
        protected Dictionary<string, Resource> ResourcesPerPath;

        protected string Host;
        protected string FullHost;
        protected bool IsProduction;

        static Logger _log = LogManager.GetCurrentClassLogger();

        protected string UserId;
        protected string AccountId;
        protected Account Account;
        public Account BusinessAccount;
        protected bool AsBusiness;

        public BaseController()
        {
            _accountService = new AccountService();
            _roleService = new RoleService();
            _resourceService = new ResourceService();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string action = ViewBag.Action = filterContext.ActionDescriptor.ActionName.ToLower();
            string controller = ViewBag.Controller =
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.IsPublic = true;
                return;
            }

            ViewBag.IsPublic = false;
            string path = controller + "." + action;

            var url = HttpContext.Request.Url;
            Host = url.Host;
            FullHost = url.Scheme + "://" + url.Authority;
            IsProduction = Host == "regit.today";

            //    ACCOUNT INITIALIZATION

            if (filterContext.HttpContext.Session == null || filterContext.HttpContext.Session.IsNewSession == true)
            {
                ViewBag.IsNewSession = true;
            }
            else
            {
                ViewBag.IsNewSession = filterContext.HttpContext.Session != null;
            }

            ResourcesPerPath = _resourceService.GetResourcesPerPath();

            Account account = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (account == null) return;
            
            Account = account;
            AccountId = account.AccountId;

            Account activeAccount = account;
            Account businessAccount = null;
            bool isBusiness = account.AccountType == AccountType.Business;
            bool isBusinessView = isBusiness;

            if (!isBusiness)
            {
                var linkedAccounts = _accountService.GetBusinessAccountsLinkWithPersonalAccount(account.Id);
                businessAccount = linkedAccounts.FirstOrDefault(x => x.EmailVerified);
                if (businessAccount != null)
                {
                    ViewBag.IsBusinessMember = true;
                }
            }
            else
            {
                businessAccount = account;
                ViewBag.IsBusinessMaster = true;
            }

            ViewBag.Account = account;
            BusinessAccount = ViewBag.BusinessAccount = businessAccount;
            ViewBag.ActiveAccount = activeAccount;
            ViewBag.IsBusiness = isBusiness;

            switch (controller)
            {
                case "workflow":
                case "campaign":
                case "interactions":
                case "businessusersystem":
                    isBusinessView = true;
                    break;            
                case "businessaccount":
                    if (action != "profile") isBusinessView = true;
                    break;
            }
            switch (path)
            {
                case "weberror.unauthorized":
                    isBusinessView = true;
                    break;
                case "businessaccount.profile":
                case "search.index":
                    if (Session["IsBusinessView"] != null)
                    {
                        isBusinessView = (bool) Session["IsBusinessView"];
                    }
                    break;
            }

            AsBusiness = ViewBag.IsBusinessView = isBusinessView;
            Session["IsBusinessView"] = isBusinessView;
            Session["TierView"] = isBusinessView ? "business" : "";
            if (isBusinessView && ViewBag.IsBusinessMember == true)
            {
                ViewBag.AsBusinessMember = true;
                ViewBag.ActiveAccount = ViewBag.BusinessAccount;
            }

            ViewBag.BodyClass = "authenticated";

            if (isBusinessView)
            {
                ViewBag.BodyClass = "regit-business authenticated";
                ViewBag.avatarUrlLiteral = "'" + ViewBag.ActiveAccount.Profile.PhotoUrl + "'";
                ViewBag.displayNameLiteral = "'" + ViewBag.ActiveAccount.Profile.DisplayName + "'";
            }
            else
            {
                ViewBag.avatarUrlLiteral = "user.PhotoUrl";
                ViewBag.displayNameLiteral = "user.DisplayName";
            }

            if (isBusinessView)
            {
                //    CHECK ROLES & PERMISSIONS
                if (ViewBag.AsBusinessMember == true)
                {
                    if (ResourcesPerPath.ContainsKey(path))
                    {
                        Resource resource = ResourcesPerPath[path];
                        Permissions permissions = resource.Permissions;
                        ViewBag.Permissions = permissions;


                        List<Role> roles = _roleService.GetRolesOfAccount(account, ViewBag.BusinessAccount.Id);
                        if (roles != null && roles.Count > 0)
                        {
                            IsRole isRole = new IsRole();
                            List<string> roleNames = new List<string>();

                            foreach (Role role in roles)
                            {
                                if (!isRole.IsAdmin && role.Name == Role.ROLE_ADMIN)
                                {
                                    isRole.IsAdmin = true;
                                    roleNames.Add("Admin");
                                }
                                if (!isRole.IsEditor && role.Name == Role.ROLE_EDITOR)
                                {
                                    isRole.IsEditor = true;
                                    roleNames.Add("Editor");
                                }
                                if (!isRole.IsApprover && role.Name == Role.ROLE_REVIEWER)
                                {
                                    isRole.IsApprover = true;
                                    roleNames.Add("Approver");
                                }
                            }

                            ViewBag.IsRole = isRole;

                            string effectivePermissions = GetEffectivePermissions(isRole, permissions);

                            if (effectivePermissions == "-")
                            {
                                WorkflowErrorViewModel error = new WorkflowErrorViewModel
                                {
                                    Name = "Unauthorized Access",
                                    Category = "workflow",
                                    Type = "page-access",
                                    Resource = resource.Id,
                                    ResourceName = resource.Name,
                                    Roles = String.Join(", ", roleNames),
                                    EffectivePermissions = "No Access"
                                };
                                TempData["error"] = error;
                                filterContext.Result = RedirectToAction("Unauthorized", "WebError");
//                            filterContext.Result = Redirect("/Error/Unauthorized");
                            }
                            else
                            {
                                ViewBag.CanWrite = effectivePermissions.Contains("W");
                                ViewBag.CanRead = ViewBag.CanWrite || effectivePermissions.Contains("R");
                            }
                        }
                    }
                }
                else
                {
                    IsRole isRole = new IsRole {IsAdmin = true};
                    ViewBag.IsRole = isRole;
                    ViewBag.CanRead = ViewBag.CanWrite = true;
                }

                //    CHECK PLAN SUBSCRIPTION
                if (path == "workflow.index" || path == "campaign.managerhandshakeusers")
                {
                    ResourceService.Subscription subscription =
                        _resourceService.GetSubscriptionByAccountId(businessAccount.AccountId);
                    ViewBag.SubscriptionPlan = subscription.Plan;
                }
            }

            base.OnActionExecuting(filterContext);
        }
//        


        public class IsRole
        {
            public bool IsAdmin { get; set; }
            public bool IsEditor { get; set; }
            public bool IsApprover { get; set; }
        }

        public class Permission
        {
            public bool Read { get; set; }
            public bool Write { get; set; }
        }

        public static string GetEffectivePermissions(IsRole isRole, Permissions permissions)
        {
            bool canRead = false;
            bool canWrite = false;

            if (isRole.IsAdmin)
            {
                canWrite = permissions.Admin.Contains("W");
                canRead = canWrite || permissions.Admin.Contains("R");
            }

            if (isRole.IsEditor)
            {
                canWrite = canWrite || permissions.Editor.Contains("W");
                canRead = canRead || permissions.Editor.Contains("W") || permissions.Editor.Contains("R");
            }

            if (isRole.IsApprover)
            {
                canWrite = canWrite || permissions.Approver.Contains("W");
                canRead = canRead || permissions.Approver.Contains("W") || permissions.Approver.Contains("R");
            }
            return canWrite ? "W" : (canRead ? "R" : "-");
        }

        public ActionResult ErrorView(string message, string name = "Error", string category = "system",
            string type = "exception")
        {
            WebErrorViewModel model = new WebErrorViewModel
            {
                Name = name,
                Category = category,
                Type = type,
                Message = message
            };
            TempData["error"] = model;
            return RedirectToAction("Index", "WebError", model);
            
        }

        //        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
//            var action = filterContext.ActionDescriptor.ActionName.ToLower();
//            base.OnActionExecuted(filterContext); 
//        }
        /* protected override void OnException(ExceptionContext filterContext)
         {
             if (filterContext.ExceptionHandled) // || !filterContext.HttpContext.IsCustomErrorEnabled)
                 return;
 
 //            var statusCode = (int) HttpStatusCode.InternalServerError;
 //            if (filterContext.Exception is HttpException)
 //            {
 //                statusCode = filterContext.Exception.As<HttpException>().GetHttpCode();
 //            }
 //            else if (filterContext.Exception is UnauthorizedAccessException)
 //            {
 //                //to prevent login prompt in IIS
 //                // which will appear when returning 401.
 //                statusCode = (int)HttpStatusCode.Forbidden; 
 //            }
             var log = LogManager.GetCurrentClassLogger();
             log.Debug(filterContext.Exception.Message);
             var exception = filterContext.Exception;
 
 //            var result = new ViewResult { ViewName = "Error" };
             WebErrorViewModel model = new WebErrorViewModel
             {
                 Name = "Error",
                 Category = "system",
                 Type = "exception",
                 Message = exception.Message
             };
             filterContext.Result = RedirectToAction("Index", "WebError", model);
 //            filterContext.Result = new ViewResult
 //            {
 //                ViewName = "~/Views/Shared/Error.cshtml"
 //            };
 
             // Prepare the response code.
             filterContext.ExceptionHandled = true;
 //            filterContext.HttpContext.Response.Clear();
 ////            filterContext.HttpContext.Response.StatusCode = statusCode;
 //            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
         }*/
    }
}