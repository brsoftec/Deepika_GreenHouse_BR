using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using GH.Web.Providers;
using GH.Core.Models;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security.Facebook;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using GH.Util;
using Microsoft.AspNet.Identity.Owin;

namespace GH.Web
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(GreenHouseDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            int minutesExpiredSession = ConfigHelp.GetIntValue("MinutesExpiredSession");
            if (minutesExpiredSession == 0)
                minutesExpiredSession = 15;
          
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
              // AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
               LoginPath = new PathString("/User/SignIn"),
               ExpireTimeSpan = TimeSpan.FromMinutes(minutesExpiredSession)

            });
             app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            


            // Configure the application for OAuth based flow
            PublicClientId = "self";
           
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
               
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(minutesExpiredSession),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);
            


            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

/*            var twitterAuthOptions = new TwitterAuthenticationOptions
            {
                ConsumerKey = ConfigurationManager.AppSettings["TWITTER_CONSUMER_KEY"],
                ConsumerSecret = ConfigurationManager.AppSettings["TWITTER_CONSUMER_SECRET"],
                BackchannelCertificateValidator = new Microsoft.Owin.Security.CertificateSubjectKeyIdentifierValidator(new[]
                {
                    "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
                    "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
                    "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
                    "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
                    "‎add53f6680fe66e383cbac3e60922e3b4c412bed", // Symantec Class 3 EV SSL CA - G3
                    "4eb6d578499b1ccf5f581ead56be3d9b6744a5e5", // VeriSign Class 3 Primary CA - G5
                    "5168FF90AF0207753CCCD9656462A212B859723B", // DigiCert SHA2 High Assurance Server C‎A 
                    "B13EC36903F8BF4701D498261A0802EF63642BC3" // DigiCert High Assurance EV Root CA
                }),  //stackoverflow: http://stackoverflow.com/questions/25011890/owin-twitter-login-the-remote-certificate-is-invalid-according-to-the-validati
                Provider = new TwitterAuthProvider()
            };

            app.UseTwitterAuthentication(twitterAuthOptions);

            var facebookAuthOptions = new FacebookAuthenticationOptions
            {
                AppId = ConfigurationManager.AppSettings["FACEBOOK_APP_ID"],
                AppSecret = ConfigurationManager.AppSettings["FACEBOOK_APP_SECRET"],
                Provider = new FacebookAuthProvider(),
                AuthorizationEndpoint = "https://www.facebook.com/v2.6/dialog/oauth",
                BackchannelHttpHandler = new FacebookBackChannelHandler(),
                UserInformationEndpoint = "https://graph.facebook.com/v2.6/me?fields=id,name,email,first_name,last_name,location,picture.type(large)"
            };

            facebookAuthOptions.Scope.Add("email");
            facebookAuthOptions.Scope.Add("public_profile");
            facebookAuthOptions.Scope.Add("publish_actions");
            facebookAuthOptions.Scope.Add("publish_pages");
            facebookAuthOptions.Scope.Add("user_posts");
            facebookAuthOptions.Scope.Add("manage_pages");
            facebookAuthOptions.Scope.Add("user_friends");
            facebookAuthOptions.Scope.Add("user_photos");

            app.UseFacebookAuthentication(facebookAuthOptions);
      
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GOOGLE_CLIENTID"],
                ClientSecret = ConfigurationManager.AppSettings["CLIENT_SECRET"],
                Provider = new GoogleAuthProvider()
            });*/

          //  app.MapSignalR();
        }
    }
    
    


/*    public class TwitterAuthProvider : TwitterAuthenticationProvider
    {
        public override Task Authenticated(TwitterAuthenticatedContext context)
        {
            context.Identity.AddClaim(new Claim("TwitterName", context.ScreenName));
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));
            context.Identity.AddClaim(new Claim("ExternalAccessTokenSecret", context.AccessTokenSecret));
           
            return base.Authenticated(context);
        }
    }

    public class FacebookAuthProvider : FacebookAuthenticationProvider
    {
        public override Task Authenticated(FacebookAuthenticatedContext context)
        {
            var firstName = context.User.GetValue("first_name");

            var lastName = context.User.GetValue("last_name");
            var displayName = context.User.GetValue("name");
            if (firstName != null)
            {
                context.Identity.AddClaim(new Claim("FirstName", firstName.ToString()));
            }
            if (lastName != null)
            {
                context.Identity.AddClaim(new Claim("LastName", lastName.ToString()));
            }
            if (displayName != null)
            {
                context.Identity.AddClaim(new Claim("DisplayName", displayName.ToString()));
            }

            var picture = context.User.GetValue("picture");
            if (picture != null)
            {
                var data = picture.Value<JObject>("data");
                if (data != null)
                {
                    var pic = data.GetValue("url");
                    if (pic != null)
                    {
                        context.Identity.AddClaim(new Claim("Avatar", pic.ToString()));
                    }
                }
            }
            
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));
            
         
            return base.Authenticated(context);
        }
    }

    public class GoogleAuthProvider : GoogleOAuth2AuthenticationProvider
    {
        public override Task Authenticated(GoogleOAuth2AuthenticatedContext context)
        {
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));
            return base.Authenticated(context);
        }
    }

    public class FacebookBackChannelHandler : HttpClientHandler
    {
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Replace the RequestUri so it's not malformed
            if (!request.RequestUri.AbsolutePath.Contains("/oauth"))
            {
                request.RequestUri = new Uri(request.RequestUri.AbsoluteUri.Replace("?access_token", "&access_token"));
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }*/


}
