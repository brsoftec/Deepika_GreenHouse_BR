using GH.Web.Areas.Cms.ViewModels;
using System.Configuration;
using System.Net.Http;
using System.Web;
using System.Web.Routing;

namespace GH.Web.Extensions
{
    public class CmsRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values[parameterName] != null)
            {
                try
                {
                    var permanentLink = values[parameterName].ToString();

                    HttpClient client = new HttpClient();
                    var adminUrl = ConfigurationManager.AppSettings["RegitAdminUrl"];

                    var task = client.GetAsync(adminUrl + "/Api/Cms/Pages/PublishedPage?link=" + permanentLink);
                    task.Wait();
                    if (task.IsCompleted && task.Result.IsSuccessStatusCode)
                    {
                        var readContentTask = task.Result.Content.ReadAsAsync<CmsPagePublishedContentModel>();
                        readContentTask.Wait();
                        var page = readContentTask.Result;
                        //cache the page
                        HttpContext.Current.Items["CmsPage"] = page;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (System.Exception)
                {
                    return false;
                }

            }
            return false;
        }
    }
}