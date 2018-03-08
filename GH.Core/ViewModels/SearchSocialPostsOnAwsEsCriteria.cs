using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SearchSocialPostsOnAwsEsCriteria : IAwsEsSearchDocument
    {
        public string SearchUser { get; set; }
        public string[] CreatorIds { get; set; }
        public string[] NotCreatorIds { get; set; }
        public string Keyword { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public SocialType[] SocialNetwork { get; set; }
        public bool SearchForGlobal { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }

        public string ToEsQueryString()
        {
            List<string> queries = new List<string>();

            queries.Add("_missing_:GroupId");
            
            if (this.CreatorIds != null && this.CreatorIds.Length != 0)
            {
                string query = string.Format("CreatorId:({0})", string.Join(" OR ", this.CreatorIds));
                queries.Add(query);
            }

            if (this.NotCreatorIds != null && this.NotCreatorIds.Length != 0)
            {
                string query = string.Format("NOT CreatorId:({0})", string.Join(" OR ", this.NotCreatorIds));
                queries.Add(query);
            }

            if (!string.IsNullOrEmpty(this.Keyword))
            {
                string query = string.Format("Message:({0})", this.Keyword);
                queries.Add(query);
            }

            string timeRangeQuery = "(ModifiedTime:[{0} TO {1}] OR CreatedTime:[{0} TO {1}])";

            if (FromTime.HasValue && ToTime.HasValue)
            {
                string query = string.Format(timeRangeQuery, this.FromTime.Value.ToString("o"), this.ToTime.Value.ToString("o"));
                queries.Add(query);
            }
            else if (FromTime.HasValue && !ToTime.HasValue)
            {
                string query = string.Format(timeRangeQuery, this.FromTime.Value.ToString("o"), "*");
                queries.Add(query);
            }
            else if (!FromTime.HasValue && ToTime.HasValue)
            {
                string query = string.Format(timeRangeQuery, "*", this.ToTime.Value.ToString("o"));
                queries.Add(query);
            }

            if (SocialNetwork != null && SocialNetwork.Length != 0)
            {
                string query = string.Format("(SocialNetwork:\"{0}\")", string.Join("\" OR SocialNetwork:\"", this.SocialNetwork.Select(s => (int)s)));
                queries.Add(query);
            }

            if (SearchForGlobal)
            {
                string query = string.Format("PostPrivacy.Type:\"{0}\"", (int)PostPrivacyType.Public);
                queries.Add(query);
            }
            else
            {
                string query = string.Format("(CreatorId:{0} OR ((NOT CreatorId:{0} AND (PostPrivacy.Type:\"{1}\" OR PostPrivacy.Type:\"{2}\"))))", SearchUser, (int)PostPrivacyType.Public, (int)PostPrivacyType.Friends);
                queries.Add(query);
            }
            
            string queryString = string.Join(" AND ", queries);

            return queryString;
        }
    }
}