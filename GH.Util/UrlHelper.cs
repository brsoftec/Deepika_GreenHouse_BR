using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GH.Util
{
    public class UrlHelper
    {
       
        public static string GetCurrentBaseUrl()
        {
            if(HttpContext.Current!=null)
            {
                var url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                if (url.EndsWith(":80"))
                {
                    return url.Replace(":80", "").Replace(":8080","");
                }
                else
                {
                    return url;
                }
            }

            return null;
        }

        public static string GetParameterFromUrlString(string urlString, string parameterName)
        {
            var pattern = "[?&]" + parameterName + "=(.*?)(&|$)";
            if(string.IsNullOrEmpty(urlString))
            {
                var match = Regex.Match(urlString, pattern);
                if (match.Success)
                {
                    var value = match.Groups[1].Value.Replace("&", "");
                    return value;
                }
            }
         
            return null;
        }
    }
}
