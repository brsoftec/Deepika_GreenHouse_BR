using GH.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GH.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //force to map cms route first
            routes.MapRoute(
                name: "CmsRoute",
                url: "{*permanentLink}",
                defaults: new { area = "Cms", controller = "Page", action = "Index" },
                constraints: new { permanentLink = new CmsRouteConstraint() },
                namespaces: new[] { "GH.Web.Areas.Cms.Controllers" }
            );          
            routes.MapRoute(
                name: "InteractionPageRoute",
                url: "interaction/{id}",
                defaults: new { controller = "Interaction", action = "Index", id = UrlParameter.Optional }
            );

            var route = routes.MapRoute(
                name: "DefaultRoute",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "User", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
