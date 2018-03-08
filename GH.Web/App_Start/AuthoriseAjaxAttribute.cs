
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace GH.Web.App_Start
{
    public class AuthoriseAjaxAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new RedirectResult("~/User/SignIn");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}