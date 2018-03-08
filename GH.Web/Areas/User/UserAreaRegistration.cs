using System.Web.Mvc;

namespace GH.Web.Areas.User
{
    public class UserAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "User_default",
                "User/{action}/{id}",
                new { controller = "User", action = "Index", id = UrlParameter.Optional }
            );            
        }
    }
}