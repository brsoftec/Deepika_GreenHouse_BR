using GH.Core.Helpers;
using GH.Web.Areas.User.Configs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Resources;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{

    [RoutePrefix("api/Translators")]
    public class LanguageApiController : ApiController
    {
        [HttpGet, Route("")]
        public Dictionary<string, string> GetDictionary(string language)
        {
            ResourceSet resourceSet = GH.Lang.Regit.ResourceManager.GetResourceSet(CultureInfo.CreateSpecificCulture(language), true, true);
            var dictionary = resourceSet.OfType<DictionaryEntry>().ToDictionary(d => d.Key.ToString(), d => d.Value.ToString());
            dictionary.Add("TRANSLATION_VERSION", ConfigHelper.GetConfig<TranslationVersion>().Version);
            return dictionary;
        }
        
        [HttpGet, Route("Version")]
        public string GetLastTranslateDate()
        {
            return ConfigHelper.GetConfig<TranslationVersion>().Version;
        }

        [HttpGet, Route("Languages")]
        public dynamic GetLanguages()
        {
            return JArray.Parse(File.ReadAllText(CommonFunctions.MapPath("~/Areas/User/Configs/languages.json"))).ToObject<dynamic[]>();
        }
    }
}
