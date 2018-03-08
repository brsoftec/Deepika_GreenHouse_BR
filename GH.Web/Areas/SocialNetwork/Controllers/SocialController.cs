using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GH.Core.Extensions;
using GH.Core.Models;
using GH.Core.Services;
using GH.Core.Adapters;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.ViewModels;
using GH.Web.Areas.SocialNetwork.ViewModels;
using GH.Core.Exceptions;
using GH.Core.Helpers;
using GH.Core.Collectors;

using NLog;
using GH.Web.Areas.SocialNetwork.Adapters;
using GH.Core.BlueCode.BusinessLogic;

namespace GH.Web.Areas.SocialNetwork.Controllers
{
    /// <summary>
    /// API for Social Network
    /// </summary>
    [Authorize]
    [RoutePrefix("API/Social")]
    public class SocialController : ApiController
    {
        private ISocialService _socialService;
        private ISocialPostService _socialPostService;
        private IAccountService _accountService;
        private ISocialPageService _socialPageService;
        private IRoleService _roleService;
        private INotificationService _notifyService;
        private IProcessPostService _processPostService;

        public SocialController()
        {
            _socialService = new SocialService();
            _accountService = new AccountService();
            _socialPostService = new SocialPostService();
            _socialPageService = new SocialPageService();
            _roleService = new RoleService();
            _notifyService = new NotificationService();
            _processPostService = new ProcessPostService();
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet, Route("GetFacebookUserCodeFromServerLongLivedToken")]
        public async Task<string> GetFacebookUserCodeFromServerLongLivedToken()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string externalAccessToken = identity.FindFirstValue("ExternalAccessToken");

            IFacebookCollector _facebookService = new FacebookCollector();
            string longLivedToken = await _facebookService.GetLongLivedToken(externalAccessToken);
            string userCode = await _facebookService.GetUserCodeFromLongLivedToken(longLivedToken);

            return userCode;
        }

        [HttpGet, Route("PullMyFeed")]
        public async Task<List<SocialPostViewModel>> PullFeedToMyDashboard(int start = 0, int take = 20)
        {
            try
            {
                var accountId = ValidateCurrentUser();
                var rAccount = _accountService.GetByAccountId(accountId);

                var socialPosts = await _socialService.GetHomeFeed(rAccount.Id, start, take);

                //get all post was shared in regit
                var sharePosts = socialPosts.Where(t => t.Type == SocialType.GreenHouse && t.RefObjectId != ObjectId.Empty).ToList();
                var postSharedIds = sharePosts.Select(t => t.RefObjectId).ToList();
                var postShareds = await _socialService.GetPostByListId(postSharedIds);

                var accountIds = socialPosts.Select(t => t.AccountId).ToList();
                accountIds.AddRange(postShareds.Select(t => t.AccountId).ToList());
                accountIds = accountIds.Distinct().ToList();
                var accounts = _accountService.GetByListId(accountIds);
                var socialPostViewModels = new List<SocialPostViewModel>();
                var relatedPost = _socialPostService.GetRelatedGroupPost(socialPosts.Select(s => s.Id.ToString()).ToList());
                foreach (var model in socialPosts)
                {
                    var account = accounts.FirstOrDefault(t => t.Id == model.AccountId);
                    var postShared = postShareds.FirstOrDefault(t => t.Id == model.RefObjectId);
                    var accountShared = postShared != null ? accounts.FirstOrDefault(t => t.Id == postShared.AccountId) : null;

                    var viewModel = SocialAdapter.SocialConvertToViewModel(model, account, rAccount, relatedPost.Where(s => s.GroupId == model.Id.ToString()).ToList(), postShared, accountShared);
                    socialPostViewModels.Add(viewModel);
                }
                return socialPostViewModels;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpGet, Route("socialpost/comments")]
        public List<CommentViewModel> GetPostBySocialPost(string socialPostId)
        {
            var commentViewModels = new List<CommentViewModel>();
            var comments = _socialService.GetCommentBySocialPost(socialPostId.ParseToObjectId());
            if (comments == null)
                return commentViewModels;
            var accountIds = comments.Select(t => t.UserId).Distinct().ToList();
            var accounts = _accountService.GetByListId(accountIds);
            foreach (var model in comments)
            {
                var account = accounts.FirstOrDefault(t => t.Id == model.UserId);
                var viewModel = SocialAdapter.CommentConvertToViewModel(model, account);
                commentViewModels.Add(viewModel);
            }
       
            return commentViewModels;
        }

        [HttpPost, Route("LikePost")]
        public IHttpActionResult LikePost(string id)
        {
            var accountId = ValidateCurrentUser();
            var socialPost = _socialService.GetById(id.ParseToObjectId());
            if (socialPost == null)
                throw new CustomException("Can not find this post.");
            var account = _accountService.GetByAccountId(accountId);
            var relatedPost = _socialPostService.GetRelatedGroupPost(new List<string>() { socialPost.Id.ToString() });
            var isLiked = SocialAdapter.CheckIsLiked(socialPost, account);
            if (relatedPost.Count == 0)
            {
                if (isLiked)
                {
                    _processPostService.UnLikeOnSocial(socialPost, account);
                }
                else
                {
                    _processPostService.LikeOnSocial(socialPost, account);
                }
            }
            else
            {
                _processPostService.UpdateRegitLike(account.Id, socialPost);
            }

            // Write activity log
            var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (waccount.AccountActivityLogSettings.RecordSocialActivity)
            {
                string title = "You like post in Regit.";
                string type = "Keep a record of your social activities";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
            }

            return Json(new { IsLiked = !isLiked });
        }

        [HttpPost, Route("AddComment")]
        public async Task<CommentViewModel> AddComment(CommentViewModel commentViewModel)
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(accountId))
                throw new CustomException("Can not find current user.");
            var rAccount = _accountService.GetByAccountId(accountId);
            if (rAccount == null)
                throw new CustomException("Can not find current user.");
            var comment = _socialService.AddNewComment(rAccount.Id, commentViewModel.Id.ParseToObjectId(), commentViewModel.Message);
            var viewModel = SocialAdapter.CommentConvertToViewModel(comment, rAccount);
           
            // Write activity log
            var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (waccount.AccountActivityLogSettings.RecordSocialActivity)
            {
                string title = "You create a comment in Regit.";
                string type = "Keep a record of your social activities";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
            }
            return viewModel;
        }

        [HttpPost, Route("SharePost")]
        public SharePostResultModel SharePost(SharePostViewModel model)
        {
            var accountId = ValidateCurrentUser();
            var rAccount = _accountService.GetByAccountId(accountId);
            var post = _socialService.GetById(model.SocialPostId.ParseToObjectId());
            if (rAccount == null)
                throw new CustomException("Can not find current user.");

            int postCount = 0;
            if (model.IsShareFacebook)
                postCount++;
            if (model.IsShareGreenHouse)
                postCount++;
            if (model.IsShareTwitter)
                postCount++;
            if (postCount > 1)
                model.IsShareGreenHouse = true;

            var types = new List<SocialType>();
            SocialPost result = null;
            SocialPost postShared = null;
            Account accountShared = null;
            List<object> postStatus = new List<object>();
            string groupId = null;
            if (model.IsShareGreenHouse)
            {
                try
                {
                    var shareResult = _socialService.SharePost(rAccount.Id, model.SocialPostId.ParseToObjectId(), SocialType.GreenHouse, model.Message);
                    if (shareResult.Errors.Count > 0)
                    {
                        postStatus.Add(new { type = "GreenHouse", sucess = false, message = "Regit: " + string.Join(". ", shareResult.Errors) });
                    }
                    else
                    {
                        postShared = shareResult.PostShared;
                        accountShared = _accountService.GetById(postShared.AccountId);
                        postStatus.Add(new { type = "GreenHouse", sucess = true, message = "Regit: posted successfully" });
                        types.Add(SocialType.GreenHouse);
                        result = shareResult.SharePost;
                        groupId = result.Id.ToString();
                        post.Shares++;
                        if (string.IsNullOrEmpty(groupId))
                        {
                            result = shareResult.SharePost;
                            groupId = result.Id.ToString();
                        }
                    }
                }
                catch (CustomException ex)
                {
                    postStatus.Add(new { type = "GreenHouse", sucess = false, message = "Regit: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                }
                catch (Exception ex)
                {
                    //Logger logger = LogManager.GetCurrentClassLogger();
                    //logger.Error(ex, "Unhandling exception occur when share post to Regit");
                    postStatus.Add(new { type = "GreenHouse", sucess = false, message = "Regit: Cannot share to Regit now" });
                }

            }

            if (model.IsShareFacebook)
            {
                if (rAccount.AccountLinks.Any(a => a.Type == SocialType.Facebook))
                {
                    try
                    {
                        var shareResult = _socialService.SharePost(rAccount.Id, model.SocialPostId.ParseToObjectId(), SocialType.Facebook, model.Message, groupId);
                        if (shareResult.Errors.Count > 0)
                        {
                            postStatus.Add(new { type = "Facebook", sucess = false, message = "Facebook: " + string.Join(". ", shareResult.Errors) });
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(groupId))
                            {
                                result = shareResult.SharePost;
                                groupId = result.Id.ToString();
                            }
                            postStatus.Add(new { type = "Facebook", sucess = true, message = "Facebook: posted successfully" });
                            types.Add(SocialType.Facebook);
                            post.Shares++;
                        }
                    }
                    catch (CustomException ex)
                    {
                        postStatus.Add(new { type = "Facebook", sucess = false, message = "Facebook: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                    }
                    catch (Exception ex)
                    {
                        //Logger logger = LogManager.GetCurrentClassLogger();
                        //logger.Error(ex, "Unhandling exception occur when share post to Facebook");
                        postStatus.Add(new { type = "Facebook", sucess = false, message = "Facebook: Cannot share to Facebook now" });
                    }
                }
                else
                {
                    postStatus.Add(new { type = "Facebook", sucess = false, message = "Facebook: Your account does not link with any Facebook account" });
                }
            }
            if (model.IsShareTwitter)
            {
                if (rAccount.AccountLinks.Any(a => a.Type == SocialType.Twitter))
                {
                    try
                    {
                        var shareResult = _socialService.SharePost(rAccount.Id, model.SocialPostId.ParseToObjectId(), SocialType.Twitter, model.Message, groupId);
                        if (shareResult.Errors.Count > 0)
                        {
                            postStatus.Add(new { type = "Twitter", sucess = false, message = "Twitter: " + string.Join(". ", shareResult.Errors) });
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(groupId))
                            {
                                result = shareResult.SharePost;
                                groupId = result.Id.ToString();
                            }
                            postStatus.Add(new { type = "Twitter", sucess = true, message = "Twitter: posted successfully" });
                            types.Add(SocialType.Twitter);
                            post.Shares++;
                        }
                    }
                    catch (CustomException ex)
                    {
                        postStatus.Add(new { type = "Twitter", sucess = false, message = "Twitter: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                    }
                    catch (Exception ex)
                    {
                        //Logger logger = LogManager.GetCurrentClassLogger();
                        //logger.Error(ex, "Unhandling exception occur when share post to Twitter");
                        postStatus.Add(new { type = "Twitter", sucess = false, message = "Twitter: Cannot share to Facebook now" });
                    }
                }
                else
                {
                    postStatus.Add(new { type = "Twitter", sucess = false, message = "Twitter: Your account does not link with any Twitter account" });
                }
            }

            _socialService.UpdatePostShareCount(model.SocialPostId.ParseToObjectId(), post.Shares);

            var responseData = SocialAdapter.SocialConvertToViewModel(result, rAccount, rAccount, null, postShared, accountShared);
            responseData.Types = types;
            
            // Write activity log
            var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (waccount.AccountActivityLogSettings.RecordSocialActivity)
            {
                string title = "You shared post: " + post.Shares;
                string type = "Keep a record of your social activities";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
            }

            return new SharePostResultModel()
            {
                postStatus = postStatus,
                SharePost = responseData,
                TotalShares = post.Shares
            };

        }

        [HttpPost, Route("PostANewFeed")]
        public async Task<dynamic> PostNewFeed()
        {
            //write log
            string type = "Keep a record of your social activities";
            //
            try
            {
                var accountId = ValidateCurrentUser();
                var rAccount = _accountService.GetByAccountId(accountId);
                var posts = new List<SocialPost>();
                List<object> postStatus = new List<object>();

                // Check if the request contains multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string root = HttpContext.Current.Server.MapPath("~/App_Data");
                var provider = new MultipartFormDataStreamProvider(root);

                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                var types = new List<SocialType>();
                var message = provider.FormData["Message"];
                var isPostGreenHouse = provider.FormData["IsPostGreenHouse"].ParseBool();
                var isPostFacebook = provider.FormData["IsPostFacebook"].ParseBool();
                var isPostTwitter = provider.FormData["IsPostTwitter"].ParseBool();
                var isFriends = provider.FormData["IsFriends"].ParseBool();
                var isPrivate = provider.FormData["IsPrivate"].ParseBool();
                var privacy = PostPrivacyType.Public;
                if (isFriends)
                    privacy = PostPrivacyType.Friends;
                else if (isPrivate)
                    privacy = PostPrivacyType.Private;

                if (!isPostFacebook && !isPostGreenHouse && !isPostTwitter)
                    throw new CustomException("You must choose at least one social network before posting.");
                if (string.IsNullOrEmpty(message))
                    throw new CustomException("The message is not empty.");
                if (isPostTwitter && message.Length > 140)
                    throw new CustomException("Twitter message lenght is not greater than 140 letters.");

                string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/SocialPostImages/");
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }
                System.Web.HttpFileCollection hfc = HttpContext.Current.Request.Files;
                string baseUrl = "/Content/SocialPostImages/";
                List<Photo> postPitures = new List<Photo>();
                for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
                {
                    System.Web.HttpPostedFile hpf = hfc[iCnt];

                    if (hpf.ContentLength > 0)
                    {

                        string filename = Guid.NewGuid() + Path.GetFileName(hpf.FileName);
                        // CHECK IF THE SELECTED FILE(S) ALREADY EXISTS IN FOLDER. (AVOID DUPLICATE)
                        if (!File.Exists(sPath + Path.GetFileName(hpf.FileName)))
                        {
                            // SAVE THE FILES IN THE FOLDER.
                            hpf.SaveAs(sPath + filename);
                            postPitures.Add(new Photo() { Url = baseUrl + filename });
                        }
                    }
                }

                if (postPitures.Count > 1)
                {
                    throw new CustomException("Cannot upload more than 1 photo");
                }

                int numberOfPost = 0;
                if (isPostFacebook)
                {
                    numberOfPost++;
                }
                if (isPostGreenHouse)
                {
                    numberOfPost++;
                }
                if (isPostTwitter)
                {
                    numberOfPost++;
                }

                if (numberOfPost > 1)
                {
                    isPostGreenHouse = true;
                }
                SocialPost result = null;
                string groupId = null;
                if (isPostGreenHouse)
                {
                    var pResult = new { type = "Facebook", sucess = true, message = "" };
                    try
                    {
                        var newPost = new SocialPost(message, rAccount.Id, new PostPrivacy { Type = privacy }, SocialType.GreenHouse, DateTime.Now, rAccount.Id, DateTime.Now, rAccount.Id);
                        newPost.Photos = postPitures;
                        var savedPost = _socialService.PostNewPost(new PostSocialNetworkViewModel { Post = newPost, Type = SocialType.GreenHouse }, null, null, null);
                        groupId = savedPost.Id.ToString();
                        result = savedPost;
                        postStatus.Add(new { type = "GreenHouse", sucess = true, message = GH.Lang.Regit.Regit_posted_successfully });
                        types.Add(SocialType.GreenHouse);
                    }
                    catch (CustomException ex)
                    {
                        postStatus.Add(new { type = "GreenHouse", sucess = false, message = "Regit: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                    }
                    catch (Exception ex)
                    {
                        postStatus.Add(new { type = "GreenHouse", sucess = false, message = GH.Lang.Regit.Regit_Cannot_post_on_Regit_now });
                    }

                    // Write activity log
                    if (rAccount.AccountActivityLogSettings.RecordSocialActivity)
                    {
                        string title = "You posted a new feed in Regit.";
                      
                        var act = new ActivityLogBusinessLogic();
                        act.WriteActivityLogFromAcc(accountId, title, type);

                    }

                }
                if (isPostFacebook)
                {
                    try
                    {
                        var newPost = new SocialPost(message, rAccount.Id, new PostPrivacy { Type = privacy }, SocialType.GreenHouse, DateTime.Now, rAccount.Id, DateTime.Now, rAccount.Id, "", "", "", groupId);
                        newPost.Photos = postPitures;
                        var fbLink = rAccount.AccountLinks.FirstOrDefault(t => t.Type == SocialType.Facebook);
                        var fbAccessToken = fbLink != null ? fbLink.AccessToken : "";
                        if (string.IsNullOrEmpty(fbAccessToken))
                            throw new CustomException(@GH.Lang.Regit.Your_Account_Does_Not_Link_With_Facebook_Message);
                        newPost.Type = SocialType.Facebook;
                        var savedPost = _socialService.PostNewPost(new PostSocialNetworkViewModel { Post = newPost, Type = SocialType.Facebook }, fbAccessToken, null, fbLink.SocialAccountId);
                        if (string.IsNullOrEmpty(groupId))
                        {
                            groupId = savedPost.Id.ToString();
                            result = savedPost;
                        }

                        postStatus.Add(new { type = "Facebook", sucess = true, message = GH.Lang.Regit.Facebook_posted_successfully });
                        types.Add(SocialType.Facebook);
                    }
                    catch (CustomException ex)
                    {
                        postStatus.Add(new { type = "Facebook", sucess = false, message = "Facebook: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                    }
                    catch (Exception ex)
                    {
                        postStatus.Add(new { type = "Facebook", sucess = false, message = GH.Lang.Regit.Facebook_Cannot_post_on_Facebook_now });
                    }

                  
                    // Write activity log
                    if (rAccount.AccountActivityLogSettings.RecordSocialActivity)
                    {
                        string title = "You posted a new feed to facebook.";
                       
                        var act = new ActivityLogBusinessLogic();
                        act.WriteActivityLogFromAcc(accountId, title, type);
                    }

                }
                if (isPostTwitter)
                {
                    try
                    {
                        var newPost = new SocialPost(message, rAccount.Id, new PostPrivacy { Type = PostPrivacyType.Public }, SocialType.GreenHouse, DateTime.Now, rAccount.Id, DateTime.Now, rAccount.Id, "", "", "", groupId);
                        newPost.Photos = postPitures;
                        var twLink = rAccount.AccountLinks.FirstOrDefault(t => t.Type == SocialType.Twitter);
                        var twAccessToken = twLink != null ? twLink.AccessToken : "";
                        var twSecrect = twLink.AccessTokenSecret;
                        if (string.IsNullOrEmpty(twAccessToken) || string.IsNullOrEmpty(twSecrect))
                            throw new CustomException(GH.Lang.Regit.Your_account_didnt_link_with_Twitter);
                        newPost.Type = SocialType.Twitter;
                        var savedPost = _socialService.PostNewPost(new PostSocialNetworkViewModel { Post = newPost, Type = SocialType.Twitter }, twAccessToken, twSecrect, twLink.SocialAccountId);
                        if (string.IsNullOrEmpty(groupId))
                        {
                            groupId = savedPost.Id.ToString();
                            result = savedPost;
                        }
                        postStatus.Add(new { type = "Twitter", sucess = true, message = GH.Lang.Regit.Twitter_posted_successfully });
                        types.Add(SocialType.Twitter);
                    }
                    catch (CustomException ex)
                    {
                        postStatus.Add(new { type = "Twitter", sucess = false, message = "Twitter: " + string.Join(". ", ex.Errors.Select(e => e.Message).ToList()) });
                    }
                    catch (Exception ex)
                    {
                        postStatus.Add(new { type = "Twitter", sucess = false, message = GH.Lang.Regit.Twitter_Cannot_post_on_Twitter_now });
                    }
                   
                    //WRITE LOG
                    if (rAccount.AccountActivityLogSettings.RecordSocialActivity)
                    {
                        string title = "You posted a new feed to twitter.";
                        var act = new ActivityLogBusinessLogic();
                        act.WriteActivityLogFromAcc(accountId, title, type);
                    }
                }
                
                if (result == null)
                    return Json(new { status = postStatus });

                var responseData = SocialAdapter.SocialConvertToViewModel(result, rAccount, rAccount);
                responseData.Types = types;
                return Json(new { status = postStatus, data = responseData });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpDelete, Route("Delete")]
        public IHttpActionResult DeletePost(string id, bool isPersonal)
        {
            var accountId = ValidateCurrentUser();
            var account = _accountService.GetByAccountId(accountId);
            //_socialService.DeletePost(account.Id, id.ParseToObjectId());
            if (!isPersonal && account.AccountType == AccountType.Personal)
            {
                account = _accountService.GetById(account.BusinessAccountRoles[0].AccountId);
            }

            var listPosts = _socialService.DeletePost(account, id.ParseToObjectId());

            //WRITE LOG
            var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (waccount.AccountActivityLogSettings.RecordSocialActivity)
            {
                string title = "You deleted a post.";
                string type = "Keep a record of your social activities";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
            }

            return Ok(listPosts);
        }

        
        private string ValidateCurrentUser()
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(accountId))
                throw new CustomException("Can not find current user.");
            return accountId;
        }

        /// <summary>
        /// Just for test
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("SyncDataEs")]
        public async Task SyncDataEs()
        {
            IAwsElasticsearchService _esService = new AwsElasticsearchService();
            await _esService.DeleteIndex(AwsElasticsearchService.DELETE_SOCIAL_POST_INDEX_CANONICAL_URI);
            await _esService.Index(AwsElasticsearchService.PUT_SOCIAL_POST_INDEX_CANONICAL_URI, AwsElasticsearchIndexSettings.GetSocialPostIndexSettingsString());
            await _esService.DeleteIndex(AwsElasticsearchService.DELETE_USER_INDEX_CANONICAL_URI);
            await _esService.Index(AwsElasticsearchService.PUT_USER_INDEX_CANONICAL_URI, AwsElasticsearchIndexSettings.GetUserIndexSettingsString());
        }

        [AllowAnonymous]
        [HttpGet, Route("RefreshData")]
        public async Task DeleteOldData()
        {
            var _socialCollection = MongoContext.Db.SocialPosts;
            var filter = new BsonDocument();
            await _socialCollection.DeleteManyAsync(filter);
        }

        private Task UpdatePostsToAwsEs(EsSocialPost[] posts)
        {
            return Task.Run(() =>
            {
                AwsElasticsearchService service = new AwsElasticsearchService();
                service.AddOrUpdateDocuments(AwsElasticsearchService.SOCIAL_POST_INDEX, AwsElasticsearchService.SOCIAL_POST_INDEX_TYPE, "Id", posts).Wait();
            });
        }

        [HttpGet, Route("CheckSocialNetworks")]
        public async Task<CheckSocialNetworkViewModel> CheckSocialNetworks()
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            return new CheckSocialNetworkViewModel
            {
                Facebook = account.AccountLinks.Any(l => l.Type == SocialType.Facebook),
                Twitter = account.AccountLinks.Any(l => l.Type == SocialType.Twitter),
                FacebookPage = account.Pages != null && account.Pages.Any(l => l.SocialType == SocialType.Facebook)
            };
        }

        [HttpGet, Route("CheckBASocialNetworks")]
        public async Task<CheckSocialNetworkViewModel> CheckBASocialNetworks(string id)
        {
            var account = _accountService.GetById(id.ParseToObjectId());
            return new CheckSocialNetworkViewModel
            {
                Facebook = account.AccountLinks.Any(l => l.Type == SocialType.Facebook),
                Twitter = account.AccountLinks.Any(l => l.Type == SocialType.Twitter),
                FacebookPage = account.Pages != null && account.Pages.Any(l => l.SocialType == SocialType.Facebook)
            };
        }

        [HttpGet, Route("GetBusinessFeed")]
        public async Task<List<SocialPostViewModel>> GetpagePosts(string BAId = null, int start = 0, int take = 0)
        {
            var accountId = ValidateCurrentUser();
            var currentUser = _accountService.GetByAccountId(accountId);
            Account businessAccount = null;
            if (string.IsNullOrEmpty(BAId))
            {
                if (currentUser.AccountType == AccountType.Personal)
                    businessAccount = _accountService.GetById(currentUser.BusinessAccountRoles[0].AccountId);
            }
            else
            {
                businessAccount = _accountService.GetById(BAId.ParseToObjectId());
                var isRoleBusiness = true;
                if (currentUser.AccountType == AccountType.Personal && businessAccount.BusinessAccountRoles.Count > 0)
                {
                    isRoleBusiness = businessAccount.BusinessAccountRoles.FirstOrDefault(x => x.AccountId == currentUser.Id) == null;
                }

                if (currentUser.Id != businessAccount.Id && currentUser.Followees.All(t => t.AccountId != businessAccount.Id) && isRoleBusiness)
                    return new List<SocialPostViewModel>();
            }

            //if businessAccount != currentUser => check follow
            if (businessAccount == null)
            {
                return new List<SocialPostViewModel>();
            }
            try
            {
                var socialPosts = await _socialService.GetHomeFeed(businessAccount.Id, start, take);

                var accountIds = socialPosts.Select(t => t.AccountId).Distinct().ToList();
                var accounts = _accountService.GetByListId(accountIds);
                var isAdminRole = _roleService.GetRolesOfAccount(currentUser, businessAccount.Id).Count(x => x.Name == Role.ROLE_ADMIN) > 0;
                var socialPostViewModels = new List<SocialPostViewModel>();
                var relatedPost = _socialPostService.GetRelatedGroupPost(socialPosts.Select(s => s.Id.ToString()).ToList());
                foreach (var model in socialPosts)
                {
                    var account = accounts.FirstOrDefault(t => t.Id == model.AccountId);
                    var viewModel = SocialAdapter.SocialConvertToViewModel(model, account, currentUser,
                        relatedPost.Where(s => s.GroupId == model.Id.ToString()).ToList(), null, null, true, isAdminRole);
                    socialPostViewModels.Add(viewModel);
                }

                return socialPostViewModels;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpPost, Route("BAPostANewFeed")]
        public async Task<dynamic> BAPostANewFeed()
        {
            try
            {
                var accountId = ValidateCurrentUser();
                var rAccount = _accountService.GetByAccountId(accountId);
                var baAccount = rAccount;
                List<SocialType> socialTypes = new List<SocialType>();

                if (rAccount.AccountType == AccountType.Personal)
                {
                    baAccount = _accountService.GetById(rAccount.BusinessAccountRoles[0].AccountId);
                    var roles = _roleService.GetRolesOfAccount(rAccount, baAccount.Id).Where(s => s.Name == Role.ROLE_EDITOR).FirstOrDefault();
                    if (roles == null)
                        throw new CustomException("You do not have permission to post.");
                }

                var posts = new List<SocialPost>();
                List<object> postStatus = new List<object>();

                // Check if the request contains multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                string root = HttpContext.Current.Server.MapPath("~/App_Data");
                var provider = new MultipartFormDataStreamProvider(root);

                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                var message = provider.FormData["Message"];
                var isPostGreenHouse = provider.FormData["IsPostGreenHouse"].ParseBool();
                var isPostFacebook = provider.FormData["IsPostFacebook"].ParseBool();
                var isPostTwitter = provider.FormData["IsPostTwitter"].ParseBool();
                var isPrivate = provider.FormData["IsPrivate"].ParseBool();
                var privacy = PostPrivacyType.Public;
                if (isPrivate)
                    privacy = PostPrivacyType.Private;

                System.Web.HttpFileCollection hfc = HttpContext.Current.Request.Files;

                if (!isPostFacebook && !isPostGreenHouse && !isPostTwitter)
                    throw new CustomException("You must choose at least one social network before posting.");
                if (string.IsNullOrEmpty(message))
                    throw new CustomException("The message is not empty.");
                if (isPostTwitter && message.Length > 140)
                    throw new CustomException("Twitter message lenght is not greater than 140 letters.");

                if (isPostGreenHouse)
                    socialTypes.Add(SocialType.GreenHouse);
                if (isPostFacebook)
                    socialTypes.Add(SocialType.Facebook);
                if (isPostTwitter)
                    socialTypes.Add(SocialType.Twitter);
                BusinessPost savedPost = null;
                var result = new List<SocialPostViewModel>();
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Business_upload/");
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }
                string baseUrl = "/Business_upload/";
                List<Photo> postPitures = new List<Photo>();
                for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
                {
                    System.Web.HttpPostedFile hpf = hfc[iCnt];

                    if (hpf.ContentLength > 0)
                    {

                        string filename = Guid.NewGuid() + Path.GetFileName(hpf.FileName);
                        // CHECK IF THE SELECTED FILE(S) ALREADY EXISTS IN FOLDER. (AVOID DUPLICATE)
                        if (!File.Exists(sPath + Path.GetFileName(hpf.FileName)))
                        {
                            // SAVE THE FILES IN THE FOLDER.
                            hpf.SaveAs(sPath + filename);
                            postPitures.Add(new Photo() { Url = baseUrl + filename });
                        }
                    }
                }

                if (postPitures.Count > 1)
                {
                    throw new CustomException("Cannot upload more than 1 photo");
                }

                //save post to db as draff

                try
                {
                    var pageId = baAccount.Pages.Count > 0 ? baAccount.Pages.First().Id : "";
                    if (socialTypes.Any(t => t == SocialType.Facebook) && string.IsNullOrEmpty(pageId))
                        throw new CustomException("Please link your account to social network before posting.");
                    savedPost = new BusinessPost(message, rAccount.Id, baAccount.Id, pageId, new PostPrivacy { Type = privacy }, socialTypes, DateTime.Now, rAccount.Id, DateTime.Now, rAccount.Id, postPitures);
                    postStatus.Add(new { sucess = true, message = "Posted successfully" });
                    savedPost = _socialPageService.Insert(savedPost);

                    var reviewRole = _roleService.GetRoleByName(Role.ROLE_REVIEWER);
                    var now = DateTime.Now;

                    var businessReviewerMembers = baAccount.BusinessAccountRoles.Where(a => a.RoleId != null && a.RoleId == reviewRole.Id).Select(a => a.AccountId).ToList();

                    var notifyTo = baAccount.BusinessAccountRoles.Where(a => a.RoleId != null && a.RoleId == reviewRole.Id).Select(a => new Notification
                    {
                        CreatedAt = now,
                        Creator = rAccount.Id,
                        ReceiverId = a.AccountId,
                        TargetType = NotificationTargetType.ReviewPost,
                        TargetId = savedPost.Id
                    }).ToList();
                    notifyTo.Add(new Notification
                    {
                        CreatedAt = now,
                        Creator = rAccount.Id,
                        ReceiverId = baAccount.Id,
                        TargetType = NotificationTargetType.ReviewPost,
                        TargetId = savedPost.Id
                    });
                    _notifyService.AddNotifications(notifyTo.ToArray());
                }
                catch (CustomException ex)
                {
                    postStatus.Add(new { sucess = false, message = "Posted failure: " + ex.Errors.Select(e => e.Message) });
                }
                catch (Exception ex)
                {
                    postStatus.Add(new { sucess = false, message = "Posted failure: Cannot post on now!" });
                }

                // Write activity log
                var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (waccount.AccountActivityLogSettings.RecordWorkflow)
                {
                    string title = "You posted a new feed for your business.";
                    string type = "Keep a record of your business social activities";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
                }
                return Json(new { status = postStatus, data = savedPost });
            }
            catch (Exception exception)
            {
                throw exception;
            }

          
        }

        [HttpPost, Route("BAEditPostFeed")]
        public async Task<dynamic> BAEditAPost()
        {
            try
            {
                var accountId = ValidateCurrentUser();
                var rAccount = _accountService.GetByAccountId(accountId);
                var baAccount = rAccount;
                List<SocialType> socialTypes = new List<SocialType>();

                if (rAccount.AccountType == AccountType.Personal)
                {
                    baAccount = _accountService.GetById(rAccount.BusinessAccountRoles[0].AccountId);
                    var roles = _roleService.GetRolesOfAccount(rAccount, baAccount.Id).Where(s => s.Name == Role.ROLE_EDITOR).FirstOrDefault();
                    if (roles == null)
                        throw new CustomException("You do not have permission to post.");
                }

                var posts = new List<SocialPost>();
                object postStatus;

                // Check if the request contains multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                string root = HttpContext.Current.Server.MapPath("~/App_Data");
                var provider = new MultipartFormDataStreamProvider(root);

                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);
                var PostId = provider.FormData["Id"];
                var EditPost = _socialPageService.GetBusinessPost(ObjectId.Parse(PostId));
                var message = provider.FormData["Message"];
                var isPostGreenHouse = provider.FormData["IsPostGreenHouse"].ParseBool();
                var isPostFacebook = provider.FormData["IsPostFacebook"].ParseBool();
                var isPostTwitter = provider.FormData["IsPostTwitter"].ParseBool();
                var isPrivate = provider.FormData["IsPrivate"].ParseBool();
                var deletedPhotos = provider.FormData["DeletedPhoto"];
                var privacy = PostPrivacyType.Public;
                if (isPrivate)
                    privacy = PostPrivacyType.Private;

                if (PostId == null)
                {
                    throw new CustomException("Post not found!");
                }
                System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;

                if (!isPostFacebook && !isPostGreenHouse && !isPostTwitter)
                    throw new CustomException("You must choose at least one social network before posting.");
                if (string.IsNullOrEmpty(message))
                    throw new CustomException("The message is not empty.");
                if (isPostTwitter && message.Length > 140)
                    throw new CustomException("Twitter message lenght is not greater than 140 letters.");

                if (isPostGreenHouse)
                    socialTypes.Add(SocialType.GreenHouse);
                if (isPostFacebook)
                    socialTypes.Add(SocialType.Facebook);
                if (isPostTwitter)
                    socialTypes.Add(SocialType.Twitter);
                BusinessPost savedPost = null;
                var result = new List<SocialPostViewModel>();
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Business_upload/");
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }
                string baseUrl = "/Business_upload/";
                List<Photo> postPitures = new List<Photo>();
                var deletedPhoto = deletedPhotos.Split(';');
                foreach (var item in EditPost.Photos)
                {
                    if (!deletedPhoto.Contains(item.Url))
                    {
                        postPitures.Add(item);
                    }
                }
                for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
                {
                    System.Web.HttpPostedFile hpf = hfc[iCnt];

                    if (hpf.ContentLength > 0)
                    {

                        string filename = Guid.NewGuid() + Path.GetFileName(hpf.FileName);
                        // CHECK IF THE SELECTED FILE(S) ALREADY EXISTS IN FOLDER. (AVOID DUPLICATE)
                        if (!File.Exists(sPath + Path.GetFileName(hpf.FileName)))
                        {
                            // SAVE THE FILES IN THE FOLDER.
                            hpf.SaveAs(sPath + filename);
                            postPitures.Add(new Photo() { Url = baseUrl + filename });
                        }
                    }
                }
                //save post to db as draff

                try
                {

                    EditPost.Message = message;
                    EditPost.ModifiedBy = rAccount.Id;
                    EditPost.ModifiedOn = DateTime.Now;
                    EditPost.Photos = postPitures;
                    EditPost.SocialTypes = socialTypes;
                    EditPost.Status = PostStatus.Edited;
                    _socialPageService.EditPost(EditPost, rAccount.Id);
                    postStatus = new { sucess = true, message = "Edit post successfully", Id = EditPost.Id.ToString() };
                    var reviewRole = _roleService.GetRoleByName(Role.ROLE_REVIEWER);
                    var now = DateTime.Now;
                    var notifyTo = baAccount.BusinessAccountRoles.Where(a => a.RoleId != null && a.RoleId == reviewRole.Id).Select(a => new Notification
                    {
                        CreatedAt = now,
                        Creator = rAccount.Id,
                        ReceiverId = a.AccountId,
                        TargetType = NotificationTargetType.ReviewPost,
                        TargetId = EditPost.Id
                    }).ToList();
                    notifyTo.Add(new Notification
                    {
                        CreatedAt = now,
                        Creator = rAccount.Id,
                        ReceiverId = baAccount.Id,
                        TargetType = NotificationTargetType.ReviewPost,
                        TargetId = EditPost.Id
                    });
                    _notifyService.AddNotifications(notifyTo.ToArray());
                }
                catch (CustomException ex)
                {
                    postStatus = new { sucess = false, message = "Edit posted failure: " + ex.Errors.Select(e => e.Message) };
                }
                catch (Exception ex)
                {
                    postStatus = new { sucess = false, message = "Edit posted failure: Cannot post on now!" };
                }


                // Write activity log
                var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (waccount.AccountActivityLogSettings.RecordWorkflow)
                {
                    string title = "You edited post for your business.";
                    string type = "Keep a record of your business social activities";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
                }
                return Json(postStatus);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpGet, Route("BusinessPost")]
        public async Task<BusinessPostViewModel> GetBusinessPost(string id)
        {
            var post = _socialPageService.GetBusinessPost(new ObjectId(id));

            if (post == null)
            {
                throw new CustomException("The post does not exist");
            }

            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            var creator = _accountService.GetById(post.AccountId);
            var stateFlowExecutors = _accountService.GetByListId(post.Workflows.Select(w => w.Executor).ToList());

            //WRITE LOG
            var waccount = _accountService.GetByAccountId(User.Identity.GetUserId());
            if (waccount.AccountActivityLogSettings.RecordWorkflow)
            {
                string title = "You got business post.";
                string type = "Keep a record of your business social activities";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(waccount.AccountId, title, type);
            }
            return BusinessPostAdapter.ConvertToViewModel(post, creator, stateFlowExecutors);
        }

        [HttpGet, Route("SearchUsersForPersonalScope")]
        public async Task<IEnumerable<AccountViewModel>> SearchUsersForPersonalScope(string keyword, int? start = null, int? length = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            var query = new EsSearchQueryString
            {
                query = new EsQueryStringType
                {
                    query_string = new EsQueryStringValue
                    {
                        query = string.Format("NOT _id:{0} AND Related:{0} AND Name:({1})", currentUser.Id, keyword)
                    }
                },
                sort = new object[] { new { _score = "desc", Name = "asc" } },
                fields = new string[] { }
            };

            IAwsElasticsearchService _esService = new AwsElasticsearchService();
            var esResult = await _esService.SearchDocuments(AwsElasticsearchService.SEARCH_USER_API_CANONICAL_URI, query, start, length);

            if (esResult.hits != null && esResult.hits.total > 0)
            {
                var userIds = esResult.hits.hits.Select(e => new ObjectId(e._id.ToString())).ToList();
                var users = _accountService.GetByListId(userIds);

                users = users.OrderByDescending(s => esResult.hits.hits.FirstOrDefault(f => f._id.ToString() == s.Id.ToString())._score).ToList();

                var accountViewModels = new List<AccountViewModel>();
                foreach (var model in users)
                {
                    var viewModel = AccountAdapter.ConvertToViewModel(model);
                    accountViewModels.Add(viewModel);
                }

                return accountViewModels;
            }
            else
            {
                return new AccountViewModel[] { };
            }
        }


        [HttpGet, Route("SearchUsersForGlobalScope")]
        public async Task<IEnumerable<AccountViewModel>> SearchUsersForGlobalScope(string keyword, int? start = null, int? length = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());

            var query = new EsSearchQueryString
            {
                query = new EsQueryStringType
                {
                    query_string = new EsQueryStringValue
                    {
                        query = string.Format("NOT _id:{0} AND NOT Related:{0} AND Name:({1})", currentUser.Id, keyword)
                    }
                },
                sort = new object[] { new { _score = "desc", Name = "asc" } },
                fields = new string[] { }
            };

            IAwsElasticsearchService _esService = new AwsElasticsearchService();
            var esResult = await _esService.SearchDocuments(AwsElasticsearchService.SEARCH_USER_API_CANONICAL_URI, query, start, length);

            if (esResult.hits != null && esResult.hits.total > 0)
            {
                var userIds = esResult.hits.hits.Select(e => new ObjectId(e._id.ToString())).ToList();
                var users = _accountService.GetByListId(userIds);

                users = users.OrderByDescending(s => esResult.hits.hits.FirstOrDefault(f => f._id.ToString() == s.Id.ToString())._score).ToList();

                var accountViewModels = new List<AccountViewModel>();
                foreach (var model in users)
                {
                    var viewModel = AccountAdapter.ConvertToViewModel(model);
                    accountViewModels.Add(viewModel);
                }

                return accountViewModels;
            }
            else
            {
                return new AccountViewModel[] { };
            }
        }

        [HttpGet, Route("SearchPostsForGlobalScope")]
        public async Task<IEnumerable<SocialPostViewModel>> SearchPostsForGlobalScope([FromUri]SearchSocialPostCriteria criteria)
        {
            SearchSocialPostsOnAwsEsCriteria esCriteria = new SearchSocialPostsOnAwsEsCriteria();
            List<SocialType> socialNetworks = new List<SocialType>();
            var currentAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (criteria.Facebook)
            {
                socialNetworks.Add(SocialType.Facebook);
            }
            if (criteria.Twitter)
            {
                socialNetworks.Add(SocialType.Twitter);
            }
            if (criteria.Regit)
            {
                socialNetworks.Add(SocialType.GreenHouse);
            }

            esCriteria.SocialNetwork = socialNetworks.ToArray();

            if (criteria.From.HasValue)
            {
                esCriteria.FromTime = criteria.From.Value;
            }

            if (criteria.To.HasValue)
            {
                esCriteria.ToTime = criteria.To.Value;
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                esCriteria.Keyword = criteria.Keyword;
            }

            esCriteria.Start = criteria.Start;
            esCriteria.Length = criteria.Length;

            var related = _accountService.GetAccountIdsRelatedToUsers(currentAccount.Id).First();
            related.Value.Add(currentAccount.Id);
            esCriteria.NotCreatorIds = related.Value.Select(v => v.ToString()).ToArray();

            esCriteria.SearchForGlobal = true;
            esCriteria.SearchUser = currentAccount.Id.ToString();

            var query = new EsSearchQueryString
            {
                query = new EsQueryStringType
                {
                    query_string = new EsQueryStringValue
                    {
                        query = esCriteria.ToEsQueryString()
                    }
                },
                sort = new object[] { new { _score = "desc", CreatedTime = "desc" } },
                fields = new string[] { }
            };

            IAwsElasticsearchService _esService = new AwsElasticsearchService();
            var esResult = await _esService.SearchDocuments(AwsElasticsearchService.SEARCH_SOCIAL_POST_API_CANONICAL_URI, query, esCriteria.Start, esCriteria.Length);

            if (esResult.hits != null && esResult.hits.total > 0)
            {
                var socialPostIds = esResult.hits.hits.Select(e => new ObjectId(e._id.ToString()));
                var socialPosts = _socialPostService.GetByIds(socialPostIds);

                var relatedPost = _socialPostService.GetRelatedGroupPost(socialPosts.Select(s => s.Id.ToString()).ToList());

                socialPosts = socialPosts.OrderByDescending(s => esResult.hits.hits.FirstOrDefault(f => f._id.ToString() == s.Id.ToString())._score).ThenBy(s => s.CreatedOn).ToList();

                var accountIds = socialPosts.Select(t => t.AccountId).Distinct().ToList();
                var accounts = _accountService.GetByListId(accountIds);
                var socialPostViewModels = new List<SocialPostViewModel>();
                foreach (var model in socialPosts)
                {
                    var account = accounts.FirstOrDefault(t => t.Id == model.AccountId);
                    var viewModel = SocialAdapter.SocialConvertToViewModel(model, account, currentAccount, relatedPost.Where(r => r.GroupId == model.Id.ToString()).ToList());
                    socialPostViewModels.Add(viewModel);
                }

                return socialPostViewModels;
            }
            else
            {
                return new SocialPostViewModel[] { };
            }
        }


        [HttpGet, Route("SearchPostsForPersonalScope")]
        public async Task<IEnumerable<SocialPostViewModel>> SearchPostsForPersonalScope([FromUri]SearchSocialPostCriteria criteria)
        {
            SearchSocialPostsOnAwsEsCriteria esCriteria = new SearchSocialPostsOnAwsEsCriteria();
            List<SocialType> socialNetworks = new List<SocialType>();
            var currentAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (criteria.Facebook)
            {
                socialNetworks.Add(SocialType.Facebook);
            }
            if (criteria.Twitter)
            {
                socialNetworks.Add(SocialType.Twitter);
            }
            if (criteria.Regit)
            {
                socialNetworks.Add(SocialType.GreenHouse);
            }

            esCriteria.SocialNetwork = socialNetworks.ToArray();

            if (criteria.From.HasValue)
            {
                esCriteria.FromTime = criteria.From.Value;
            }

            if (criteria.To.HasValue)
            {
                esCriteria.ToTime = criteria.To.Value;
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                esCriteria.Keyword = criteria.Keyword;
            }

            esCriteria.Start = criteria.Start;
            esCriteria.Length = criteria.Length;

            var related = _accountService.GetAccountIdsRelatedToUsers(currentAccount.Id).First();
            related.Value.Add(currentAccount.Id);
            esCriteria.CreatorIds = related.Value.Select(v => v.ToString()).ToArray();

            esCriteria.SearchForGlobal = false;
            esCriteria.SearchUser = currentAccount.Id.ToString();

            var query = new EsSearchQueryString
            {
                query = new EsQueryStringType
                {
                    query_string = new EsQueryStringValue
                    {
                        query = esCriteria.ToEsQueryString()
                    }
                },
                sort = new object[] { new { _score = "desc", CreatedTime = "desc" } },
                fields = new string[] { }
            };

            IAwsElasticsearchService _esService = new AwsElasticsearchService();
            var esResult = await _esService.SearchDocuments(AwsElasticsearchService.SEARCH_SOCIAL_POST_API_CANONICAL_URI, query, esCriteria.Start, esCriteria.Length);

            if (esResult.hits != null && esResult.hits.total > 0)
            {
                var socialPostIds = esResult.hits.hits.Select(e => new ObjectId(e._id.ToString()));
                var socialPosts = _socialPostService.GetByIds(socialPostIds);

                var relatedPost = _socialPostService.GetRelatedGroupPost(socialPosts.Select(s => s.Id.ToString()).ToList());

                socialPosts = socialPosts.OrderByDescending(s => esResult.hits.hits.FirstOrDefault(f => f._id.ToString() == s.Id.ToString())._score).ThenBy(s => s.CreatedOn).ToList();

                var accountIds = socialPosts.Select(t => t.AccountId).Distinct().ToList();
                var accounts = _accountService.GetByListId(accountIds);
                var socialPostViewModels = new List<SocialPostViewModel>();
                foreach (var model in socialPosts)
                {
                    var account = accounts.FirstOrDefault(t => t.Id == model.AccountId);
                    var viewModel = SocialAdapter.SocialConvertToViewModel(model, account, currentAccount, relatedPost.Where(r => r.GroupId == model.Id.ToString()).ToList());
                    socialPostViewModels.Add(viewModel);
                }

                return socialPostViewModels;
            }
            else
            {
                return new SocialPostViewModel[] { };
            }
        }

    }
}
