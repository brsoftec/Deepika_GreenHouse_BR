using System.Linq;
using GH.Core.Collectors;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.Services
{
    public class ProcessPostService : IProcessPostService
    {
        private IMongoCollection<SocialPost> _socialPostCollection;
        private IFacebookCollector _facebookCollector;
        private ITwitterCollector _twitterCollector;

        public ProcessPostService()
        {
            var mongoDb = MongoContext.Db;
            _socialPostCollection = mongoDb.SocialPosts;
            _facebookCollector = new FacebookCollector();
            _twitterCollector = new TwitterCollector();
        }


        public UpdateResult UnLikeAsAccountTwitter(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink)
        {
            _twitterCollector.Unfavorite(accountLink.AccessToken, accountLink.AccessTokenSecret, socialPost.SocialId);
            var removeItem = socialPost.Like.RegitLikes.FirstOrDefault(t => t.Id == account.Id);
            socialPost.Like.RegitLikes.Remove(removeItem);
            var update = Builders<SocialPost>.Update.PullFilter(t => t.Like.RegitLikes, s => s.Id == removeItem.Id);
            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UnLikeAsAccountRegit(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account)
        {
            var removeItem = socialPost.Like.RegitLikes.FirstOrDefault(t => t.Id == account.Id);
            socialPost.Like.RegitLikes.Remove(removeItem);
            var update = Builders<SocialPost>.Update.PullFilter(t => t.Like.RegitLikes, s => s.Id == removeItem.Id);
            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UnLikeAsAccountPages(FilterDefinition<SocialPost> filter, SocialPost socialPost, Account account, SocialPage accountPage)
        {
            UpdateDefinition<SocialPost> update;
            var result = _facebookCollector.UnLikesAPost(accountPage.AccessToken, socialPost.SocialId);

            if (result.success == true)
            {
                var removeItem = socialPost.Like.FaceBookLikes.FirstOrDefault(x => x.Id == accountPage.Id);
                socialPost.Like.FaceBookLikes.Remove(removeItem);
                update = Builders<SocialPost>.Update.PullFilter(t => t.Like.FaceBookLikes, s => s.Id == removeItem.Id);
            }
            else
            {
                var removeItem = socialPost.Like.RegitLikes.FirstOrDefault(t => t.Id == account.Id);
                socialPost.Like.RegitLikes.Remove(removeItem);
                update = Builders<SocialPost>.Update.PullFilter(t => t.Like.RegitLikes, s => s.Id == removeItem.Id);
            }

            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UnLikeAsAccountFacebook(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink)
        {
            UpdateDefinition<SocialPost> update;
            var result = _facebookCollector.UnLikesAPost(accountLink.AccessToken, socialPost.SocialId);

            if (result.success == true)
            {
                var removeItem = socialPost.Like.FaceBookLikes.FirstOrDefault(x => x.Id == accountLink.SocialAccountId);
                socialPost.Like.FaceBookLikes.Remove(removeItem);
                update = Builders<SocialPost>.Update.PullFilter(t => t.Like.FaceBookLikes, s => s.Id == removeItem.Id);
            }
            else
            {
                var removeItem = socialPost.Like.RegitLikes.FirstOrDefault(t => t.Id == account.Id);
                socialPost.Like.RegitLikes.Remove(removeItem);
                update = Builders<SocialPost>.Update.PullFilter(t => t.Like.RegitLikes, s => s.Id == removeItem.Id);
            }

            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UpdateLikeAsAccountRegit(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account)
        {
            var update = Builders<SocialPost>.Update.AddToSet(t => t.Like.RegitLikes,
                new ClassObjectId { Id = account.Id });
            socialPost.Like.RegitLikes.Add(new ClassObjectId { Id = account.Id });
            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UpdateLikeAsAccountTwitter(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink)
        {
            UpdateDefinition<SocialPost> update;
            var result = _twitterCollector.Favorite(accountLink.AccessToken, accountLink.AccessTokenSecret,
                socialPost.SocialId);

            if (result)
            {
                var totalLike = socialPost.Like.TwitterLikeCount++;
                update = Builders<SocialPost>.Update.Set(t => t.Like.TwitterLikeCount, totalLike);
            }
            else
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.RegitLikes,
                    new ClassObjectId { Id = account.Id });
                socialPost.Like.RegitLikes.Add(new ClassObjectId { Id = account.Id });
            }

            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UpdateLikeAsAccountFacebook(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink)
        {
            UpdateDefinition<SocialPost> update;
            var result = _facebookCollector.LikesAPost(accountLink.AccessToken, socialPost.SocialId);
            if (result.success == true)
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.FaceBookLikes, new SocialAccount
                {
                    Id = accountLink.SocialAccountId,
                    DisplayName = account.Profile.DisplayName
                });
                socialPost.Like.FaceBookLikes.Add(new SocialAccount
                {
                    Id = accountLink.SocialAccountId,
                    DisplayName = account.Profile.DisplayName
                });
            }
            else
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.RegitLikes,
                    new ClassObjectId { Id = account.Id });
                socialPost.Like.RegitLikes.Add(new ClassObjectId { Id = account.Id });
            }

            return _socialPostCollection.UpdateOne(filter, update);
        }

        public UpdateResult UpdateLikeAsAccountPage(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, SocialPage accountPage)
        {
            UpdateDefinition<SocialPost> update;
            var result = _facebookCollector.LikesAPost(accountPage.AccessToken, socialPost.SocialId);
            if (result.success == true)
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.FaceBookLikes, new SocialAccount
                {
                    Id = accountPage.Id,
                    DisplayName = accountPage.PageName
                });
                socialPost.Like.FaceBookLikes.Add(new SocialAccount
                {
                    Id = accountPage.Id,
                    DisplayName = accountPage.PageName
                });
            }
            else
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.RegitLikes,
                    new ClassObjectId { Id = account.Id });
                socialPost.Like.RegitLikes.Add(new ClassObjectId { Id = account.Id });
            }

            return _socialPostCollection.UpdateOne(filter, update);
        }

        public SocialPost UpdateRegitLike(ObjectId accountId, SocialPost socialPost)
        {
            var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, socialPost.Id);
            UpdateDefinition<SocialPost> update;
            var oldAccountId = socialPost.Like.RegitLikes.FirstOrDefault(t => t.Id == accountId);
            if (oldAccountId == null) //not like before
            {
                update = Builders<SocialPost>.Update.AddToSet(t => t.Like.RegitLikes, new ClassObjectId { Id = accountId });
                socialPost.Like.RegitLikes.Add(new ClassObjectId { Id = accountId });
            }
            else //liked
            {
                socialPost.Like.RegitLikes.Remove(oldAccountId);
                update = Builders<SocialPost>.Update.PullFilter(t => t.Like.RegitLikes, s => s.Id == oldAccountId.Id);
            }
            _socialPostCollection.UpdateOne(filter, update);
            return socialPost;
        }

        public UpdateResult LikeOnSocial(SocialPost socialPost, Account account)
        {
            var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, socialPost.Id);

            switch (socialPost.Type)
            {
                case SocialType.Facebook:
                    {
                        if (account.AccountType == AccountType.Business)
                        {
                            var accountPage = account.Pages.FirstOrDefault();
                            if (accountPage != null)
                            {
                                return UpdateLikeAsAccountPage(filter, socialPost, account, accountPage);
                            }
                        }

                        var accountLink = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                        return accountLink != null
                            ? UpdateLikeAsAccountFacebook(filter, socialPost, account, accountLink)
                            : UpdateLikeAsAccountRegit(filter, socialPost, account);
                    }
                case SocialType.Twitter:
                    {
                        var accountLink = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                        return accountLink != null
                            ? UpdateLikeAsAccountTwitter(filter, socialPost, account, accountLink)
                            : UpdateLikeAsAccountRegit(filter, socialPost, account);
                    }
                default:
                    return UpdateLikeAsAccountRegit(filter, socialPost, account);
            }
        }

        public UpdateResult UnLikeOnSocial(SocialPost socialPost, Account account)
        {
            var filter = Builders<SocialPost>.Filter.Eq(t => t.Id, socialPost.Id);

            switch (socialPost.Type)
            {
                case SocialType.Facebook:
                    {
                        if (account.AccountType == AccountType.Business)
                        {
                            var accountPage = account.Pages.FirstOrDefault();
                            if (accountPage != null)
                            {
                                return UnLikeAsAccountPages(filter, socialPost, account, accountPage);
                            }
                        }

                        var accountLink = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                        return accountLink != null
                            ? UnLikeAsAccountFacebook(filter, socialPost, account, accountLink)
                            : UnLikeAsAccountRegit(filter, socialPost, account);
                    }
                case SocialType.Twitter:
                    {
                        var accountLink = account.AccountLinks.FirstOrDefault(x => x.Type == SocialType.Facebook);
                        return accountLink != null
                            ? UnLikeAsAccountTwitter(filter, socialPost, account, accountLink)
                            : UnLikeAsAccountRegit(filter, socialPost, account);
                    }
                default:
                    return UnLikeAsAccountRegit(filter, socialPost, account);
            }
        }
    }
}