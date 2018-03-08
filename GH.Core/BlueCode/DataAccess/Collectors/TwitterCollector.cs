using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace GH.Core.Collectors
{
    public class TwitterCollector : ITwitterCollector
    {
        private IMongoCollection<SocialPost> _socialPostCollection;

        private string _consumerKey { get { return ConfigurationManager.AppSettings["TWITTER_CONSUMER_KEY"]; } }
        private string _consumerSecret { get { return ConfigurationManager.AppSettings["TWITTER_CONSUMER_SECRET"]; } }

        public TwitterCollector()
        {
            _socialPostCollection = MongoContext.Db.SocialPosts;
        }

        public async Task<dynamic> VerifyCredentials(string oauthToken, string oauthTokenSecret, bool includeEntities, bool skipStatus, bool includeEmail)
        {
            string requestUrl = string.Format(TwitterHelper.TWITTER_API_BASE_URL + TwitterHelper.ACCOUNT_VERIFY_CREDENTIALS);

            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("include_entities", includeEntities.ToString().ToLower());
            parameters.Add("skip_status", skipStatus.ToString().ToLower());
            parameters.Add("include_email", includeEmail.ToString().ToLower());

            var headers = TwitterHelper.GenerateHeaders(_consumerKey, _consumerSecret, oauthToken, oauthTokenSecret, requestUrl, parameters, TwitterApiHttpMethod.GET);

            requestUrl = requestUrl + "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value));

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", headers.ToString());

            var response = client.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dynamic result = JObject.Parse(data);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }

        }

        public async Task<dynamic> GetHomeTimeline(string oauthToken, string oauthTokenSecret, TwitterHomeTimelineCriteria criteria)
        {
            string requestUrl = string.Format(TwitterHelper.TWITTER_API_BASE_URL + TwitterHelper.STATUS_HOME_TIMELINE);

            var parameters = criteria != null ? criteria.GetDictionary() : new Dictionary<string, string>();

            var headers = TwitterHelper.GenerateHeaders(_consumerKey, _consumerSecret, oauthToken, oauthTokenSecret, requestUrl, parameters, TwitterApiHttpMethod.GET);

            requestUrl = requestUrl + "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value));

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", headers.ToString());

            var response = client.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dynamic result = JArray.Parse(data);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public async Task<dynamic> UpdateStatus(string oauthToken, string oauthTokenSecret, TwitterUpdateStatusModel status)
        {
            string requestUrl = string.Format(TwitterHelper.TWITTER_API_BASE_URL + TwitterHelper.STATUS_UPDATE);

            var parameters = status.GetDictionary();

            var headers = TwitterHelper.GenerateHeaders(_consumerKey, _consumerSecret, oauthToken, oauthTokenSecret, requestUrl, parameters, TwitterApiHttpMethod.POST);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", headers.ToString());

            FormUrlEncodedContent contentBody = new FormUrlEncodedContent(parameters.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)));
            var response = client.PostAsync(requestUrl, contentBody).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dynamic result = JObject.Parse(data);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public bool Favorite(string access_token, string access_token_secret, string id)
        {
            AuthenticateUser(access_token, access_token_secret);
            long _id = long.Parse(id);
            return Tweet.FavoriteTweet(_id);
        }

        public bool Unfavorite(string access_token, string access_token_secret, string id)
        {
            AuthenticateUser(access_token, access_token_secret);
            long _id = long.Parse(id);
            return Tweet.UnFavoriteTweet(_id);
        }

        public async Task<dynamic> Retweet(string oauthToken, string oauthTokenSecret, TwitterRetweetModel retweet)
        {
            string requestUrl = string.Format(TwitterHelper.TWITTER_API_BASE_URL + TwitterHelper.STATUS_RETWEET, retweet.Id);

            var parameters = retweet.GetDictionary();

            var headers = TwitterHelper.GenerateHeaders(_consumerKey, _consumerSecret, oauthToken, oauthTokenSecret, requestUrl, parameters, TwitterApiHttpMethod.POST);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", headers.ToString());

            FormUrlEncodedContent contentBody = new FormUrlEncodedContent(parameters.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)));
            var response = client.PostAsync(requestUrl, contentBody).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dynamic result = JObject.Parse(data);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public async Task<dynamic> Unretweet(string oauthToken, string oauthTokenSecret, TwitterRetweetModel retweet)
        {
            string requestUrl = string.Format(TwitterHelper.TWITTER_API_BASE_URL + TwitterHelper.STATUS_UNRETWEET, retweet.Id);

            var parameters = retweet.GetDictionary();

            var headers = TwitterHelper.GenerateHeaders(_consumerKey, _consumerSecret, oauthToken, oauthTokenSecret, requestUrl, parameters, TwitterApiHttpMethod.POST);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", headers.ToString());

            FormUrlEncodedContent contentBody = new FormUrlEncodedContent(parameters.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)));
            var response = client.PostAsync(requestUrl, contentBody).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dynamic result = JObject.Parse(data);
                return result;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public ITweet GetTweet(string access_token, string access_token_secret, string tId)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                long id = Int64.Parse(tId);
                return Tweet.GetTweet(id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public bool DestroyTweet(string access_token, string access_token_secret, string tId)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                long id = Int64.Parse(tId);
                return Tweet.DestroyTweet(id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public IList<ITweet> GetHomeTweet(string id, string access_token, string access_token_secret, int limit = 25)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                //var date = new DateTime(2016, 06, 13);

                //var tweets = Search.SearchTweets(new TweetSearchParameters("")
                //{
                //    Since = date.AddDays(-1),
                //    Until = date,
                //    SearchType = SearchResultType.Recent
                //}).Where(x => x.CreatedAt.DayOfYear == date.DayOfYear).ToArray();
                //return authenticatedUser.GetUserTimeline(limit).ToList();
                var _id = long.Parse(id);
                return Timeline.GetUserTimeline(_id, limit).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public SocialPost PostTweet(string access_token, string access_token_secret, SocialPost socialPost)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                var message = socialPost.Message;
                ITweet tweet = null;
                if (socialPost.Photos != null && socialPost.Photos.Count > 0)
                {
                    var listOfMedia = new List<IMedia>();
                    foreach (var photo in socialPost.Photos)
                    {
                        var webClient = new WebClient();
                        byte[] data = webClient.DownloadData(CommonFunctions.MapPath("~" + photo.Url));

                        var media = Upload.UploadImage(data);
                        listOfMedia.Add(media);
                    }
                    message = string.IsNullOrEmpty(message) ? " " : message;
                    tweet = Tweet.PublishTweet(message, new PublishTweetOptionalParameters
                    {
                        Medias = listOfMedia
                    });
                    if (tweet != null)
                        socialPost.SocialId = tweet.IdStr;
                }
                else
                {
                    tweet = Tweet.PublishTweet(message);
                    if (tweet != null)
                        socialPost.SocialId = tweet.IdStr;
                }
                if (tweet == null)
                    throw new CustomException("Can not post the post to Twitter. May be you posted it few minutes ago. Please check it on Twitter.");
                return socialPost;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public SocialPost ReplyTweet(string access_token, string access_token_secret, SocialPost socialPost, string reply_id)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                ITweet tweet = null;
                long replyId = 0;
                if (!string.IsNullOrEmpty(reply_id))
                    long.TryParse(reply_id, out replyId);
                var tweetToReplyTo = Tweet.GetTweet(replyId);
                var textToPublish = string.Format("@{0} {1}", tweetToReplyTo.CreatedBy.ScreenName, socialPost.Message);
                tweet = Tweet.PublishTweetInReplyTo(textToPublish, tweetToReplyTo);
                if (tweet != null)
                    socialPost.SocialId = tweet.IdStr;

                if (tweet == null)
                    throw new CustomException("Can not post the post to Twitter. May be you posted it few minutes ago. Please check it on Twitter.");
                return socialPost;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<SocialPost> PostTweetWithMultiplePhotoByteData(string access_token, string access_token_secret, SocialPost socialPost, List<byte[]> images)
        {
            try
            {
                AuthenticateUser(access_token, access_token_secret);
                var message = socialPost.Message;

                if (images != null)
                {
                    if (images.Count > 0)
                    {
                        var listOfMedia = new List<IMedia>();
                        foreach (var image in images)
                        {
                            listOfMedia.Add(Upload.UploadImage(image));
                            break;
                        }

                        var tweet = Tweet.PublishTweet(message, new PublishTweetOptionalParameters
                        {
                            Medias = listOfMedia
                        });

                        if (tweet != null)
                        {
                            socialPost.SocialId = tweet.IdStr;
                        }
                        return socialPost;
                    }
                }
                else
                {
                    var tweet = Tweet.PublishTweet(message);
                    if (tweet != null)
                    {
                        socialPost.SocialId = tweet.IdStr;
                    }
                    return socialPost;
                }

                return null;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        private void AuthenticateUser(string access_token, string access_token_secret)
        {
            var userCredentials = Auth.SetUserCredentials(_consumerKey, _consumerSecret, access_token, access_token_secret);
        }
    }
}