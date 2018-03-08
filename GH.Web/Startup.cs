using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using SignalR.EventAggregatorProxy.Owin;
using SignalR.EventAggregatorProxy.EventAggregation;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Nexmo.Api.Request;

[assembly: OwinStartup(typeof(GH.Web.Startup))]

namespace GH.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<CustomAuthenticationMiddleware>();
            ConfigureAuth(app);
            ConfigureSignalR(app);
        }

        public void ConfigureSignalR(IAppBuilder app)
        {
            app.MapSignalR();

            app.MapEventProxy<GH.Core.SignalR.Events.Event>();
        }
    }

    public static class Global
    {
        public static Credentials NexmoCredentials = new Credentials
        {
            ApiKey = "8610189e",
            ApiSecret = "048bce8a9587add6"
        };
        
    }


    public class CustomAuthenticationMiddleware : OwinMiddleware
    {
        public CustomAuthenticationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Path.ToString().ToLower() == "/token")
            {
                // Buffer the response
                var buffer = new MemoryStream();
                var body = context.Response.Body;
                context.Response.Body = buffer;

                await Next.Invoke(context);


                buffer.Seek(0, SeekOrigin.Begin);
                var resp = "";
                if (context.Response.StatusCode == 400 && context.Response.Headers.ContainsKey("AuthErrorResponse"))

                {
                    context.Response.Headers.Remove("AuthErrorResponse");

                    resp = JsonConvert.SerializeObject(
                        new
                        {
                            success = false,
                            message = "Incorrect username or password",
                            error_description = "Incorrect username or password",
                            error = "invalid_grant"
                        });

                    context.Response.StatusCode = 200;
                }
                else
                {
                    var reader = new StreamReader(buffer);
                    resp = await reader.ReadToEndAsync();
                    var regex = new Regex(@"""profile"":\s*""({.+})""", RegexOptions.Multiline);
                    var regex2 = new Regex(@"""profile"":\s*("".+"")", RegexOptions.Multiline);
                    var match = regex.Match(resp);
                    if (match.Success)
                    {
                        var profileStr = match.Groups[1].ToString();
//                        var profileJson = profileStr.Replace("\\\"", "\"");
                        var profileJson = Regex.Unescape(profileStr);
                        resp = regex2.Replace(resp, "\"profile\":" + profileJson);
                    }
                    //resp = Regex.Unescape(resp);
                    //resp = resp.Replace("\\\"", "\"");
                }
                var bytes = Encoding.UTF8.GetBytes(resp);
                buffer.SetLength(0); //change the buffer
                buffer.Write(bytes, 0, bytes.Length);

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = bytes.Length;

                // You need to do this so that the response we buffered
                // is flushed out to the client application.
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(body);
                context.Response.Body = body;
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}