using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface ISocialPostService
    {
        IList<SocialPost> GetByIds(IEnumerable<ObjectId> ids);
        IList<SocialPost> GetPostsLaterThanDate(DateTime date);
        IList<SocialPost> GetRelatedGroupPost(List<string> groupIds, string ParrentId=null);
    }
}