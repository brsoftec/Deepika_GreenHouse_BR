using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace GH.Core.Services
{
    public interface IAccountService
    {
        SearchUserResult Search(SearchUserCriteria criteria);
        List<Account> GetListALlUserEmailVerified();
        Account Insert(Account account, string invitedDelegationId=null);
        //void Update(string accountId, UpdateDefinition<Account> builder);

        /// <summary>
        /// Get all account whose linked social network
        /// </summary>
        /// <returns></returns>
        List<Account> GetAllAccountLinkedSocialNetwork();
        long CountMembersInBusiness(ObjectId userid);
        Account GetById(ObjectId id);
        Account GetByEmail(string email);
        Account GetByAccountId(string accountId);
        List<Account> GetByListId(List<ObjectId> ids);
        List<Account> GetByListSocialId(List<string> ids);
        List<Account> GetBusinessAccountsLinkWithPersonalAccount(ObjectId personalAccountId);
        void UpdateStatus(Account account, string status);
        void UpdateProfileProperty(Account account, string name, dynamic value);
        void UpdateViewPreference(Account account, string name, dynamic value);
        void UpdateViewPreferences(Account account, AccountViewPreferences prefs);
        Account UpdateProfile(Account account, MultipartFileData profilePicture);
        Account UpdatePrivacies(AccountPrivacies privacies, string accountId);
        Account UpdateDelegation(List<DelegationItemTemplate> delegations, string accountId);
        Account UpdateAccountActivityLogSettings(AccountActivityLogSettings actLogSettings, string accountId);
        Account UpdateSecurityQuestions(AnswerSecurityQuestionModel questions, string accountId);
        Account UpdatePhoneNumber(string phoneNumber, string accountId);
        Account UpdatePinCode(string pinCode, string accountId);
        string GeneratePhoneVerifiedToken(string accountId);
        bool CheckDuplicateEmail(string email, string ignoreAccountId);
        bool CheckDuplicateEmail(string email, ObjectId ignoreAccountId);
        bool HasAccountLinkWithSocial(SocialType social, string socialId);
        bool HasPersonalAccountLinkWithSocial(SocialType social, string socialId);
        void LinkAccount(AccountLink accountLink, ObjectId accountId);
        void UnlinkAccount(ObjectId accountId, SocialType network);
        void ConnectSocialPage(SocialPage page, ObjectId accountId);
        void DisconnectSocialPage(ObjectId accountId, SocialType network);
        List<Account> GetMembersInBusiness(ObjectId businessAccountId, int? start, int? length, out long total);
        void UpdateMemberRoleInBusiness(ObjectId businessAccountId, ObjectId memberAccountId, List<ObjectId> roleIds, ObjectId modifierAccountId);
        void UpdateMember(ObjectId businessAccountId, ObjectId memberAccountId, List<string> roles);
        void RemoveMemberFromBusiness(ObjectId businessAccountId, ObjectId memberAccountId, ObjectId removerAccountId);
        List<Account> GetUsersModifiedAfter(DateTime fromTime);
        Dictionary<ObjectId, List<ObjectId>> GetAccountIdsRelatedToUsers(params ObjectId[] userId);
        Account UpdateBusinessAccountPictureAlbum(ObjectId businessAccountId, List<MultipartFileData> newPhotos, List<string> deletingPhotos);
        void UpdateWorkTime(ObjectId businessAccountId, DateTime? workHourFrom, DateTime? workHourTo, List<string> workdays);
        Account FollowBusiness(ObjectId followerId, ObjectId businessId, DateTime? datefollow = null);
        Account UnfollowBusiness(ObjectId unfollowerId, ObjectId businessId);

        void SetSMSAuthenticated(string userId);
        bool IsSMSAuthenticated(string userId);
        void ClearSession(string userId);

        IQueryable<Account> Search(Expression<Func<Account, bool>> expression);
        Account UpdateLastestNotification(LatestNotification latestNotification, string accountId);

        Account UpdateEmail(string email, ObjectId account, GreenHouseDbContext sqlDb);
        void UpdateOtp(string value, ObjectId account);
        void UpdateWebsite(string website, ObjectId account);

        void UpdateResetPasswordToken(ObjectId account, ObjectId? verifyTokenId);

        SearchUserResult SearchUsers(SearchUserParameter searchModel);
        FolloweeResult GetFollowByListUserId(FolloweeParameter parameter);
        Account UpdateNotificationSettings(AccountNotificationSettings settings, string accountId);
        Account UpdateBusinessPrivacy(BusinessPrivacy privacy, string accountId);

        void UpdateLanguage(ObjectId accountId, string languageCode);

        VerifyEmailViewModel SendVerifyEmail(Account account, string link);
        VerifyEmailViewModel VerifyEmail(string email, string code);

        IEnumerable<Account> FindUsersByKeyword(string keyword);
        Task<UpdateResult> ManualVerifyAccount(IEnumerable<ObjectId> accountIds);
        string GetCompanyDetailsByAccountId(string accountId);
        void UpdateCompanyDetailsByAccountId(string accountId, string companyDetails);
        string GetProfileByAccountId(string accountId);
        void UpdateProfileByAccountId(string accountId, string profileString);
        Account UpdatePictureProfileByAccountId(string accountId, string profilePicture);
        bool UpdatePIN(string pin, Account account);
        bool CloseAccount(Account account);
        bool UnCloseAccount(Account account);
        List<Account> GetListUser(int start = 0, int take = 10, DateTime? startDate = null, DateTime? endDate = null);
        string GetRoleById(ObjectId? id);
        Role GetRoleByName(string name);
        List<UserRole> GetUserRolePending(List<JoiningBusinessInvitation> lstRolesPending);
        void FollowRegit(string userId);
        bool IsConfirmSMS(string userId);

        Task<FuncResult> SetProfile(string name, object value, string accountId);
        Task<FuncResult> SetAccountStatusAsync(string key, object value, string accountId);

        Task<FuncResult> SetSecurityQuestionsAsync(IList<AccessSecurityAnswer> sqa, Account account);

        Task<FuncResult> CloseAccount(string accountId);
        Task<FuncResult> ReopenAccount(string accountId);

    }
}
