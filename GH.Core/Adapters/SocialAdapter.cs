using GH.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using GH.Core.Helpers;
using GH.Core.ViewModels;
using Tweetinvi.Models;
using MongoDB.Bson;

namespace GH.Core.Adapters
{
    public class SocialAdapter
    {
        public static SocialPost FacebookConvertToModel(Account account, dynamic post)
        {
            DateTime createdDateTime = DateTime.Parse(post.created_time);
            var model = new SocialPost
            {
                AccountId = account.Id,
                Privacy = new PostPrivacy { Type = PostPrivacyType.Public },
                Type = SocialType.Facebook,
                SocialId = post.id,
                CreatedOn = createdDateTime,
                CreatedBy = account.Id,
                ModifiedBy = account.Id,
                ModifiedOn = createdDateTime,
                Message = ""
            };
            //update privacy
            if (post.privacy.value == "ALL_FRIENDS" || post.privacy.value == "CUSTOM")
                model.Privacy.Type = PostPrivacyType.Friends;
            else if (post.privacy.value == "SELF")
                model.Privacy.Type = PostPrivacyType.Private;

            //parse the message with story
            if (CommonFunctions.JsonHasProperty(post, "message"))
            {
                model.Message += post.message;
                //if (CommonFunctions.JsonHasProperty(post, "story"))
                //    model.Message += " - " + post.story;
            }
            //comments
            if (CommonFunctions.JsonHasProperty(post, "comments"))
            {
                foreach (var comment in post.comments.data)
                {
                    model.Comments.Add(new Comment { SocialId = comment.id, CreatedOn = DateTime.Parse(comment.created_time), Message = comment.message, From = new SocialAccount { Id = comment.from.id, DisplayName = comment.from.name } });
                }
            }
            //if there are 2 or more 2 photos, json has attachments, if it has only photo, attachment is not availible, instead of full_picture
            var photos = new List<Photo>();
            if (CommonFunctions.JsonHasProperty(post, "attachments"))
            {
                foreach (var item in post.attachments.data[0].subattachments.data)
                {
                    photos.Add(new Photo { Url = item.media.image.src });
                }
            }
            if (CommonFunctions.JsonHasProperty(post, "likes"))
            {
                model.Like = new Like { TwitterLikeCount = 0, RegitLikes = new List<ClassObjectId>(), FaceBookLikes = new List<SocialAccount>() };
                foreach (var like in post.likes.data)
                {
                    model.Like.FaceBookLikes.Add(new SocialAccount { Id = like.id, DisplayName = like.name });
                }
                //if (post.likes.summary.has_liked)
                //    model.Like.RegitLikeIds.Add(account.Id);
            }

            if (CommonFunctions.JsonHasProperty(post, "source"))
            {
                model.VideoUrl = post.source;
                photos.Add(new Photo { Url = post.full_picture });
            }

            if (CommonFunctions.JsonHasProperty(post, "shares"))
            {
                model.Shares = Int32.Parse(post.shares.count.ToString());
            }
            else if (!CommonFunctions.JsonHasProperty(post, "attachments") && CommonFunctions.JsonHasProperty(post, "full_picture"))
                photos.Add(new Photo { Url = post.full_picture });
            model.Photos = photos;
            return model;
        }

        public static SocialPost TwitterConvertToModel(Account account, ITweet tweet, List<ITweet> comments = null)
        {
            var videoUrl = "";
            var message = tweet.Text.Contains("https://t.co/") ? tweet.Text.Substring(0, tweet.Text.IndexOf("https://t.co/")).Trim() : tweet.Text;
            var model = new SocialPost
            {
                Message = message,
                SocialId = tweet.IdStr,
                Type = SocialType.Twitter,
                Privacy = new PostPrivacy
                {
                    Type = PostPrivacyType.Public
                },
                CreatedOn = tweet.CreatedAt,
                ModifiedOn = tweet.CreatedAt,
                AccountId = account.Id,
                CreatedBy = account.Id,
                ModifiedBy = account.Id
            };
            //comvert comment
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    model.Comments.Add(TwitterConvertToCommentModel(comment, account));
                }
            }
            //convert photo
            List<Photo> listOfPhoto = new List<Photo>();
            var lOfMedia = tweet.Media.ToList();
            foreach (var media in lOfMedia)
            {
                var photo = new Photo
                {
                    Url = media.MediaURL
                };
                var video_details = media.VideoDetails;
                if (video_details != null)
                {
                    foreach (var variant in video_details.Variants)
                    {
                        var url = variant.URL;

                        if (url.EndsWith("mp4"))
                        {
                            videoUrl = url;
                            break;
                        }
                    }
                }
                listOfPhoto.Add(photo);
            }

            model.Photos = listOfPhoto;
            if (videoUrl != "")
                model.VideoUrl = videoUrl;

            //convert like
            Like like = new Like { RegitLikes = new List<ClassObjectId>() };
            like.TwitterLikeCount = tweet.FavoriteCount;
            if (tweet.Favorited)//if I like my post
            {
                like.RegitLikes.Add(new ClassObjectId { Id = account.Id });
                like.TwitterLikeCount--;
            }
            model.Like = like;
            model.Shares = tweet.RetweetCount;

            return model;
        }

        public static SocialPost BusinessPostToModel(BusinessPost businessPost)
        {
            var model = new SocialPost
            {
                AccountId = businessPost.BusinessAccountId,
                CreatedBy = businessPost.CreatedBy,
                CreatedOn = businessPost.CreatedOn,
                ModifiedBy = businessPost.ModifiedBy,
                ModifiedOn = businessPost.ModifiedOn,
                Message = businessPost.Message,
                Privacy = businessPost.Privacy,
                SocialPageId = businessPost.SocialPageId,
                Photos = businessPost.Photos,
                VideoUrl = businessPost.VideoUrl
            };

            return model;
        }

        public static Comment TwitterConvertToCommentModel(ITweet tweet, Account account)
        {
            var message = tweet.Text.Contains("https://t.co/") ? tweet.Text.Substring(0, tweet.Text.IndexOf("https://t.co/")).Trim() : tweet.Text;
            var comment = new Comment { SocialId = tweet.IdStr, CreatedOn = tweet.CreatedAt, Message = message, From = new SocialAccount { DisplayName = tweet.CreatedBy.Name, Link = "https://twitter.com/" + tweet.CreatedBy.ScreenName, Id = tweet.CreatedBy.IdStr, PhotoUrl = tweet.CreatedBy.ProfileImageUrlFullSize } };
            if (comment.From.Id == account.SocialAccountId)
                comment.UserId = account.Id;
            return comment;
        }

        private static bool CheckIsCanDeleted(bool isBusinessFeed, bool isAdmin, Account createdUser, Account currentUser)
        {
            if (isBusinessFeed)
            {
                if (currentUser.AccountType == AccountType.Business)
                {
                    return true;
                }
                if (currentUser.AccountType == AccountType.Personal && isAdmin)
                {
                    return true;
                }
            }
            else
            {
                if (createdUser.Id == currentUser.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public static SocialPostViewModel SocialConvertToViewModel(SocialPost model, Account accountOfPost, Account currentAccount,
            List<SocialPost> relatedPosts = null, SocialPost postShared = null, Account accountShared = null, bool isBusinessFeed = false, bool isAdmin = false)
        {
            var viewModel = new SocialPostViewModel();
            if (model == null)
                return viewModel;
            viewModel.Id = model.Id.ToString();
            viewModel.SocialId = model.SocialId;
            viewModel.Privacy = model.Privacy.Type;
            viewModel.Types = new List<SocialType>() { model.Type };
            viewModel.Message = model.Message;
            viewModel.Photos = model.Photos == null ? viewModel.Photos : model.Photos.ToList();
            viewModel.TotalLike = model.Like != null ? model.Like.TotalLikes : 0;
            viewModel.TotalComment = model.Comments != null ? model.Comments.Count() : 0;
            viewModel.RefObjectId = model.RefObjectId.ToString();
            viewModel.VideoUrl = model.VideoUrl;
            viewModel.CreatedOn = model.CreatedOn;
            viewModel.Account = AccountAdapter.ConvertToViewModel(accountOfPost);
            viewModel.TotalShares = model.Shares;
            viewModel.IsCanDeleted = CheckIsCanDeleted(isBusinessFeed, isAdmin, accountOfPost, currentAccount);
            viewModel.IsLiked = CheckIsLiked(model, currentAccount);
            if (relatedPosts != null)
            {
                foreach (var item in relatedPosts)
                {
                    if (viewModel.Types.Contains(item.Type))
                    {
                        continue;
                    }
                    viewModel.Types.Add(item.Type);
                    viewModel.TotalShares += item.Shares;
                    viewModel.TotalLike += item.Like.TotalLikes;
                    viewModel.TotalComment += item.Comments.Count;
                }
            }

            if (postShared != null)
            {
                viewModel.IsSharePost = true;
                viewModel.PostShared.Id = postShared.Id.ToString();
                viewModel.PostShared.Message = postShared.Message;
                viewModel.PostShared.Photos = postShared.Photos == null ? viewModel.PostShared.Photos : postShared.Photos.ToList();
                viewModel.PostShared.SocialId = postShared.SocialId;
                viewModel.PostShared.VideoUrl = postShared.VideoUrl;
                viewModel.PostShared.Account = AccountAdapter.ConvertToViewModel(accountShared);
            }

            return viewModel;
        }

        public static bool CheckIsLiked(SocialPost socialPost, Account account)
        {
            var isLiked = socialPost.Like.RegitLikes.Any(t => t.Id == account.Id);
            if (account.AccountType == AccountType.Business)
            {
                if (account.Pages != null && account.Pages.Count > 0)
                {
                    foreach (var socialPage in account.Pages)
                    {
                        isLiked = isLiked || socialPost.Like.FaceBookLikes.Any(t => t.Id == socialPage.Id);
                    }
                }
                else
                {
                    var accountFacebook = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                    if (accountFacebook != null)
                    {
                        isLiked = isLiked || socialPost.Like.FaceBookLikes.Any(t => t.Id == accountFacebook.SocialAccountId);
                    }
                }
            }
            else
            {
                var accountFacebook = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                if (accountFacebook != null)
                {
                    isLiked = isLiked || socialPost.Like.FaceBookLikes.Any(t => t.Id == accountFacebook.SocialAccountId);
                }
            }

            return isLiked;
        }

        public static CommentViewModel CommentConvertToViewModel(Comment model, Account account)
        {
            var viewModel = new CommentViewModel
            {
                Id = model.Id.ToString(),
                Message = model.Message,
                UserId = model.UserId.ToString(),
                SocialId = model.SocialId,
                Account = AccountAdapter.ConvertToViewModel(account),
                SocialAccount = SocialAccountConvertToViewModel(model.From),
                CreatedOn = model.CreatedOn
            };
            if (!string.IsNullOrEmpty(viewModel.Account.Id))
                viewModel.IsUserInSystem = true;
            return viewModel;
        }

        public static SocialAccountViewModel SocialAccountConvertToViewModel(SocialAccount model)
        {
            return new SocialAccountViewModel
            {
                Id = model.Id,
                DisplayName = model.DisplayName,
                PhotoUrl = string.IsNullOrEmpty(model.PhotoUrl) ? "/Areas/User/Content/Images/no-pic.png" : model.PhotoUrl,
                Link = model.Link
            };
        }
    }
}