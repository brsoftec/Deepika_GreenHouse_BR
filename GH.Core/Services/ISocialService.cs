using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GH.Core.Models;
using MongoDB.Bson;
using GH.Core.ViewModels;
using MongoDB.Driver;

namespace GH.Core.Services
{
    public interface ISocialService
    {

        /// <summary>
        /// Get social post by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SocialPost GetById(ObjectId id);

        /// <summary>
        /// insert social post to mongodb
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        SocialPost CreatePost(SocialPost post);

        /// <summary>
        /// Insert or update post. Only use for pull post from social network
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        SocialPost InsertOrUpdatePostPullFromSocialNetwork(SocialPost post);

        /// <summary>
        /// Delete post by id
        /// </summary>
        /// <param name="account"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        IEnumerable<SocialPost> DeletePost(Account account, ObjectId postId);

        SocialPost DeleteSocialPost(SocialPost socialPost);

        /// <summary>
        /// get feed of account2 from account1 with permission
        /// </summary>
        /// <param name="account1"></param>
        /// <param name="account2"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        Task<dynamic> GetFeed(ObjectId account1, ObjectId account2, Permission permission);

        /// <summary>
        /// Find post by social-id
        /// </summary>
        /// <param name="socialId"></param>
        /// <returns></returns>
        Task<SocialPost> FindPostBySocialId(string socialId);

        Task<List<SocialPost>> GetPostByListId(List<ObjectId> ids);

        Task<List<SocialPost>> GetHomeFeed(ObjectId accountId, int start = 0, int take = 20);

        /// <summary>
        /// add comment to one facebook post on green house
        /// </summary>
        /// <param name="account"></param>
        /// <param name="socialId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Comment AddNewComment(ObjectId accountId, ObjectId socialId, string message);

        SocialPost PostNewPost(PostSocialNetworkViewModel postViewModel, string accessToken, string secretToken, string userSocialid);

        /// <summary>
        /// Get comment by social post
        /// </summary>
        /// <param name="socialPostId"></param>
        /// <returns></returns>
        IList<Comment> GetCommentBySocialPost(ObjectId socialPostId);

        /// <summary>
        /// Share post to social network
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="postId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ShareResult SharePost(ObjectId accountId, ObjectId postId, SocialType type,string message, string groupId = null);

        /// <summary>
        /// Get new fedd from Facebook
        /// </summary>
        /// <param name="rAccount"></param>
        /// <param name="lastestFeedOfFacebook"></param>
        /// <param name="fbAccessToken"></param>
        /// <returns></returns>
        List<SocialPost> GetFeedFromFacebook(Account rAccount, SocialPost lastestFeedOfFacebook, string fbAccessToken);

        /// <summary>
        /// Get new feed from Twitter
        /// </summary>
        /// <param name="rAccount"></param>
        /// <param name="lastestFeedOfTwitter"></param>
        /// <param name="tw_access_token"></param>
        /// <param name="tw_secret_token"></param>
        /// <returns></returns>
        List<SocialPost> GetFeedFromTwitter(Account rAccount, SocialPost lastestFeedOfTwitter, string tw_access_token, string tw_secret_token, string socialAccountId);

        /// <summary>
        /// Get last post by account and type
        /// </summary>
        /// <param name="rAccountId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        SocialPost GetLastPost(ObjectId rAccountId, SocialType type);

        /// <summary>
        /// Update post share by account
        /// </summary>
        /// <param name="rAccountId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        UpdateResult UpdatePostShareCount(ObjectId socialPostId, int shares);

        /// <summary>
        /// Get page's post
        /// </summary>
        /// <param name="pageid"></param>
        /// <returns></returns>
        List<SocialPost> GetPageFeed(string pageid);

        /// <summary>
        /// Get all post related post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        IEnumerable<SocialPost> GetAllRelatedPostByPostId(ObjectId postId);

        List<SocialPost> GetFacebookFeedByBusinessAccount(Account rAccount, string fbAccessToken, DateTime fromDate);

        List<SocialPost> GetFacebookFeedByPersonalAccount(Account rAccount, string fbAccessToken, DateTime fromDate);

        List<SocialPost> UpdateCommentForUnknowFacebookUser(string fbAccessToken, List<string> socialAccountIds,
            List<SocialPost> newPullPosts);

        UpdateResult UpdateCommentOnRegit(SocialPost socialPost, Comment comment, FilterDefinition<SocialPost> filter);

        Comment CommentOnFacebook(string message, Account account, SocialPost socialPost, Comment comment);

        Comment CommentOnTwitter(string message, Account account, SocialPost socialPost, Comment comment);
    }
}
