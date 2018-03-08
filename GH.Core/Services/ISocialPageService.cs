using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface ISocialPageService
    {
        BusinessPost GetBusinessPost(ObjectId id);
        BusinessPost Insert(BusinessPost post);
        BusinessPost Update(BusinessPost post);
        bool Delete(ObjectId id);
        List<BusinessPost> GetListBusinessPost(ObjectId accountId,ObjectId businessAccountId, int start, int take);
        BusinessPost RejectPost(ObjectId postId, string comment, ObjectId executorId);
        BusinessPost ApprovePost(ObjectId postId, ObjectId executorId);
        BusinessPost EditPost(BusinessPost post, ObjectId editor);
    }
}