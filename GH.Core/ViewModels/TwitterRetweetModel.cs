using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TwitterRetweetModel
    {
        [Required]
        public string Id { get; set; }
        public bool? TrimUser { get; set; }

        public IDictionary<string, string> GetDictionary()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            dictionary.Add("id", Id);

            if (TrimUser.HasValue)
            {
                dictionary.Add("trim_user", TrimUser.Value.ToString().ToLower());
            }
            
            return dictionary;
        }
    }
}