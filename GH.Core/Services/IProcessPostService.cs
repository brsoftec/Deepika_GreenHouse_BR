using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GH.Core.Services
{
    public interface IProcessPostService
    {
        UpdateResult UnLikeAsAccountTwitter(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink);

        UpdateResult UnLikeAsAccountRegit(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account);

        UpdateResult UnLikeAsAccountPages(FilterDefinition<SocialPost> filter, SocialPost socialPost, Account account,
            SocialPage accountPage);

        UpdateResult UnLikeAsAccountFacebook(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink);

        UpdateResult UpdateLikeAsAccountRegit(FilterDefinition<SocialPost> filter, SocialPost socialPost, Account account);

        UpdateResult UpdateLikeAsAccountTwitter(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink);

        UpdateResult UpdateLikeAsAccountFacebook(FilterDefinition<SocialPost> filter, SocialPost socialPost,
            Account account, AccountLink accountLink);

        UpdateResult UpdateLikeAsAccountPage(FilterDefinition<SocialPost> filter, SocialPost socialPost, Account account,
            SocialPage accountPage);
        
        SocialPost UpdateRegitLike(ObjectId accountId, SocialPost socialPost);

        UpdateResult LikeOnSocial(SocialPost socialPost, Account account);

        UpdateResult UnLikeOnSocial(SocialPost socialPost, Account account);
    }
}
