using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TwitterHomeTimelineCriteria
    {
        public int? Count { get; set; }
        public int? SinceId { get; set; }
        public int? MaxId { get; set; }
        public bool? TrimUser { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? ContributorDetails { get; set; }
        public bool? IncludeEntities { get; set; }

        public IDictionary<string, string> GetDictionary()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();


            if (Count.HasValue)
            {
                dictionary.Add("count", Count.Value.ToString());
            }

            if (SinceId.HasValue)
            {
                dictionary.Add("since_id", SinceId.Value.ToString());
            }

            if (MaxId.HasValue)
            {
                dictionary.Add("max_id", MaxId.Value.ToString());
            }

            if (TrimUser.HasValue)
            {
                dictionary.Add("trim_user", TrimUser.Value.ToString().ToLower());
            }

            if (ExcludeReplies.HasValue)
            {
                dictionary.Add("exclude_replies", ExcludeReplies.Value.ToString().ToLower());
            }

            if (ContributorDetails.HasValue)
            {
                dictionary.Add("contributor_details", ContributorDetails.Value.ToString().ToLower());
            }

            if (IncludeEntities.HasValue)
            {
                dictionary.Add("include_entities", IncludeEntities.Value.ToString().ToLower());
            }

            return dictionary;
        }

    }
}