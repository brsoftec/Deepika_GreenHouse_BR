using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace GH.Core.Helpers
{
    public static class NotificationTemplateHelper
    {
        public static NotificationContent ReplacePlaceholderFromTemplate<T>(NotificationTemplate template, T value)
        {
            //regular expression get all sub-string in pattern
            //example: {{substring}}abc{{substring2}}
            //regex - example for {{ }}: /{{([^\}]+)}}
            string pattern = template.OpenPlaceHolder + "([^" + template.ClosePlaceHolder + "]+)" + template.ClosePlaceHolder;

            //find matches in Title of template
            var matchTitle = Regex.Match(template.Title, pattern);
            //find matches in Body of template
            var matchBody = Regex.Match(template.Body, pattern);

            //contain all placeholder of title and body
            List<string> allPlaceHolders = new List<string>();

            //while has placeholder
            while (matchTitle.Value != "")
            {
                allPlaceHolders.Add(matchTitle.Value.Replace(template.OpenPlaceHolder, "").Replace(template.ClosePlaceHolder, ""));
                matchTitle = matchTitle.NextMatch();
            }

            while (matchBody.Value != "")
            {
                allPlaceHolders.Add(matchBody.Value.Replace(template.OpenPlaceHolder, "").Replace(template.ClosePlaceHolder, ""));
                matchBody = matchBody.NextMatch();
            }

            //distinct placeholders for optimized loop
            allPlaceHolders = allPlaceHolders.Distinct().ToList();

            //reflect type of model
            PropertyInfo[] props = typeof(T).GetProperties();

            //assign basic value to return model
            NotificationContent result = new NotificationContent();
            result.Title = template.Title;
            result.Body = template.Body;

            //loop all received placeholders and replace it with real value
            foreach (var item in allPlaceHolders)
            {
                string placeHolder = template.OpenPlaceHolder + item + template.ClosePlaceHolder;
                string replaceTo = GetValueOfProperty<T>(item, value).ToString();
                result.Title = result.Title.Replace(placeHolder, replaceTo);
                result.Body = result.Body.Replace(placeHolder, replaceTo);
            }

            return result;
        }

        /// <summary>
        /// Get value by property name
        /// Property Name can be a navigate to child property of navigation property
        /// Example: Assignee.Name. Assignee is name of Navigation Property of Workplan
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValueOfProperty<T>(string propertyName, T value)
        {
            string[] navs = propertyName.Split('.');
            PropertyInfo[] props = typeof(T).GetProperties();
            var prop = props.FirstOrDefault(p => p.Name == navs[0]);
            if (navs.Length == 1)
            {
                return prop.GetValue(value);
            }
            else
            {
                var nextInject = navs.Skip(1).ToArray();
                var navVal = prop.GetValue(value);
                var type = navVal.GetType();
                MethodInfo method = typeof(NotificationTemplateHelper).GetMethod("GetValueOfProperty",
                    BindingFlags.Public | BindingFlags.Static);

                // Build a method with the specific type argument you're interested in
                method = method.MakeGenericMethod(type);

                object[] param = { string.Join(".", nextInject), navVal };

                object res = method.Invoke(null, param);
                return res;
            }
        }
    }
}