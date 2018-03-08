using System.Web.Mvc;

namespace GH.Web.Areas.Cms
{
    public class CmsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Cms";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
        }
    }
}