using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.Collectors;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Adapters;
using GH.Core.Exceptions;
using GH.Core.Helpers;
using Newtonsoft.Json.Linq;
using System.Configuration;
using GH.Core.ViewModels;

namespace GH.Core.Services
{
    public class SocialService : ISocialService
    {
        private IMongoCollection<SocialPost> _socialPostCollection;
        private IFacebookCollector _facebookCollector;
        private ITwitterCollector _twitterCollector;
        private IAccountService _accountService;
        private INetworkService _networkService;
        private ISocialPostService _socialPostService;
        public SocialService()
        {
            var mongoDb = MongoContext.Db;
            _socialPostCollection = mongoDb.SocialPosts;
            //_accountCollection = mongoDb.Accounts;

            _facebookCollector = new FacebookCollector();
            _twitterCollector = new TwitterCollector();
            _accountService = new AccountService();
            _networkService = new NetworkService();
            _socialPostService = new SocialPostService();
        }

        public SocialPost GetById(ObjectId id)
        {
            return _socialPostCollection.Find(t => t.Id == id).FirstOrDefault();
        }

        public SocialPost CreatePost(SocialPost post)
        {
            _socialPostCollection.InsertOne(post);
            return post;
        }

        /// <summary>
        /// Insert or update post. Only use for pull post from social network
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public SocialPost InsertOrUpdatePostPullFromSocialNetwork(SocialPost post)
        {
            var exsitingPost = _socialPostCollection.Find(t => t.SocialId == post.SocialId).FirstOrDefault();
            if (exsitingPost == null)
                _socialPostCollection.InsertOne(post);
            else
            {
                exsitingPost.Message = post.Message;
                exsitingPost.Photos = post.Photos;
                exsitingPost.VideoUrl = post.VideoUrl;
                if (post.Comments.Count > 0)
                {
                    if (exsitingPost.Comments.Count == 0)
                        exsitingPost.Comments = post.Comments;
                    else
                    {
                        foreach (var comment in post.Comments)
                        {
                            var oldComment = exsitingPost.Comments.FirstOrDefault(t => t.SocialId == comment.SocialId);
                            if (oldComment == null)
                                exsitingPost.Comments.Add(comment);
                            else
                                oldComment.Message = comment.Message;
                        }
                    }
                }
                if (post.Like.RegitLikes.Count > 0)
                {
                    var likeId = post.Like.RegitLikes[0];
                    var isLiked = exsitingPost.Like.RegitLikes.FirstOrDefault(x => x.Id == likeId.Id);
                    if (isLiked == null)
                    {
                        exsitingPost.Like.RegitLikes.Add(likeId);
                    }
                }
                else
                {
                    var isLiked = exsitingPost.Like.RegitLikes.FirstOrDefault(x => x.Id == post.AccountId);
                    if (isLiked != null)
                    {
                        exsitingPost.Like.RegitLikes.Remove(isLiked);
                    }
                }
                exsitingPost.Like.TwitterLikeCount = post.Like.TwitterLikeCount;
                exsitingPost.Like.FaceBookLikes = post.Like.FaceBookLikes;
                var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, exsitingPost.Id);
                var update = Builders<SocialPost>.Update.Set(t => t.Message, exsitingPost.Message)
                                                      .Set(t => t.Photos, exsitingPost.Photos)
                                                      .Set(t => t.VideoUrl, exsitingPost.VideoUrl)
                                                      .Set(t => t.Comments, exsitingPost.Comments)
                                                      .Set(t => t.Like, exsitingPost.Like);
                _socialPostCollection.UpdateOne(filter, update);
            }
            return post;
        }

        public SocialPost DeleteSocialPost(SocialPost socialPost)
        {
            var referencePost = GetById(socialPost.RefObjectId);
            if (referencePost != null)
            {
                referencePost.Shares--;
                _socialPostCollection.UpdateOne(Builders<SocialPost>.Filter.Eq(t => t.Id, referencePost.Id),
                    Builders<SocialPost>.Update.Set(x => x.Shares, referencePost.Shares));
            }
            _socialPostCollection.DeleteOne(Builders<SocialPost>.Filter.Eq(t => t.Id, socialPost.Id));

            return referencePost;
        }

        public async Task<dynamic> GetFeed(ObjectId account1, ObjectId account2, Permission permission)
        {
            if (permission.IsFriend)
            {
                if (permission.ListGroupBelong == null)
                {
                    var filterForPublicFeed = Builders<SocialPost>.Filter.And(
                        new FilterDefinition<SocialPost>[]
                        {
                            Builders<SocialPost>.Filter.Eq(f => f.AccountId, account2),
                            Builders<SocialPost>.Filter.Eq(f => f.Privacy.Type, PostPrivacyType.Public)
                        }
                        );

                    var publicFeeds = _socialPostCollection.Find(filterForPublicFeed);

                    return publicFeeds.ToList();
                }
                else
                {
                    var filterForGroup = Builders<SocialPost>.Filter.And(new FilterDefinition<SocialPost>[]
                    {
                        Builders<SocialPost>.Filter.Eq(f => f.AccountId, account2),
                        Builders<SocialPost>.Filter.Or(new FilterDefinition<SocialPost>[]
                        {
                            Builders<SocialPost>.Filter.Eq(f => f.Privacy.Type, PostPrivacyType.Public),
                            Builders<SocialPost>.Filter.In(f => f.Privacy.GroupId, permission.ListGroupBelong)
                        }
                        )
                    });

                    var publicFeeds = _socialPostCollection.Find(filterForGroup);

                    return publicFeeds.ToList();
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<SocialPost> FindPostBySocialId(string socialId)
        {
            try
            {
                return _socialPostCollection.Find(t => t.SocialId == socialId).FirstOrDefault();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<List<SocialPost>> GetPostByListId(List<ObjectId> ids)
        {
            return _socialPostCollection.Find(t => ids.Contains(t.Id)).ToList();
        }

        public async Task<List<SocialPost>> GetHomeFeed(ObjectId accountId, int start = 0, int take = 20)
        {
            try
            {
                //step 0: prepare data
                SocialPost lastestFeedOfTwitter = null;
                var newPullPosts = new List<SocialPost>();
                //get accout
                var rAccount = _accountService.GetById(accountId);

                //Step 1: get feed from mongo db
                var posts = GetFeedFromMongo(rAccount, start, take);
                return posts;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public IList<Comment> GetCommentBySocialPost(ObjectId socialPostId)
        {
            var socialPosts = _socialPostService.GetRelatedGroupPost(new List<string>() { socialPostId.ToString() }, socialPostId.ToString());
            var comments = new List<Comment>();
            foreach (var item in socialPosts)
            {
                if (item.Comments != null)
                {
                    comments.AddRange(item.Comments);
                }
            }
            return comments.OrderBy(s => s.CreatedOn).ToList();
        }

        public Comment AddNewComment(ObjectId accountId, ObjectId socialId, string message)
        {
            try
            {
                var comment = new Comment { Id = ObjectId.GenerateNewId(), Message = message, UserId = accountId, CreatedOn = DateTime.Now };
                var account = _accountService.GetById(accountId);
                var socialPost = GetById(socialId);
                if (socialPost == null)
                    throw new CustomException("Can not find the post.");
                var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, socialId);
                var relatedPost = _socialPostService.GetRelatedGroupPost(new List<string>() { socialPost.Id.ToString() });

                if (relatedPost.Count != 0)
                {
                    UpdateCommentOnRegit(socialPost, comment, filter);
                    return comment;
                }

                if (socialPost.Type == SocialType.Facebook)
                {
                    comment = CommentOnFacebook(message, account, socialPost, comment);
                }

                if (socialPost.Type == SocialType.Twitter)
                {
                    comment = CommentOnTwitter(message, account, socialPost, comment);
                }

                UpdateCommentOnRegit(socialPost, comment, filter);
                return comment;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public UpdateResult UpdateCommentOnRegit(SocialPost socialPost, Comment comment, FilterDefinition<SocialPost> filter)
        {
            if (socialPost.Comments != null)
            {
                var update = Builders<SocialPost>.Update.AddToSet(t => t.Comments, comment);
                return _socialPostCollection.UpdateOne(filter, update);
            }

            var u = Builders<SocialPost>.Update.Set(t => t.Comments, new List<Comment> { comment });
            return _socialPostCollection.UpdateOne(filter, u);
        }

        public Comment CommentOnTwitter(string message, Account account, SocialPost socialPost, Comment comment)
        {
            var twlink = account.AccountLinks.FirstOrDefault(s => s.Type == SocialType.Twitter);
            if (twlink != null)
            {
                var result = _twitterCollector.ReplyTweet(twlink.AccessToken, twlink.AccessTokenSecret,
                    new SocialPost
                    {
                        Type = SocialType.Twitter,
                        Message = message
                    }, socialPost.SocialId);
                comment.SocialId = result.SocialId;
            }

            return comment;
        }

        public Comment CommentOnFacebook(string message, Account account, SocialPost socialPost, Comment comment)
        {
            var accessToken = string.Empty;

            if (account.AccountType == AccountType.Business)
            {
                var accountPage = account.Pages.FirstOrDefault();
                if (accountPage != null)
                {
                    accessToken = accountPage.AccessToken;
                }
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                var fblink = account.AccountLinks.FirstOrDefault(s => s.Type == SocialType.Facebook);
                if (fblink == null)
                {
                    return comment;
                }

                accessToken = fblink.AccessToken;
            }

            var result = _facebookCollector.Comment(accessToken, socialPost.SocialId, message);
            if (result.id != null && !string.IsNullOrEmpty(result.id))
            {
                comment.SocialId = result.id;
            }

            return comment;
        }

        public ShareResult SharePost(ObjectId accountId, ObjectId postId, SocialType type, string message, string groupId = null)
        {
            var shareResult = new ShareResult();
            var account = _accountService.GetById(accountId);
            if (type != SocialType.GreenHouse && (account.AccountLinks == null || account.AccountLinks.Count() == 0))
                throw new CustomException("Your account didn't link with social network account.");
            var post = GetById(postId);

            if (post == null)
                throw new CustomException("Can not find this post.");
            if (type == SocialType.Facebook)
                shareResult = ShareFacebook(account, post, message);
            else if (type == SocialType.Twitter)
                shareResult = ShareTwitter(account, post, message);
            else //green house
                shareResult = ShareGreenHouse(accountId, post, message);

            //save share post into mongo
            if (shareResult.SharePost != null)
            {
                shareResult.SharePost.GroupId = groupId;
                CreatePost(shareResult.SharePost);
            }

            return new ShareResult { Errors = shareResult.Errors, SharePost = shareResult.SharePost, PostShared = shareResult.PostShared };
        }

        public SocialPost PostNewPost(PostSocialNetworkViewModel postViewModel, string accessToken, string secretToken, string userSocialId)
        {
            postViewModel.Post.Type = postViewModel.Type;
            if (postViewModel.Type == SocialType.Facebook)
            {
                string[] photoUrls = null;
                if (postViewModel.Post.Photos != null && postViewModel.Post.Photos.Count != 0)
                {
                    photoUrls = postViewModel.Post.Photos.Select(p => p.Url).ToArray();
                }
                if (!postViewModel.IsPostToFacebookPage)
                {
                    var fbPost = _facebookCollector.PostFeed(userSocialId, accessToken, postViewModel.Post.Message, photoUrls, postViewModel.Post.Privacy.Type);
                    postViewModel.Post.SocialId = fbPost.id;
                }
                else
                {
                    var fbPost = _facebookCollector.PostPageFeed(postViewModel.PageId, accessToken, postViewModel.Post.Message, photoUrls);
                    postViewModel.Post.SocialId = fbPost.id;
                }
            }
            if (postViewModel.Type == SocialType.Twitter)
            {
                var twPost = _twitterCollector.PostTweet(accessToken, secretToken, postViewModel.Post);
                postViewModel.Post.SocialId = twPost.SocialId;
            }
            return CreatePost(postViewModel.Post);
        }

        public List<SocialPost> GetFeedFromFacebook(Account rAccount, SocialPost lastestFeedOfFacebook, string fb_access_token)
        {
            var newPullPosts = new List<SocialPost>();
            if (string.IsNullOrEmpty(fb_access_token))
                return newPullPosts;
            var maxMinuteToUpdate = ConfigurationManager.AppSettings["MINUTE_TO_REFRESH"].ToString().ParseInt();
            var fromDate = DateTime.Now.AddMonths(-1).Date;
            var hasGetFacebook = false;
            //check the last feed is greater than maxMinuteToUpdate
            if (lastestFeedOfFacebook != null && (DateTime.Now - lastestFeedOfFacebook.CreatedOn).TotalMinutes > maxMinuteToUpdate)
            {
                hasGetFacebook = true;
                fromDate = lastestFeedOfFacebook.CreatedOn;
            }
            else//the first time of this account
                hasGetFacebook = true;
            if (!hasGetFacebook)
                return newPullPosts;
            //if type of user is personal
            if (rAccount.AccountType == AccountType.Personal)
            {
                var result = _facebookCollector.PullNewFeed(fb_access_token, fromDate);
                foreach (var item in result.data)
                {
                    SocialPost post = SocialAdapter.FacebookConvertToModel(rAccount, item);
                    post.PullAt = DateTime.Now;
                    newPullPosts.Add(post);
                }
            }
            else//if user's type is business
            {
                if (rAccount.Pages != null || rAccount.Pages.Count >= 1)
                {
                    var result = _facebookCollector.GetPagePosts(fb_access_token, rAccount.Pages[0].Id, fromDate);
                    foreach (var item in result.data)
                    {
                        SocialPost post = SocialAdapter.FacebookConvertToModel(rAccount, item);
                        post.PullAt = DateTime.Now;
                        post.SocialPageId = rAccount.Pages[0].Id;
                        newPullPosts.Add(post);
                    }
                }
            }

            if (newPullPosts.Count == 0)
                return newPullPosts;

            //update user of comment if he/she is aready in mongo
            var socialAccountIds = newPullPosts.SelectMany(t => t.Comments.Select(s => s.From.Id)).Distinct().ToList();
            if (socialAccountIds.Count > 0)
            {
                var accounts = _accountService.GetByListSocialId(socialAccountIds);
                //remove account that have in mongodb
                accounts.ForEach(t => socialAccountIds.Remove(t.SocialAccountId));
                //get account info from fb
                dynamic socialAccounts = null;
                if (socialAccountIds.Count > 0)
                    socialAccounts = _facebookCollector.GetListFriend(fb_access_token, socialAccountIds);
                newPullPosts = UpdateUserForComment(newPullPosts, accounts, socialAccounts);
            }
            return newPullPosts;
        }

        public List<SocialPost> UpdateCommentForUnknowFacebookUser(string fbAccessToken, List<string> socialAccountIds, List<SocialPost> newPullPosts)
        {
            var accounts = _accountService.GetByListSocialId(socialAccountIds);
            //remove account that have in mongodb
            accounts.ForEach(t => socialAccountIds.Remove(t.SocialAccountId));
            //get account info from fb
            dynamic socialAccounts = null;
            if (socialAccountIds.Count > 0)
                socialAccounts = _facebookCollector.GetListFriend(fbAccessToken, socialAccountIds);
            newPullPosts = UpdateUserForComment(newPullPosts, accounts, socialAccounts);
            return newPullPosts;
        }

        public List<SocialPost> GetFacebookFeedByBusinessAccount(Account rAccount, string fbAccessToken, DateTime fromDate)
        {
            List<SocialPost> newPullPosts = new List<SocialPost>();
            var result = _facebookCollector.GetPagePosts(fbAccessToken, rAccount.Pages[0].Id, fromDate);
            foreach (var item in result.data)
            {
                SocialPost post = SocialAdapter.FacebookConvertToModel(rAccount, item);
                post.PullAt = DateTime.Now;
                post.SocialPageId = rAccount.Pages[0].Id;
                newPullPosts.Add(post);
            }
            return newPullPosts;
        }

        public List<SocialPost> GetFacebookFeedByPersonalAccount(Account rAccount, string fbAccessToken, DateTime fromDate)
        {
            List<SocialPost> newPullPosts = new List<SocialPost>();
            var result = _facebookCollector.PullNewFeed(fbAccessToken, fromDate);
            foreach (var item in result.data)
            {
                SocialPost post = SocialAdapter.FacebookConvertToModel(rAccount, item);
                post.PullAt = DateTime.Now;
                newPullPosts.Add(post);
            }
            return newPullPosts;
        }

        public List<SocialPost> GetFeedFromTwitter(Account rAccount, SocialPost lastestFeedOfTwitter, string tw_access_token, string tw_secret_token, string socialAccountId)
        {
            var newPullPosts = new List<SocialPost>();
            if (string.IsNullOrEmpty(tw_access_token) || string.IsNullOrEmpty(tw_secret_token))
                return newPullPosts;
            var maxMinuteToUpdate = ConfigurationManager.AppSettings["MINUTE_TO_REFRESH"].ToString().ParseInt();
            var twLimit = 25;
            var hasGetTwitter = false;
            if (lastestFeedOfTwitter != null)
            {
                //kiểm tra feed cuối cùng có quá maxMinuteToUpdate hay không
                if ((DateTime.Now - lastestFeedOfTwitter.CreatedOn).TotalMinutes > maxMinuteToUpdate)
                    hasGetTwitter = true;
            }
            else
            {
                hasGetTwitter = true;
                twLimit = 100;
            }
            if (hasGetTwitter)
            {
                var twFeed = _twitterCollector.GetHomeTweet(socialAccountId, tw_access_token, tw_secret_token, twLimit);
                var parentTweets = twFeed.Where(t => t.InReplyToStatusId == null).ToList();
                var twComments = twFeed.Where(t => t.InReplyToStatusId != null).ToList();
                foreach (var tweet in parentTweets)
                {
                    var comments = twComments.Where(t => t.InReplyToStatusId == tweet.Id).ToList();
                    var twPost = SocialAdapter.TwitterConvertToModel(rAccount, tweet, comments);
                    twPost.PullAt = DateTime.Now;
                    newPullPosts.Add(twPost);
                }
            }
            return newPullPosts;
        }

        public UpdateResult UpdatePostShareCount(ObjectId socialPostId, int shares)
        {
            var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, socialPostId);
            var update = Builders<SocialPost>.Update.Set(t => t.Shares, shares);
            return _socialPostCollection.UpdateOne(filter, update);
        }

        public List<SocialPost> GetPageFeed(string pageid)
        {
            return null;
        }

        public IEnumerable<SocialPost> GetAllRelatedPostByPostId(ObjectId postId)
        {
            var builders = Builders<SocialPost>.Filter;
            var filter = builders.Eq("_id", postId) | builders.Eq("GroupId", postId.ToString());
            var result = _socialPostCollection.Find(filter);
            return result.ToList();
        }

        public IEnumerable<SocialPost> DeletePost(Account account, ObjectId postId)
        {
            var listReferencePost = new List<SocialPost>();
            var relatedPosts = GetAllRelatedPostByPostId(postId);
            foreach (var relatedPost in relatedPosts)
            {
                if (relatedPost.Type == SocialType.Facebook)
                {
                    if (string.IsNullOrEmpty(relatedPost.SocialPageId))
                    {
                        var fbLink = account.AccountLinks.Find(s => s.Type == SocialType.Facebook);
                        if (fbLink != null)
                            _facebookCollector.DeletePost(fbLink.AccessToken, relatedPost.SocialId);
                    }
                    else
                    {
                        var fbLink = account.Pages.Find(s => s.SocialType == SocialType.Facebook);
                        if (fbLink != null)
                            _facebookCollector.DeletePost(fbLink.AccessToken, relatedPost.SocialId);
                    }
                }

                if (relatedPost.Type == SocialType.Twitter)
                {
                    var twitterLink = account.AccountLinks.Find(s => s.Type == SocialType.Twitter);
                    if (twitterLink != null)
                        _twitterCollector.DestroyTweet(twitterLink.AccessToken, twitterLink.AccessTokenSecret, relatedPost.SocialId);
                }

                var post = DeleteSocialPost(relatedPost);
                if (post != null)
                {
                    listReferencePost.Add(post);
                }
            }

            return listReferencePost;
        }

        #region Helps
        private List<SocialPost> GetFeedFromMongo(Account rAccount, int start = 0, int take = 20)
        {
            var networksUserBelong = _networkService.GetNetworksUserBelongTo(rAccount.Id);
            var listOfFriends = networksUserBelong.Select(n => n.NetworkOwner).ToList();
            var networkIds = networksUserBelong.Select(n => n.Id).ToList();
            //var followees = rAccount.Followees.Select(s => s.AccountId).ToList();
            //if (rAccount.AccountType == AccountType.Personal)
            //{
            //    listOfFriends.AddRange(rAccount.BusinessAccountRoles.Select(s => s.AccountId));
            //    listOfFriends.AddRange(followees);
            //}


            //build query to get lastest social post of account and its friends
            var filterQuery = Builders<SocialPost>.Filter.And(new FilterDefinition<SocialPost>[]
            {
                Builders<SocialPost>.Filter.Eq(f=>f.GroupId,null),
                Builders<SocialPost>.Filter.Or(new FilterDefinition<SocialPost>[]
                {
                    Builders<SocialPost>.Filter.Eq(f => f.AccountId, rAccount.Id),

                    Builders<SocialPost>.Filter.And(new FilterDefinition<SocialPost>[]
                    {
                        Builders<SocialPost>.Filter.In(f => f.AccountId, listOfFriends),
                        Builders<SocialPost>.Filter.Or(new FilterDefinition<SocialPost>[] {
                            Builders<SocialPost>.Filter.Eq(f => f.Privacy.Type, PostPrivacyType.Public),
                            Builders<SocialPost>.Filter.Eq(f => f.Privacy.Type, PostPrivacyType.Friends)
                        })
                    })
                })
            });
            return _socialPostCollection.Find(filterQuery).Sort(Builders<SocialPost>.Sort.Descending(s => s.ModifiedOn)).Skip(start).Limit(take).ToList();
        }

        public SocialPost GetLastPost(ObjectId rAccountId, SocialType type)
        {
            return _socialPostCollection.Find(t => t.AccountId == rAccountId && t.Type == type).SortByDescending(s => s.ModifiedOn).FirstOrDefault();
        }

        private List<SocialPost> UpdateUserForComment(List<SocialPost> posts, List<Account> accounts, List<dynamic> socialAccounts)
        {
            List<JObject> jObjects = new List<JObject>();
            if (socialAccounts != null && socialAccounts.Count > 0)
                socialAccounts.ForEach(t => jObjects.Add(JObject.FromObject(t)));
            foreach (var post in posts)
            {
                foreach (var comment in post.Comments)
                {
                    var account = accounts.FirstOrDefault(t => t.SocialAccountId == comment.From.Id);
                    if (account != null)//if have account in mongodb, update
                        comment.UserId = account.Id;
                    else if (jObjects.Count > 0)//else get information from social network
                    {
                        var jObject = jObjects.FirstOrDefault(t => t.Property(comment.From.Id) != null);
                        if (jObject != null)
                        {
                            dynamic socialAccount = jObject.GetValue(comment.From.Id);
                            comment.From.DisplayName = socialAccount.name;
                            comment.From.Link = socialAccount.link;
                            if (socialAccount.picture != null)
                                comment.From.PhotoUrl = socialAccount.picture.data.url;
                        }
                    }
                }
            }
            return posts;
        }

        /// <summary>
        /// Kiểm tra user đó đã có trong Regit hay chưa
        /// </summary>
        /// <param name="socialIds"></param>
        private List<SocialPost> CheckAccountInRegit(List<SocialPost> newPullPosts)
        {
            var socialIds = newPullPosts.SelectMany(t => t.Like.FaceBookLikes.Select(s => s.Id)).ToList();
            var count = socialIds.Count;
            var rAccounts = new List<Account>();
            int skip = 0, limit = 2000;
            while (count > 0)
            {
                var ids = socialIds.Skip(skip).Take(limit).ToList();
                rAccounts.AddRange(_accountService.GetByListSocialId(ids).ToList());
                skip += limit;
                count -= limit;
            }
            if (rAccounts.Count == 0)
                return newPullPosts;

            return newPullPosts;
        }

        private ShareResult ShareFacebook(Account account, SocialPost post, string messageShare)
        {
            var shareResult = new ShareResult();
            try
            {
                var fbLink = account.AccountLinks.FirstOrDefault(t => t.Type == SocialType.Facebook);
                var fbAccessToken = fbLink != null ? fbLink.AccessToken : "";
                if (string.IsNullOrEmpty(fbAccessToken))
                {
                    shareResult.Errors.Add("Your account didn't link with Facebook account.");
                }
                else
                {
                    string[] photoUrl = null;
                    if (post.Photos != null && post.Photos.Count > 0)
                        photoUrl = post.Photos.Select(p => p.Url).ToArray();
                    if (!string.IsNullOrEmpty(messageShare))
                        post.Message = messageShare.Trim() + Environment.NewLine + post.Message;
                    var result = _facebookCollector.PostFeed(fbLink.SocialAccountId, fbAccessToken, post.Message, photoUrl, post.Privacy.Type);

                    //save into mongo db after post fb
                    shareResult.SharePost = new SocialPost
                    {
                        AccountId = account.Id,
                        CreatedBy = account.Id,
                        CreatedOn = DateTime.Now,
                        Message = messageShare,
                        ModifiedBy = account.Id,
                        ModifiedOn = DateTime.Now,
                        Privacy = post.Privacy,
                        SocialId = result.id,
                        RefObjectId = post.Id,
                        Photos = post.Photos,
                        Type = SocialType.Facebook,
                        VideoUrl = post.VideoUrl,
                        Like = new Like()
                    };
                }
            }
            catch (CustomException ex)
            {
                shareResult.Errors.AddRange(ex.Errors.Select(t => t.Message));
            }
            return shareResult;
        }

        private ShareResult ShareTwitter(Account account, SocialPost post, string messageShare)
        {
            var shareResult = new ShareResult();
            try
            {
                var twValidate = true;
                if (string.IsNullOrEmpty(post.Message) && (post.Photos == null || post.Photos.Count() == 0))
                {
                    shareResult.Errors.Add("Your content is not empty");
                    twValidate = false;
                }
                else if (!string.IsNullOrEmpty(post.Message) && post.Message.Length > 140)
                {
                    shareResult.Errors.Add("Your content is greater than 140 letters.");
                    twValidate = false;
                }
                if (twValidate)
                {
                    var twLink = account.AccountLinks.FirstOrDefault(t => t.Type == SocialType.Twitter);
                    var twAccessToken = twLink != null ? twLink.AccessToken : "";
                    var twSecrect = twLink.AccessTokenSecret;
                    if (string.IsNullOrEmpty(twAccessToken) || string.IsNullOrEmpty(twSecrect))
                    {
                        shareResult.Errors.Add("Your account didn't link with Twitter account.");
                        twValidate = false;
                    }
                    if (twValidate)
                    {
                        if (!string.IsNullOrEmpty(messageShare))
                            post.Message = messageShare.Trim() + Environment.NewLine + post.Message;
                        var result = _twitterCollector.PostTweet(twAccessToken, twSecrect, post);

                        if (result != null)
                            shareResult.SharePost = new SocialPost
                            {
                                Id = ObjectId.Empty,
                                AccountId = account.Id,
                                Privacy = new PostPrivacy { Type = PostPrivacyType.Public },
                                Type = SocialType.Twitter,
                                CreatedBy = account.Id,
                                CreatedOn = DateTime.Now,
                                ModifiedBy = account.Id,
                                ModifiedOn = DateTime.Now,
                                RefObjectId = result.Id,
                                Photos = result.Photos,
                                Message = result.Message,
                                SocialId = result.SocialId,
                                VideoUrl = result.VideoUrl,
                                Like = new Like()
                            };
                    }
                }
            }
            catch (CustomException ex)
            {
                shareResult.Errors.AddRange(ex.Errors.Select(t => t.Message));
            }
            return shareResult;
        }

        private ShareResult ShareGreenHouse(ObjectId accountId, SocialPost post, string messageShare)
        {
            var shareResult = new ShareResult();
            shareResult.PostShared = post;
            shareResult.SharePost = new SocialPost
            {
                AccountId = accountId,
                CreatedBy = accountId,
                CreatedOn = DateTime.Now,
                Message = messageShare,
                ModifiedBy = accountId,
                ModifiedOn = DateTime.Now,
                Privacy = post.Privacy,
                RefObjectId = post.Id,
                Photos = new List<Photo>(),
                Type = SocialType.GreenHouse,
                VideoUrl = post.VideoUrl,
                Like = new Like()
            };
            return shareResult;
        }
        #endregion
    }
}