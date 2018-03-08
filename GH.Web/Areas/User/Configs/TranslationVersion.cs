using GH.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace GH.Web.Areas.User.Configs
{
    [XmlRoot("version")]
    [ConfigPath("~/Areas/User/Configs/translation-version.xml")]
    public class TranslationVersion
    {
        [XmlText]
        public string Version { get; set; }
    }
}