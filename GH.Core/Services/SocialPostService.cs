using GH.Core.Extensions;
using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GH.Core.Services
{
    public class SocialPostService : ISocialPostService
    {
        public IList<SocialPost> GetByIds(IEnumerable<ObjectId> ids)
        {
            var result = MongoContext.Db.SocialPosts.Find(c => ids.Contains(c.Id));
            return result.ToList();
        }

        public IList<SocialPost> GetPostsLaterThanDate(DateTime date)
        {
            var result = MongoContext.Db.SocialPosts.Find(c => c.ModifiedOn >= date || (c.PullAt != null && c.PullAt >= date));

            return result.ToList();
        }

        public IList<SocialPost> GetRelatedGroupPost(List<string> groupIds, string ParentID=null)
        {
            if (ParentID!=null)
            {
                ObjectId parentId = ObjectId.Parse(ParentID);
                return MongoContext.Db.SocialPosts.Find(s => groupIds.Contains(s.GroupId) || parentId==s.Id).ToList();
            }
            else
            {
                return MongoContext.Db.SocialPosts.Find(s => groupIds.Contains(s.GroupId)).ToList();
            }            
        }
    }
}