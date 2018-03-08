using System;
using System.IO;
using System.Web;

using System.Web.Mvc;

using System.Web.Routing;

namespace GH.Web.Helpers
{
    public static class ViewUtils
    {
  
        public static string RenderPartialViewToString(string viewName, object model)
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            try
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", "EmptyController");

                var controllerContext = new ControllerContext(new RequestContext(httpContext, routeData), new EmptyController());
                var view = ViewEngines.Engines.FindPartialView(controllerContext, viewName).View;
                var sw = new StringWriter();
                view.Render(new ViewContext(controllerContext, view, new ViewDataDictionary { Model = model }, new TempDataDictionary(), sw), sw);

                return sw.ToString();
                
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
    }
    internal class EmptyController: Controller { }
}