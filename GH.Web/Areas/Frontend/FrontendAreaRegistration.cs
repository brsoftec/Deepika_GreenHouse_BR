using System.Web.Mvc;

namespace GH.Web.Areas.Frontend
{
    public class FrontendAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Frontend";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            /*context.MapRoute(
                "User_default",
                "User/{action}/{id}",
                new { controller = "User", action = "Index", id = UrlParameter.Optional }
            );*/

            context.MapRoute(
               "Home_default",
               "Home/{action}/{id}",
               new { controller = "Home", action = "Index", id = UrlParameter.Optional }
           );

        }
    }
}