using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Collectors
{
    public class SocialApiUrl
    {
        #region Facebook
        public const string API_LONG_LIVED_TOKEN = "https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id={0}&client_secret={1}&fb_exchange_token={2}";

        public const string API_USER_CODE_FROM_LONG_LIVED_TOKEN = "https://graph.facebook.com/oauth/client_code?client_id={0}&client_secret={1}&access_token={2}&redirect_uri={3}";

        /// <summary>
        /// https://graph.facebook.com/v2.6/me/feed
        /// </summary>
        public const string API_USER_FEEDS = "https://graph.facebook.com/v2.6/me/feed";

        public const string API_POST_PHOTO = "https://graph.facebook.com/v2.6/me/photos";

        public const string API_POST_VIDEO = "https://graph.facebook.com/v2.6/me/videos";

        public const string API_USER_FEEDS_WITH_ALL_INFORMATION = "https://graph.facebook.com/v2.6/me/feed?fields=id,message,privacy,created_time,story,full_picture,attachments{subattachments{media{image{src}}}},source,comments.limit(10000),likes.summary(true).limit(10000),sharedposts.limit(10000),shares&since={since}";

        public const string API_PAGE_FEEDS_WITH_ALL_INFORMATION = "https://graph.facebook.com/v2.6/{fbId}/feed?fields=id,message,privacy,created_time,story,full_picture,attachments{subattachments{media{image{src}}}},source,comments.limit(10000),likes.summary(true).limit(10000),sharedposts.limit(10000),shares&since={since}";

        public const string API_POST_NEW_PAGE_FEED = "https://graph.facebook.com/v2.6/{0}/feed";

        public const string API_POST_DELETE = "https://graph.facebook.com/v2.6/{0}";

        public const string API_PUBLISH_PHOTO_ACTION = "https://graph.facebook.com/v2.6/{0}/{1}";


        /// <summary>
        /// https://graph.facebook.com/v2.6/me/photos
        /// </summary>
        public const string API_USER_PHOTOS = "https://graph.facebook.com/v2.6/me/photos";

        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}/likes.
        /// param {0}: id of post
        /// </summary>
        public const string API_GET_LIKES_OF_POST = "https://graph.facebook.com/v2.6/{0}/likes?limits=100000";

        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}/likes.
        /// param {0}: id of post
        /// </summary>
        public const string API_LIKES_OF_POST = "https://graph.facebook.com/v2.6/{0}/likes";

        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}/comments.
        /// param {0}: id of post
        /// </summary>
        public const string API_GET_COMMENTS_OF_POST = "https://graph.facebook.com/v2.6/{0}/comments";

        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}/comments.
        /// param {0}: id of post
        /// </summary>
        public const string API_COMMENTS_OF_POST = "https://graph.facebook.com/v2.6/{0}/comments";


        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}?fields=picture.
        /// param {0}: id of post
        /// </summary>
        public const string API_GET_PICTURES_OF_POST = "https://graph.facebook.com/v2.6/{0}?fields=full_picture";

        /// <summary>
        /// https://graph.facebook.com/v2.6/{0}/sharedposts.
        /// param {0}: id of post
        /// </summary>
        public const string API_GET_SHARES_OF_POST = "https://graph.facebook.com/v2.6/{0}/sharedposts";

        public const string API_GET_VIDEO_OF_POST = "https://graph.facebook.com/v2.6/{0}?fields=source";

        public const string API_GET_ALL_INFORMATION_OF_POST = "https://graph.facebook.com/v2.6/{0}?fields=id,message,privacy,created_time,story,full_picture,attachments{subattachments{media{image{src}}}},source,comments.limit(10000),likes.summary(true).limit(10000),shares";

        public const string API_GET_LIST_FRIEND = "https://graph.facebook.com/v2.6/?ids={0}&fields=name,link,picture.type(large){url}";
        public const string API_GET_LIST_PAGE = "https://graph.facebook.com/v2.6/me/accounts";
        public const string API_GET_PAGE_FEED = "https://graph.facebook.com/v2.6/{0}/feed";
        #endregion
    }
}