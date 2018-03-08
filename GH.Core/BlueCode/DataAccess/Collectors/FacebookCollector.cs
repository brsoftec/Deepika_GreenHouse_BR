using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Facebook;
using MongoDB.Driver;
using GH.Core.Extensions;
using GH.Core.Exceptions;
using GH.Core.Helpers;
using System.Dynamic;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Linq;

namespace GH.Core.Collectors
{
    public class FacebookCollector : IFacebookCollector
    {
        private IMongoCollection<SocialPost> _socialPostCollection;
        public FacebookCollector()
        {
            _socialPostCollection = MongoContext.Db.SocialPosts;
        }

        public async Task<string> GetLongLivedToken(string shortLivedAccessToken)
        {
            string appId = ConfigurationManager.AppSettings["FACEBOOK_APP_ID"];
            string appSecret = ConfigurationManager.AppSettings["FACEBOOK_APP_SECRET"];
       
          
            var requestUrl = string.Format(SocialApiUrl.API_LONG_LIVED_TOKEN, appId, appSecret, shortLivedAccessToken);

            HttpClient client = new HttpClient();

            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var token = data.Split('&')[0].Split('=')[1];

                return token;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public async Task<string> GetUserCodeFromLongLivedToken(string longLivedAccessToken)
        {
            string appId = ConfigurationManager.AppSettings["FACEBOOK_APP_ID"];
            string appSecret = ConfigurationManager.AppSettings["FACEBOOK_APP_SECRET"];
            string redirectURI = ConfigurationManager.AppSettings["FACEBOOK_REDIRECT_URI"];
          
            var requestUrl = string.Format(SocialApiUrl.API_USER_CODE_FROM_LONG_LIVED_TOKEN, appId, appSecret, longLivedAccessToken, redirectURI);


            HttpClient client = new HttpClient();

            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                string code = data.Split(':')[1].Split('\"')[1];

                return code;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new HttpException((int)response.StatusCode, content);
            }
        }

        public dynamic PullNewFeed(string accessToken, DateTime date)
        {
            string urlAPI = SocialApiUrl.API_USER_FEEDS_WITH_ALL_INFORMATION.Replace("{since}", date.ToString("MM/dd/yyyy"));
            var fbCLient = new FacebookClient(accessToken);
            dynamic results = fbCLient.Get(urlAPI);

            return results;
        }

        public dynamic LikesAPost(string accessToken, string postId)
        {
            try
            {
                string urlAPI = string.Format(SocialApiUrl.API_LIKES_OF_POST, postId);
                var fbClient = new FacebookClient(accessToken);
                dynamic results = fbClient.Post(urlAPI, new { });
                return results;
            }
            catch (Exception ex)
            {
                return new { success = false };
            }
        }

        public dynamic UnLikesAPost(string accessToken, string postId)
        {
            try
            {
                string urlAPI = string.Format(SocialApiUrl.API_LIKES_OF_POST, postId);
                var fbClient = new FacebookClient(accessToken);
                dynamic results = fbClient.Delete(urlAPI, new { });
                return results;
            }
            catch (FacebookOAuthException ex)
            {
                if (ex.ErrorCode == 100)
                {
                    return new { errorCode = 100, success = false, message = ex.Message, id = string.Empty };
                }

                throw new CustomException(ex.Message);
            }
            catch (FacebookApiException ex)
            {
                throw new CustomException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<dynamic> GetLikesOfPost(string accessToken, string postId)
        {
            string urlAPI = string.Format(SocialApiUrl.API_GET_LIKES_OF_POST, postId);
            var fbClient = new FacebookClient(accessToken);
            dynamic results = fbClient.Get(urlAPI);

            return results;
        }

        public async Task<dynamic> GetCommentOfPost(string accessToken, string postId)
        {
            string urlAPI = string.Format(SocialApiUrl.API_GET_COMMENTS_OF_POST, postId);
            var fbClient = new FacebookClient(accessToken);
            dynamic results = fbClient.Get(urlAPI);

            return results;
        }

        public async Task<dynamic> GetPictureOfPost(string accessToken, string postId)
        {
            string urlAPI = string.Format(SocialApiUrl.API_GET_PICTURES_OF_POST, postId);
            var fbClient = new FacebookClient(accessToken);
            dynamic results = fbClient.Get(urlAPI);

            return results;
        }

        public dynamic GetAllInformationOfPost(string accessToken, string postId)
        {
            string urlAPI = SocialApiUrl.API_GET_ALL_INFORMATION_OF_POST.Replace("{0}", postId);
            var fbClient = new FacebookClient(accessToken);
            dynamic results = fbClient.Get(urlAPI);

            return results;
        }

        public async Task<dynamic> GetShareOfPost(string accessToken, string postId)
        {
            string urlAPI = string.Format(SocialApiUrl.API_GET_SHARES_OF_POST, postId);
            var fbClient = new FacebookClient(accessToken);
            dynamic results = fbClient.Get(urlAPI);
            return results;
        }

        public dynamic PostFeed(string userSocialId, string accessToken, string content, string[] imagePath = null, PostPrivacyType privacy = PostPrivacyType.Public)
        {
            try
            {
                var privacyStr = "EVERYONE";
                if (privacy == PostPrivacyType.Friends)
                    privacyStr = "ALL_FRIENDS";
                else if (privacy == PostPrivacyType.Private)
                    privacyStr = "SELF";
                string urlAPI = SocialApiUrl.API_USER_FEEDS;
                var fbClient = new FacebookClient(accessToken);
                if (imagePath != null && imagePath.Length != 0)
                {
                    urlAPI = string.Format(SocialApiUrl.API_PUBLISH_PHOTO_ACTION, "me", ConfigurationManager.AppSettings["FACEBOOK_PUBLISH_REGIT_PHOTO_NAMESPACE"]);

                    var form = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("regit_photo", ConfigurationManager.AppSettings["RegitUrl"]),
                        new KeyValuePair<string, string>("fb:explicitly_shared","true"),
                        new KeyValuePair<string, string>("message",content),
                        new KeyValuePair<string, string>("privacy","{\"value\":\"" + privacyStr + "\"}")
                    };

                    for (int i = 0; i < imagePath.Length; i++)
                    {
                        var imgPath = imagePath[i].TrimStart('~', '/');
                        if (!imgPath.ToLower().StartsWith("http://") && !imgPath.ToLower().StartsWith("https://"))
                        {
                            imgPath = ConfigurationManager.AppSettings["RegitUrl"] + "/" + imgPath;
                        }
                        form.Add(new KeyValuePair<string, string>(string.Format("image[{0}][url]", i), imgPath));
                        form.Add(new KeyValuePair<string, string>(string.Format("image[{0}][user_generated]", i), "true"));
                    }

                    form.Add(new KeyValuePair<string, string>("access_token", accessToken));

                    var postContent = new FormUrlEncodedContent(form);

                    var response = new HttpClient().PostAsync(urlAPI, postContent).Result;

                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseObject = JObject.Parse(responseContent);
                        var result = responseObject.ToObject<dynamic>();
                        result.id = userSocialId + "_" + result.id.ToString();
                        return result;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var responseObject = JObject.Parse(responseContent);
                        throw new Exception(responseObject.ToObject<dynamic>().error.message.ToString());
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
                else
                {
                    var result = fbClient.Post(urlAPI, new { message = content, privacy = new { value = privacyStr } });
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public dynamic PostPageFeed(string pageId, string accessToken, string content, string[] imagePath = null)
        {
            try
            {
                string urlAPI = string.Format(SocialApiUrl.API_POST_NEW_PAGE_FEED, pageId);
                var fbClient = new FacebookClient(accessToken);
                dynamic result_image = null;
                if (imagePath != null && imagePath.Length != 0)
                {
                    if (imagePath.Length == 1)
                    {
                        result_image = PostImage(accessToken, imagePath[0], content);
                        
                        return new { id = result_image.post_id };
                    }
                    else
                    {
                        List<string> images = new List<string>();
                        for (int i = 0; i < imagePath.Length; i++)
                        {
                            images.Add(PostImage(accessToken, imagePath[i], content, false).id.ToString());
                        }

                        var post = new
                        {
                            message = content,
                            attached_media = new List<string>()
                        };

                        for (int i = 0; i < images.Count; i++)
                        {
                            post.attached_media.Add("{\"media_fbid\": \"" + images[i] + "\"}");
                        }

                        var result = fbClient.Post(urlAPI, post);
                        return result;
                    }
                }
                else
                {
                    var result = fbClient.Post(urlAPI, new { message = content });
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public dynamic PostImage(string accessToken, string imagePath, string message = "", bool published = true, bool isInternetPath = false, string privacy = null)
        {
            var fbClient = new FacebookClient(accessToken);

            dynamic payload = new ExpandoObject();
            payload.caption = message;
            payload.published = published;

            if (!string.IsNullOrEmpty(privacy))
            {
                payload.privacy = new { value = privacy };
            }

            if (!isInternetPath)
            {
                var absPath = CommonFunctions.MapPath("~" + imagePath.Trim('~'));
                FacebookMediaObject media = new FacebookMediaObject
                {
                    FileName = Path.GetFileName(absPath),
                    ContentType = "image/jpeg"
                };

                media.SetValue(File.ReadAllBytes(absPath));

                payload.source = media;
            }
            else
            {
                payload.url = imagePath;
            }


            var result = fbClient.Post(SocialApiUrl.API_POST_PHOTO, payload);
            return result;
        }

        public dynamic PostVideo(string accessToken, string videoUrl, string message = "")
        {
            var fbClient = new FacebookClient(accessToken);
            var result = fbClient.Post(SocialApiUrl.API_POST_VIDEO, new { title = message, file_url = videoUrl });
            return result;
        }

        public SocialPost SharePost(string accessToken, SocialPost post)
        {
            try
            {
                string urlAPI = SocialApiUrl.API_USER_FEEDS;
                var fbClient = new FacebookClient(accessToken);

                var message = post.Message;

                if (post.Photos != null && post.Photos.Count > 0)
                {
                    dynamic result = fbClient.Post(urlAPI, new
                    {
                        message = post.Message,
                        link = post.Photos[0].Url
                    });

                    if (result != null && result.id != null)
                        post.SocialId = result.id;

                    return post;
                }
                else
                {
                    dynamic resultWithoutImage = fbClient.Post(SocialApiUrl.API_USER_FEEDS, new
                    {
                        message = message
                    });

                    if (resultWithoutImage != null && resultWithoutImage.id != null)
                        post.SocialId = resultWithoutImage.id;
                    return post;
                }

            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public async Task<string> PostImageWithDataByteArray(string accessToken, byte[] data, string fileName)
        {
            try
            {
                var fbClient = new FacebookClient(accessToken);
                var stream = new MemoryStream(data);

                dynamic res = fbClient.Post(SocialApiUrl.API_USER_PHOTOS, new
                {
                    file = new FacebookMediaStream()
                    {
                        ContentType = "image/jpg",
                        FileName = fileName
                    }.SetValue(stream)
                });

                if (res != null)
                {
                    if (res.id != null)
                    {
                        return res.id;
                    }
                }

                return null;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public async Task<string> PostFeedWithImageId(string accessToken, string imageId, string message)
        {
            try
            {
                string urlAPI = SocialApiUrl.API_USER_FEEDS;
                var fbClient = new FacebookClient(accessToken);

                if (imageId != null)
                {
                    dynamic result = fbClient.Post(urlAPI, new
                    {
                        message = message,
                        object_attachment = imageId
                    });

                    if (result.id
                        != null)
                    {
                        return result.id;
                    }
                }
                else
                {
                    dynamic result = fbClient.Post(urlAPI, new
                    {
                        message = message
                    });

                    if (result.id
                        != null)
                    {
                        return result.id;
                    }
                }

                return "";
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public List<dynamic> GetListFriend(string accessToken, List<string> ids)
        {
            try
            {
                var result = new List<dynamic>();
                var fbClient = new FacebookClient(accessToken);
                var count = ids.Count;
                count = count <= 50 ? 50 : count;
                var skip = 0;
                for (int i = 50; i <= count; i += 50)
                {
                    var idTemps = ids.Skip(skip).Take(50);
                    var url = SocialApiUrl.API_GET_LIST_FRIEND.Replace("{0}", string.Join(",", idTemps));
                    var doc = fbClient.Get(url);
                    result.Add(doc);
                    skip = i;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public dynamic GetListPages(string accessToken)
        {
            try
            {
                var fbClient = new FacebookClient(accessToken);
                var url = SocialApiUrl.API_GET_LIST_PAGE;
                var result = fbClient.Get(url);
                return result;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public dynamic GetPagePosts(string accessToken, string pageId, DateTime since)
        {
            try
            {
                var fbClient = new FacebookClient(accessToken);
                var url = SocialApiUrl.API_PAGE_FEEDS_WITH_ALL_INFORMATION.Replace("{fbId}", pageId).Replace("{since}", since.ToString("MM/dd/yyyy"));
                var result = fbClient.Get(url);
                return result;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public bool DeletePost(string accessToken, string postId)
        {
            try
            {
                string urlAPI = string.Format(SocialApiUrl.API_POST_DELETE, postId);
                var fbClient = new FacebookClient(accessToken);
                dynamic results = fbClient.Delete(urlAPI);
                return results.success;
            }
            catch (FacebookApiException facebookApiException)
            {
                // Post was deleted on facebook.
                if (facebookApiException.ErrorCode == 100)
                {
                    return true;
                }
                throw new CustomException(facebookApiException.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public dynamic Comment(string accessToken, string postid, string comment)
        {
            try
            {
                string urlApi = string.Format(SocialApiUrl.API_COMMENTS_OF_POST, postid);
                var fbClient = new FacebookClient(accessToken);
                dynamic results = fbClient.Post(urlApi, new { message = comment });
                return results;
            }
            catch (FacebookOAuthException ex)
            {
                if (ex.ErrorCode == 100)
                {
                    return new { errorCode = 100, success = false, message = ex.Message, id = string.Empty };
                }

                throw new CustomException(ex.Message);
            }
            catch (FacebookApiException ex)
            {
                throw new CustomException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
    }
}