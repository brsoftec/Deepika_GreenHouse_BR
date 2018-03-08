using GH.Core.Collectors;
using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public class SocialPageService : ISocialPageService
    {

        private IFacebookCollector _facebookCollector;
        private IMongoCollection<BusinessPost> _businessPostCollection;
        private IAccountService _accountService;
        public SocialPageService()
        {
            var mongoDb = MongoContext.Db;
            _facebookCollector = new FacebookCollector();
            _accountService = new AccountService();
            _businessPostCollection = mongoDb.BusinessPosts;
        }
        public BusinessPost GetBusinessPost(ObjectId id)
        {
            return _businessPostCollection.Find(s => s.Id == id).FirstOrDefault();
        }

        public BusinessPost Insert(BusinessPost post)
        {
            _businessPostCollection.InsertOne(post);
            return post;
        }

        public BusinessPost Update(BusinessPost post)
        {
            _businessPostCollection.UpdateOne(Builders<BusinessPost>.Filter.Eq(s => s.Id, post.Id), Builders<BusinessPost>.Update.Set(t => t.Message, post.Message)
                                                      .Set(t => t.Photos, post.Photos)
                                                      .Set(t => t.VideoUrl, post.VideoUrl));

            return post;
        }

        public bool Delete(ObjectId id)
        {
            try
            {
                _businessPostCollection.DeleteOne(Builders<BusinessPost>.Filter.Eq(s => s.Id, id));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<BusinessPost> GetListBusinessPost(ObjectId accountId, ObjectId businessAccountId, int start, int take)
        {
            return _businessPostCollection.Find(Builders<BusinessPost>.Filter.And(new FilterDefinition<BusinessPost>[]{
                Builders<BusinessPost>.Filter.Eq(s=>s.AccountId,accountId),
                Builders<BusinessPost>.Filter.Eq(s=>s.BusinessAccountId,businessAccountId)
            })).Skip(start).Limit(take).ToList();
        }

        public BusinessPost RejectPost(ObjectId postId, string comment, ObjectId executorId)
        {
            var post = _businessPostCollection.Find(p => p.Id == postId).FirstOrDefault();
            if (post == null)
            {
                throw new CustomException("Rejecting post does not exist");
            }

            IRoleService roleService = new RoleService();
            var reviewRole = roleService.GetRoleByName(Role.ROLE_REVIEWER);

            var executor = _accountService.GetById(executorId);
            if (executor.Id != post.BusinessAccountId && !executor.BusinessAccountRoles.Any(b => b.AccountId == post.BusinessAccountId && b.RoleId == reviewRole.Id))
            {
                throw new CustomException("Permission denied");
            }

            if (post.Status == PostStatus.Approved || post.Status == PostStatus.Posted)
            {
                throw new CustomException("The post has been approved or posted");
            }

            if (post.Status == PostStatus.Rejected)
            {
                throw new CustomException("The post has already been rejected");
            }

            var flow = new PostStateFlow
            {
                Action = PostStateAction.Reject,
                Comment = comment,
                ExecuteTime = DateTime.Now,
                Executor = executorId
            };

            post.Status = PostStatus.Rejected;
            post.Workflows.Add(flow);

            _businessPostCollection.UpdateOne(b => b.Id == post.Id, Builders<BusinessPost>.Update.Set(f => f.Status, PostStatus.Rejected).AddToSet(f => f.Workflows, flow));
            return post;
        }

        public BusinessPost ApprovePost(ObjectId postId, ObjectId executorId)
        {
            var post = _businessPostCollection.Find(p => p.Id == postId).FirstOrDefault();
            if (post == null)
            {
                throw new CustomException("Approving post does not exist");
            }

            IRoleService roleService = new RoleService();
            var reviewRole = roleService.GetRoleByName(Role.ROLE_REVIEWER);

            var executor = _accountService.GetById(executorId);
            if (executor.Id != post.BusinessAccountId && !executor.BusinessAccountRoles.Any(b => b.AccountId == post.BusinessAccountId && b.RoleId == reviewRole.Id))
            {
                throw new CustomException("Permission denied");
            }

            if (post.Status == PostStatus.Rejected)
            {
                throw new CustomException("The post has been rejected");
            }


            if (post.Status == PostStatus.Approved || post.Status == PostStatus.Posted)
            {
                throw new CustomException("The post has already been approved or posted");
            }

            var flow = new PostStateFlow
            {
                Action = PostStateAction.Approve,
                ExecuteTime = DateTime.Now,
                Executor = executorId
            };

            post.Status = PostStatus.Approved;
            post.Workflows.Add(flow);

            _businessPostCollection.UpdateOne(b => b.Id == post.Id, Builders<BusinessPost>.Update.Set(f => f.Status, PostStatus.Approved).AddToSet(f => f.Workflows, flow));
            return post;
        }

        public BusinessPost EditPost(BusinessPost post, ObjectId editor)
        {
            var existPost = _businessPostCollection.Find(p => p.Id == post.Id).FirstOrDefault();
            if (existPost == null)
            {
                throw new CustomException("Editing post does not exist");
            }

            IRoleService roleService = new RoleService();
            var editorRole = roleService.GetRoleByName(Role.ROLE_EDITOR);

            var executor = _accountService.GetById(editor);
            if (executor.Id != existPost.BusinessAccountId && !executor.BusinessAccountRoles.Any(b => b.AccountId == existPost.BusinessAccountId && b.RoleId == editorRole.Id))
            {
                throw new CustomException("Permission denied");
            }

            if (existPost.Status == PostStatus.Approved || existPost.Status == PostStatus.Posted)
            {
                throw new CustomException("The post has already been approved or posted");
            }

            existPost.Photos = post.Photos;
            existPost.VideoUrl = post.VideoUrl;
            existPost.Privacy = post.Privacy;
            existPost.SocialTypes = post.SocialTypes;
            existPost.Status = PostStatus.Edited;

            var flow = new PostStateFlow
            {
                Action = PostStateAction.Repost,
                ExecuteTime = DateTime.Now,
                Executor = editor
            };
            existPost.Workflows.Add(flow);

            _businessPostCollection.UpdateOne(Builders<BusinessPost>.Filter.Eq(s => s.Id, post.Id), Builders<BusinessPost>.Update.Set(t => t.Message, post.Message)
                                                    .Set(t => t.Photos, post.Photos)
                                                    .Set(t => t.VideoUrl, post.VideoUrl)
                                                    .Set(t => t.Privacy, post.Privacy)
                                                    .Set(t => t.SocialTypes, post.SocialTypes)
                                                    .Set(t => t.Status, PostStatus.Edited)
                                                    .AddToSet(t => t.Workflows, flow));

            return existPost;
        }
    }
}