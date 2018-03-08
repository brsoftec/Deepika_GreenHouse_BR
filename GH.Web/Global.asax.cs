using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.BusinessLogic.Payment;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace GH.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
       
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
           /// BundleTable.EnableOptimizations = true;
            var viewEngine = ViewEngines.Engines.OfType<RazorViewEngine>().FirstOrDefault();
        
            viewEngine.ViewLocationFormats = viewEngine.ViewLocationFormats.Concat(new string[] 
            {
                "~/Areas/Frontend/Views/{1}/{0}.cshtml",
                "~/Areas/Cms/Views/{1}/{0}.cshtml",
                "~/Areas/User/Views/{1}/{0}.cshtml",
            }).ToArray();

            viewEngine.MasterLocationFormats = viewEngine.MasterLocationFormats.Concat(new string[] 
            {
                "~/Areas/Frontend/Views/{1}/{0}.cshtml",
                "~/Areas/Frontend/Views/Shared/{0}.cshtml",
                "~/Areas/Cms/Views/{1}/{0}.cshtml",
                "~/Areas/Cms/Views/Shared/{0}.cshtml",
                "~/Areas/User/Views/Shared/{0}.cshtml",
                "~/Areas/User/Views/{1}/{0}.cshtml",
            }).ToArray();

            viewEngine.PartialViewLocationFormats = viewEngine.PartialViewLocationFormats.Concat(new string[] 
            {
                "~/Areas/Frontend/Views/{1}/{0}.cshtml",
                "~/Areas/Frontend/Views/Shared/{0}.cshtml",
                "~/Areas/Cms/Views/{1}/{0}.cshtml",
                "~/Areas/Cms/Views/Shared/{0}.cshtml",
                "~/Areas/User/Views/Shared/{0}.cshtml",
                "~/Areas/User/Views/{1}/{0}.cshtml",
            }).ToArray();
            //Execute Tasks
           // new SubcriptionTask("TaskSubscription").Start();
            new BillingTask("TaskBilling").Start();
            //
            JobScheduler.Start();

        }
        // Added the following procedure:
      
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var lang = HttpContext.Current.Request.Cookies.Get("regit-language");
          //  if (new HttpRequestWrapper(System.Web.HttpContext.Current.Request).IsAjaxRequest() && System.Web.HttpContext.Current.Request.IsAuthenticated)
            //    HttpContext.Current.Response.Redirect("~/User/SignUp");
            if (lang != null)
            {
                CultureInfo ci = new CultureInfo(lang.Value);
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }

        protected void Application_PostAuthorizeRequest()
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);

        }
    }
}
