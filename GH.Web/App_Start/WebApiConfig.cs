using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using GH.Core.Exceptions;
using System.Configuration;
using System.Web.Http.Cors;

namespace GH.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            
            var cors = new EnableCorsAttribute("*", "*", "*");

            config.EnableCors(cors);
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var showExceptionStr = ConfigurationManager.AppSettings["ATTACH_EXCEPTION"];
            bool showException = false;

            if (!string.IsNullOrEmpty(showExceptionStr))
            {
                bool.TryParse(showExceptionStr, out showException);
            }

            config.Filters.Add(new CustomExceptionFilter(showException));
        }
    }
}
