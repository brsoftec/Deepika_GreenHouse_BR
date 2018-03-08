using Microsoft.Owin;
using Owin;
using IdentityServer3.AccessTokenValidation;
using System.Configuration;

[assembly: OwinStartup(typeof(GH.API.Startup))]

namespace GH.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string authority = $"{ConfigurationManager.AppSettings["ServiceUri"]}identity";

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions()
            {
                Authority = authority,
                RequiredScopes = new string[] { "RegitApi" },
                DelayLoadMetadata = true,
                ValidationMode = ValidationMode.ValidationEndpoint,
            });
        }
    }
}
