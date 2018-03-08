using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Notification;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.Models
{
    public class Account
    {
        public Account()
        {
            AccountLinks = new List<AccountLink>();
            BusinessAccountRoles = new List<BusinessAccountRole>();
            PictureAlbum = new List<string>();
            Followers = new List<Follow>();
            Followees = new List<Follow>();

            Language = "en-US";
        }

        [BsonId]
        public ObjectId Id { get; set; }

        public string AccountId { get; set; }
        public string SocialAccountId { get; set; }
        public bool AccountVerified { get; set; }
        public bool EmailVerified { get; set; }

        public string EmailVerifyToken { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? EmailVerifyTokenDate { get; set; }

        public bool PhoneNumberVerified { get; set; }
        public string PhoneVerifiedToken { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? PhoneVerifiedTokenExpired { get; set; }
        
        [BsonElementAttribute("Status")]
        public string Status { get; set; }

        [BsonElementAttribute("InviteId")]
        public string InviteId { get; set; }

        [BsonElementAttribute("Profile")]
        public Profile Profile { get; set; }

        [BsonElementAttribute("AccountLink")]
        public List<AccountLink> AccountLinks { get; set; }

        [BsonElementAttribute("AccountPrivacies")]
        public AccountPrivacies AccountPrivacies { get; set; }

        [BsonElementAttribute("AccountActivityLogSettings")]
        public AccountActivityLogSettings AccountActivityLogSettings { get; set; }

        [BsonElementAttribute("SecurityQuesion1")]
        public SecurityQuestionAnswer SecurityQuesion1 { get; set; }

        [BsonElementAttribute("SecurityQuesion2")]
        public SecurityQuestionAnswer SecurityQuesion2 { get; set; }

        [BsonElementAttribute("SecurityQuesion3")]
        public SecurityQuestionAnswer SecurityQuesion3 { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime CreatedDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime ModifiedDate { get; set; }

        public CompanyDetails CompanyDetails { get; set; }

        public AccountType AccountType { get; set; }
        public List<BusinessAccountRole> BusinessAccountRoles { get; set; }

        public List<SocialPage> Pages { get; set; }

        public ObjectId Creator { get; set; }
        public IEnumerable<DelegationItemTemplate> Delegations { get; set; }
        public LatestNotification LastestNotification { get; set; }

        public List<string> PictureAlbum { get; set; }

        public List<Follow> Followers { get; set; }
        public List<Follow> Followees { get; set; }

        public ObjectId? ResetPasswordToken { get; set; }

        public AccountNotificationSettings NotificationSettings { get; set; }
        public BusinessPrivacy BusinessPrivacies { get; set; }

        public string Language { get; set; }
        public bool BusinessAccountVerified { get; set; }

        [BsonElementAttribute("AccountStatus")]
        public AccountStatus AccountStatus { get; set; }

        [BsonElementAttribute("ViewPreferences")]
        public AccountViewPreferences ViewPreferences { get; set; }
    }

    public class CompanyDetails
    {
        public static readonly string[] AllWorkdays = {"MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"};
        public string Industry { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Email { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? WorkHourFrom { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? WorkHourTo { get; set; }

        public List<string> Workdays { get; set; }
        public BsonDocument WorkTime { get; set; }
    }

    public class SocialPage
    {
        public string Id { get; set; }
        public string PageName { get; set; }
        public SocialType SocialType { get; set; }
        public string AccessToken { get; set; }
    }

    public enum AccountType
    {
        Personal,
        Business
    }


    public class AccountActivityLogSettings
    {
        public bool RecordAccess { get; set; }
        public bool RecordProfile { get; set; }
        public bool RecordAccount { get; set; }
        public bool RecordNetwork { get; set; }
        public bool RecordVault { get; set; }
        public bool RecordDelegation { get; set; }
        public bool RecordInteraction { get; set; }
        public bool RecordSocialActivity { get; set; }
        public bool RecordProfileBusiness { get; set; }
        public bool RecordAccountBusiness { get; set; }
        public bool RecordBusinessSystem { get; set; }
        public bool RecordCampaign { get; set; }
        public bool RecordWorkflow { get; set; }
    }

    public class AccountPrivacies
    {
        public bool FindMe { get; set; }
        public bool ShareMyActivity { get; set; }
        public bool ViewMyProfile { get; set; }
        public bool NotFollowBusinessSendMeAds { get; set; }
        public SendMeMessagePrivacy SendMeMessage { get; set; }
        public bool AutoDeleteMessage { get; set; }
    }

//    public class UserViewPreference
//    {
//        public string Name { get; set; }
//        public string Value { get; set; }
//    }

    [BsonIgnoreExtraElements]
    public class AccountStatus
    {
        public bool LocationDetected { get; set; }
        public bool NoSecurityQuestions { get; set; }
        public bool ToConvertInvite { get; set; }
        public bool NoAvatar { get; set; }
    }

    public class ProfilePropertyViewModel
    {
        public string PropName;
        public dynamic PropValue;
    }

    public class AccountPreferences
    {
        public List<Dictionary<string, string>> ViewPreferences;
    }

    public class AccountViewPreferences
    {
        public bool ShowIntroSlides;
        public bool ShowBusinessIntroSlides;
        public bool ShowIntroVault;
        public bool ShowIntroBusiness;
    }


    public class ViewPreferenceViewModel
    {
        public string PrefName;
        public dynamic PrefValue;
    }


    public enum SendMeMessagePrivacy
    {
        All,
        Network,
        Business
    }

    [BsonIgnoreExtraElements]
    public class Profile
    {
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PinCode { get; set; }
        public string PhotoUrl { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string ZipPostalCode { get; set; }
        public string Description { get; set; }
        public string Gender { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? Birthdate { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class EmbeddedProfile
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
    }

    public class AccountLink
    {
        public SocialType Type { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string SocialAccountId { get; set; }
        public string TwitterName { get; set; }
    }

    public class Follow
    {
        public ObjectId AccountId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime Time { get; set; }
    }

    public enum AccountPrivacy
    {
        Public,
        Private,
        Hidden
    }

    public class BusinessPrivacy
    {
        public AccountPrivacy Privacy { get; set; }
        public bool AllowComment { get; set; }
    }

    public class AccountNotificationSettings
    {
        public bool Interactions { get; set; }
        public bool EventAndReminders { get; set; }
        public bool NetworkRequest { get; set; }
        public bool Workflow { get; set; }
    }
}