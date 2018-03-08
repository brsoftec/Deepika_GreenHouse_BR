using System.Collections.Generic;
using PayPal.Api;

namespace WizLocal.BL.Core
{
    public static class PayPalConfig
    {
		public readonly static string ClientId;
		public readonly static string ClientSecret;

		static PayPalConfig()
	    {
			var config = GetConfig();
			ClientId = config["clientId"];
			ClientSecret = config["clientSecret"];
		}
		private static Dictionary<string, string> GetConfig()
        {

			return ConfigManager.Instance.GetProperties();

			Dictionary<string, string> configMap = new Dictionary<string, string>();

            // Endpoints are varied depending on whether sandbox OR live is chosen for mode

            // TODO [Payment for Test] Change from Sandbox to Live 1

            /* SANDBOX */
            //configMap.Add("mode", "sandbox");

            /* LIVE */
            configMap.Add("mode", "live");

            // These values are defaulted in SDK. If you want to override default values, uncomment it and add your value
            // configMap.Add("connectionTimeout", "360000");
            // configMap.Add("requestRetries", "1");
            return configMap;
        }

        // Create accessToken
        private static string GetAccessToken()
        {
            // ###AccessToken
            // Retrieve the access token from
            // OAuthTokenCredential by passing in
            // ClientID and ClientSecret
            // It is not mandatory to generate Access Token on a per call basis.
            // Typically the access token can be generated once and
            // reused within the expiry window EBWKjlELKMYqRNQ6sYvFo64FtaRLRR5BdHEESmha49TM//EO422dn3gQLgDbuwqTjzrFgFtaRLRR5BdHEESmha49TM

            //try
            //{
            //    // SAND BOX
            //    //string accessToken = new OAuthTokenCredential("AXT7QxBp_3QWP9kjMZkgxK6HLEVC-4Bs4ihTTtILdszZeCaYHNmS3bpvzivS", "ELp4VBBCa_Q7PoZKk2HrhUBFdPAxcC0R2zHJOg3TGXKq-G9Ti_NsYohEgE_d", GetConfig()).GetAccessToken(); // SAND BOX
                
            //    // LIVE
            //    string accessToken = new OAuthTokenCredential("AdVmJRBl3NCI7RlPQzbj17t4usRC2H_YCjK9DSkbvVALjIG0r1Rj1AXAd9zD", "EN0f_xCPb4G7l6bS-NPa1NxsAI2fG1XWKbD3dIL03u3KTfazj7ZPYy3pqjQe", GetConfig()).GetAccessToken();// LIVE
            //    return accessToken;
            //}
            //catch
            //{
            //    return "";
            //}

			string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
			return accessToken;

		}

		
		public static APIContext GetAPIContext(string accessToken = "")
        {
            APIContext apiContext = new APIContext(string.IsNullOrEmpty(accessToken) ? GetAccessToken() : accessToken);
            apiContext.Config = GetConfig();
            
            return apiContext;
        }

        //private static string AccessTokenTest
        //{
        //    get
        //    {
        //        var token = new OAuthTokenCredential
        //            (
        //            "AR69XxBhCpF6da9mKqlNGqDicibcSUmxKD_CgUxGQrWDWffqTdbAHNcuxZei",
        //            "EB4RwxAh6oydbdip-c6h1rxZXP4JDlwWci_Tyqf0oGB5G2JsoQe9d49-s61U",
        //            GetConfigurationTest()
        //            ).GetAccessToken();

        //        return token;
        //    }
        //}

        //public static APIContext GetAPIContextTest()
        //{
        //        var context = new APIContext(AccessTokenTest);
        //        context.Config = GetConfigurationTest();
        //        return context;
        //}

        //private static Dictionary<string, string> GetConfigurationTest()
        //{
        //    Dictionary<string, string> configurationMap = new Dictionary<string, string>();
        //    configurationMap.Add("mode", "sandbox");
        //    return configurationMap;
        //}  
    }
}
