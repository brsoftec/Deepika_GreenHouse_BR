using GH.Core.Collectors;
using GH.Core.Exceptions;
using GH.Core.Models;
using GH.Core.Services;
using GH.Lang;
using GH.Web.Areas.User.ViewModels;
using GH.Web.Results;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GH.API.API
{
    [Authorize]
    [RoutePrefix("Api/AccountLink")]
    public class AccountLinkApiController : ApiController
    {
        IAccountService _accountService;

        public AccountLinkApiController()
        {
            _accountService = new AccountService();
        }

        [AllowAnonymous]
        [Route("FacebookAppId")]
        [HttpGet]
        public string GetFacebookAppId()
        {
            return ConfigurationManager.AppSettings["FACEBOOK_APP_ID"];
        }


        [Route("Unlink")]
        [HttpPost]
        public async Task Unlink(UnlinkAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            if (model.Network == SocialType.GreenHouse)
            {
                throw new CustomException(Regit.Invalid_Request_Message);
            }

            var currentAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            _accountService.UnlinkAccount(currentAccount.Id, model.Network);
        }

        [Route("Link")]
        [HttpPost]
        public async Task Link(LinkAccountModel model)
        {


            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            if (model.Network == SocialType.GreenHouse)
            {
                throw new CustomException(Regit.Invalid_Request_Message);
            }


            if (model.Network == SocialType.Facebook)
            {
                var parsedToken = await VerifyExternalAccessToken(model.Network.ToString(), model.AccessToken);

                if (parsedToken == null)
                {
                    throw new CustomException(Regit.Invalid_Request_Message);
                }

                var appId = GetFacebookAppId();
                if (parsedToken.app_id != appId || parsedToken.user_id != model.SocialAccountId)
                {
                    throw new CustomException(Regit.Invalid_Request_Message);
                }

                IFacebookCollector _facebookCollector = new FacebookCollector();
                try
                {
                    var longLivedToken = await _facebookCollector.GetLongLivedToken(model.AccessToken);
                    model.AccessToken = longLivedToken;
                    model.SecretAccessToken = null;
                }
                catch (HttpException ex)
                {
                    throw new CustomException(ex.GetHtmlErrorMessage());
                }
            }
            else if (model.Network == SocialType.Twitter)
            {
                ITwitterCollector _twitterCollector = new TwitterCollector();

                JObject credentials = JObject.FromObject(await _twitterCollector.VerifyCredentials(model.AccessToken, model.SecretAccessToken, false, false, false));

                string id = credentials.GetValue("id_str").Value<string>();

                if (credentials.GetValue("id_str").Value<string>() != model.SocialAccountId)
                {
                    throw new CustomException(Regit.Invalid_Request_Message);
                }
            }

            var currentAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            _accountService.LinkAccount(new AccountLink { Type = model.Network, SocialAccountId = model.SocialAccountId, AccessToken = model.AccessToken, AccessTokenSecret = model.SecretAccessToken, TwitterName = model.TwitterName }, currentAccount.Id);
        }

        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[AllowAnonymous]
        //[Route("ConnectSocialNetwork")]
        //[HttpGet]
        //public IHttpActionResult ConnectToSocialNetwork(string provider, string error = null)
        //{
        //    if (error != null)
        //    {
        //        return Redirect(Url.Content("~/User/ExternalLoginSuccess") + "#error=" + Uri.EscapeDataString(error));
        //    }

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    AccountController.ExternalLoginData externalLogin = AccountController.ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    var authentication = Request.GetOwinContext().Authentication;
        //    authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    string redirect = string.Format(Url.Content("~/User/ExternalLoginSuccess") + "#provider={0}&socialAccountId={1}&accessToken={2}&accessTokenSecret={3}&twitterName={4}", externalLogin.LoginProvider, externalLogin.ProviderKey, externalLogin.ExternalAccessToken, string.IsNullOrEmpty(externalLogin.ExternalAccessTokenSecret) ? "" : externalLogin.ExternalAccessTokenSecret, externalLogin.TwitterName);

        //    return Redirect(redirect);
        //}

        [HttpPost]
        [Route("ConnectToFacebookPage")]
        public async Task ConnectToFacebookPage(ConnectToFacebookPageModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var currentUser = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (currentUser.AccountType != AccountType.Business)
                throw new CustomException(Regit.Connect_To_Page_Feature_Not_Supported_Message);

            var facebookLink = currentUser.AccountLinks.FirstOrDefault(a => a.Type == SocialType.Facebook);
            if (facebookLink == null)
                throw new CustomException(Regit.Your_Account_Does_Not_Link_With_Facebook_Message);

            IFacebookCollector _facebookService = new FacebookCollector();
            JArray listPages = JArray.FromObject(_facebookService.GetListPages(facebookLink.AccessToken).data);
            var pagesParsed = listPages.ToObject<List<FacebookPageResponse>>();

            var page = pagesParsed.FirstOrDefault(p => p.id == model.Id);

            //save information of page
            _accountService.ConnectSocialPage(new SocialPage { Id = page.id, PageName = page.name, SocialType = SocialType.Facebook, AccessToken = page.access_token }, currentUser.Id);
        }

        [HttpPost]
        [Route("DisconnectFacebookPage")]
        public async Task DisconnectFacebookPage()
        {
            var currentUser = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (currentUser.AccountType != AccountType.Business)
            {
                throw new CustomException(Regit.Connect_To_Page_Feature_Not_Supported_Message);
            }

            var facebookLink = currentUser.AccountLinks.FirstOrDefault(a => a.Type == SocialType.Facebook);
            if (facebookLink == null)
            {
                throw new CustomException(Regit.Your_Account_Does_Not_Link_With_Facebook_Message);
            }
            _accountService.DisconnectSocialPage(currentUser.Id, SocialType.Facebook);
        }

        [HttpGet]
        [Route("ListFacebookPages")]
        public async Task<dynamic> GetFacebookPages()
        {
            var currentUser = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (currentUser.AccountType != AccountType.Business)
            {
                throw new CustomException(Regit.Connect_To_Page_Feature_Not_Supported_Message);
            }

            var facebookLink = currentUser.AccountLinks.FirstOrDefault(a => a.Type == SocialType.Facebook);
            if (facebookLink == null)
            {
                throw new CustomException(Regit.Your_Account_Does_Not_Link_With_Facebook_Message);
            }

            IFacebookCollector _facebookService = new FacebookCollector();
            var listPages = _facebookService.GetListPages(facebookLink.AccessToken);

            return listPages;
        }


        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                var appToken = ConfigurationManager.AppSettings["FACEBOOK_APP_TOKEN"];
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];
                }
            }

            return parsedToken;
        }


        public class ParsedExternalAccessToken
        {
            public string user_id { get; set; }
            public string app_id { get; set; }
        }

    }
}
