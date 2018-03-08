using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using GH.Core.IServices;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using GH.Core.Models;
using GH.Core.Services;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.AuthToken;
using GH.Core.ViewModels;
using GH.Core.BlueCode.Entity.ManageTokenDevice;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GH.Web.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private readonly IDisabledUserService _disableUserService;
        private IAccountService _accountService;
        private static IAuthTokensLogic _authTokensLogic = new AuthTokensLogic();


        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
            _disableUserService = new DisabledUserService();
            _accountService = new AccountService();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            // const string TOKEN_DEVICE = "tokendevice";

            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            var form = await context.Request.ReadFormAsync();

            // string tokenDevice = form[TOKEN_DEVICE];
            string fcmToken = form["fcm_token"];

            if (user == null)
            {
                context.SetError("invalid_grant", GH.Lang.Regit.Incorrect_Login);
                context.Response.Headers.Add("AuthErrorResponse", new[] {"incorrect"});

                return;
            }

            var accountInfo = _accountService.GetByAccountId(user.Id);

/*            if (!accountInfo.EmailVerified)
            {
                context.SetError("invalid_grant", "EMAIL_NOT_VERIFIED");
                return;
            }*/

            if (accountInfo.Status == "Close")
            {
                context.SetError("invalid_grant", "Account closed");
                return;
            }           
            if (!string.IsNullOrEmpty(fcmToken) && accountInfo.AccountType == AccountType.Business)
            {
                context.SetError("unsupported_client", "Business account not supported by mobile client");
                return;
            }

            var isDisabled = _disableUserService.IsDisabled(context.UserName);
            if (isDisabled)
            {
                context.SetError("invalid_grant", "DISABLE_USER_BY_MAIL");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            var account = _accountService.GetByAccountId(user.Id);
            AuthenticationProperties properties = CreateProperties(user.UserName, account, fcmToken);

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
/*
            if (!string.IsNullOrEmpty(tokenDevice))
            {
                var tokenDeviceLogic = new ManageTokenDeviceBusinessLogic();
                var manageTokenDevice = new ManageTokenDevice();
                manageTokenDevice.AccountId = account.AccountId;
                manageTokenDevice.TokenDevice = tokenDevice;
                manageTokenDevice.Status = EnumStatusTokenDevice.Online;
                manageTokenDevice.CreatedDate = DateTime.Now;
                tokenDeviceLogic.Insert(manageTokenDevice);
            }
*/

            context.Request.Context.Authentication.SignIn(cookiesIdentity);

            if (account.AccountActivityLogSettings.RecordAccess)
            {
                string title = "You logged in Regit.";
                string type = "user";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(user.Id, title, type);
            }

            context.Response.Cookies.Append("regit-language", accountInfo.Language);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            if (context.Properties.Dictionary.TryGetValue("fcmToken", out var fcmToken))
            {
                context.Properties.ExpiresUtc = DateTime.Now.AddDays(90);
            }

            context.AdditionalResponseParameters.Add("success", true);
            context.AdditionalResponseParameters.Add("message", "Login successful");
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
//            context.AdditionalResponseParameters.Add("prof", JObject.FromObject(new BasicProfile(){UserId = "abc"}));

            return Task.FromResult<object>(null);
        }


        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/User/ExternalLoginSuccess");
                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public override Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            var token = new AuthToken();
            token.AccountId = context.Properties.Dictionary["accountId"];
            token.AccessToken = context.AccessToken;
            token.Status = EnumStatusAuthToken.Active;
            token.Issued = context.Properties.IssuedUtc?.UtcDateTime ?? DateTime.Now;
            token.Expires = context.Properties.ExpiresUtc?.UtcDateTime ?? token.Issued.AddDays(1);
            if (context.Properties.Dictionary.TryGetValue("fcmToken", out var fcmToken))
                token.FcmToken = fcmToken;
            var request = context.Request;
            token.ClientInfo = new AuthClientInfo
            {
                host = request.Host.ToString(),
                ip = request.RemoteIpAddress,
                ua = request.Headers["User-Agent"]
            };

            _authTokensLogic.InsertToken(token);
//            context.AdditionalResponseParameters.Add("message", "Login successful");
            return base.TokenEndpointResponse(context);
        }


        public static AuthenticationProperties CreateProperties(string userName, Account account,
            string fcmToken = null)

        {
            var profile = new
            {
                firstName = account.Profile.FirstName,
                lastName = account.Profile.LastName,
                displayName = account.Profile.DisplayName,
                avatar = account.Profile.PhotoUrl,
                gender = account.Profile.Gender,
                dob = account.Profile.Birthdate?.ToString("yyyy/MM/dd"),
                country = account.Profile.Country,
                city = account.Profile.City,
            };
            var json = JsonConvert.SerializeObject(profile);

            var data = new Dictionary<string, string>
            {
                {"userName", userName},
                {"accountId", account.AccountId},
                {"profile", json}
            };
            if (!string.IsNullOrEmpty(fcmToken))
            {
                data.Add("fcmToken", fcmToken);
            }

            return new AuthenticationProperties(data);
        }
    }
}