using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GH.Core.Models;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Models;

namespace GH.Core.Collectors
{
    public interface ITwitterCollector
    {
        Task<dynamic> VerifyCredentials(string oauthToken, string oauthTokenSecret, bool includeEntities, bool skipStatus, bool includeEmail);

        /// <summary>
        /// API for getting twitter user's home timeline
        /// </summary>
        Task<dynamic> GetHomeTimeline(string oauthToken, string oauthTokenSecret, TwitterHomeTimelineCriteria criteria);

        Task<dynamic> UpdateStatus(string oauthToken, string oauthTokenSecret, TwitterUpdateStatusModel status);

        //Task<dynamic> Favorite(string oauthToken, string oauthTokenSecret, TwitterFavoritePostModel favorite);

        //Task<dynamic> Unfavorite(string oauthToken, string oauthTokenSecret, TwitterFavoritePostModel favorite);

        /// <summary>
        /// Like the tweet
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Favorite(string access_token, string access_token_secret, string id);

        /// <summary>
        /// Unlike the tweet
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Unfavorite(string access_token, string access_token_secret, string id);

        Task<dynamic> Retweet(string oauthToken, string oauthTokenSecret, TwitterRetweetModel retweet);

        Task<dynamic> Unretweet(string oauthToken, string oauthTokenSecret, TwitterRetweetModel retweet);

        /// <summary>
        /// get single tweet by id
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <returns></returns>
        ITweet GetTweet(string access_token,string access_token_secret, string tId);

        bool DestroyTweet(string access_token, string access_token_secret, string tId);

        /// <summary>
        /// get home tweet for current user
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <returns></returns>
        IList<ITweet> GetHomeTweet(string id, string access_token, string access_token_secret, int limit = 25);

        /// <summary>
        /// post on twitter a feed or share a green house post with multiple images
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <param name="socialPost"></param>
        /// <returns></returns>
        SocialPost PostTweet(string access_token, string access_token_secret, SocialPost socialPost);

        /// <summary>
        /// post on twitter a feed or share a green house post with multiple images
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <param name="socialPost"></param>
        /// <param name="reply_id"></param>
        /// <returns></returns>
        SocialPost ReplyTweet(string access_token, string access_token_secret, SocialPost socialPost,string reply_id);

        /// <summary>
        /// post tweet with multiple image by list of byte[]
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="access_token_secret"></param>
        /// <param name="socialPost"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        Task<SocialPost> PostTweetWithMultiplePhotoByteData(string access_token, string access_token_secret,
            SocialPost socialPost, List<byte[]> images);
    }
}