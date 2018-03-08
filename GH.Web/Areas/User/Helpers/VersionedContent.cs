using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GH.Web.Areas.User.Helpers
{
    public static class VersionedContent
    {

        public static string Versioned(this UrlHelper helper, string path)
        {
            var value = helper.Content(path);

            var local = helper.RequestContext.HttpContext.Server.MapPath(path);
            if (File.Exists(local))
            {
                //DateTime modified = File.GetLastWriteTimeUtc(local);
                var lastWrite = File.GetLastWriteTime(local).Ticks.ToString();
                bool query = path.Contains("?") || path.Contains("&");
                value = String.Concat(value, query ? "&" : "?", "v=", lastWrite);
                //value = String.Concat(value, query ? "&" : "?", "v=", ConvertToUnixTimestamp(modified));

            }
            return value;
        }

        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            return epoch.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            TimeSpan diff = date - epoch;
            return Math.Floor(diff.TotalSeconds);
        }
    }

    internal static class BundleExtensions
    {
        public static Bundle Versioned(this Bundle sb)
        {
            sb.Transforms.Add(new LastModifiedBundleTransform());
            return sb;
        }
        public class LastModifiedBundleTransform : IBundleTransform
        {
            public void Process(BundleContext context, BundleResponse response)
            {
                foreach (var file in response.Files)
                {
                    var lastWrite = File.GetLastWriteTime(HostingEnvironment.MapPath(file.IncludedVirtualPath)).Ticks.ToString();
                    file.IncludedVirtualPath = string.Concat(file.IncludedVirtualPath, "?v=", lastWrite);
                }
            }
        }
    }
}