using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TwitterFavoritePostModel
    {
        [Required]
        public string Id { get; set; }
        public bool? IncludeEntities { get; set; }

        public IDictionary<string, string> GetDictionary()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            dictionary.Add("id", Id);

            if (IncludeEntities.HasValue)
            {
                dictionary.Add("include_entities", IncludeEntities.Value.ToString().ToLower());
            }
            
            return dictionary;
        }
    }
}