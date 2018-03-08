using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GH.Core.Collectors
{
    public interface IFacebookCollector
    {
        /// <summary>
        /// Get long lived token from short lived token
        /// </summary>
        /// <param name="shortLivedAccessToken">Short lived access token</param>
        /// <returns></returns>
        Task<string> GetLongLivedToken(string shortLivedAccessToken);

        /// <summary>
        /// Get user code from long lived token
        /// </summary>
        /// <param name="longLivedAccessToken"></param>
        /// <returns></returns>
        Task<string> GetUserCodeFromLongLivedToken(string longLivedAccessToken);

        /// <summary>
        /// Get new feed with comments, likes, shared in every post
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        dynamic PullNewFeed(string accessToken, DateTime date);

        /// <summary>
        /// Unlikes of one's post in Facebook
        /// </summary>
        /// <param name="accessToken">access token given by Facebook</param>
        /// <param name="postId">id of one post in Facebook</param>
        /// <returns></returns>
        dynamic UnLikesAPost(string accessToken, string postId);

        /// <summary>
        /// likes of one's post in Facebook
        /// </summary>
        /// <param name="accessToken">access token given by Facebook</param>
        /// <param name="postId">id of one post in Facebook</param>
        /// <returns></returns>
        dynamic LikesAPost(string accessToken, string postId);

        /// <summary>
        /// get likes of one's post in Facebook
        /// </summary>
        /// <param name="accessToken">access token given by Facebook</param>
        /// <param name="postId">id of one post in Facebook</param>
        /// <returns></returns>
        Task<dynamic> GetLikesOfPost(string accessToken, string postId);

        /// <summary>
        /// get comments of one's post in Facebook
        /// </summary>
        /// <param name="accessToken">access token given by Facebook</param>
        /// <param name="postId">id of one post in Facebook</param>
        /// <returns></returns>
        Task<dynamic> GetCommentOfPost(string accessToken, string postId);

        /// <summary>
        /// get link picture of one's post
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<dynamic> GetPictureOfPost(string accessToken, string postId);

        /// <summary>
        /// get shares about this post through postID
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="postID"></param>
        /// <returns></returns>
        Task<dynamic> GetShareOfPost(string accessToken, string postID);

        /// <summary>
        /// post a feed to facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="imagePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        dynamic PostFeed(string userSocialId, string accessToken, string content, string[] imagePath = null, PostPrivacyType privacy = PostPrivacyType.Public);

        /// <summary>
        /// Post new feed on page
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="accessToken"></param>
        /// <param name="content"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        dynamic PostPageFeed(string pageId, string accessToken, string content, string[] imagePath = null);

        /// <summary>
        /// post an image to facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        dynamic PostImage(string accessToken, string imagePath, string message = "", bool published = true, bool isInternetPath = false, string privacy= null);

        /// <summary>
        /// Post video to Facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="videoUrl"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        dynamic PostVideo(string accessToken, string imagePath, string message = "");

        /// <summary>
        /// share a facebook post in green house to facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        SocialPost SharePost(string accessToken, SocialPost post);

        /// <summary>
        /// post an image to facebook with a array byte
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> PostImageWithDataByteArray(string accessToken, byte[] data, string fileName);

        /// <summary>
        /// post a new feed with message and a photo id after post photo to facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="imageId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<string> PostFeedWithImageId(string accessToken, string imageId, string message);

        /// <summary>
        /// Get all comments, likes and picture of post
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        dynamic GetAllInformationOfPost(string accessToken, string postId);

        /// <summary>
        /// Get list friend by ids
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<dynamic> GetListFriend(string accessToken, List<string> ids);

        /// <summary>
        /// Get list of page which user have role
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        dynamic GetListPages(string accessToken);

        /// <summary>
        /// Get list of page posts
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="pageId"></param>
        /// <param name="since"></param>
        /// <returns></returns>
        dynamic GetPagePosts(string accessToken, string pageId, DateTime since);

        /// <summary>
        /// Delete the post by id
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        bool DeletePost(string accessToken, string postId);

        /// <summary>
        /// comment on post
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="postId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        dynamic Comment(string accessToken, string postid, string comment);

    }
}