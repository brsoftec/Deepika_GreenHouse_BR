using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace GH.Web.Extensions
{
    public class ContextPerRequest
    {
        public static GreenHouseDbContext Db
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<GreenHouseDbContext>();
            }
        }
    }
}