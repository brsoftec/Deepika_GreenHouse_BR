using System.Web.Mvc;

namespace GH.Web.Areas.SocialNetWork
{
    public class ServicesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SocialNetWork";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
        }
    }
}