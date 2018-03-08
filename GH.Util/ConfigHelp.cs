using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Util
{
    public class ConfigHelp
    {
        public static String GetStringValue(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            return string.IsNullOrEmpty(value) ? "" : value;

        }
        public static bool GetBoolValue(string key)
        {
            string value = GetStringValue(key);
            bool valueint = false;
            bool.TryParse(value, out valueint);
            return valueint;
        }
        public static int GetIntValue(string key)
        {
            string value = GetStringValue(key);
            int valueint = 0;
            int.TryParse(value, out valueint);
            return valueint;
        }
    }
}
