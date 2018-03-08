using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.BlueCode.Entity.Notification;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class ProfileBusinessLogic : IProfileBusinessLogic
    {

        IProfileRepository profileRepository;
        public ProfileBusinessLogic(IProfileRepository profileRepository)
        {
            this.profileRepository = profileRepository;
        }

        public ProfileBusinessLogic()
        {
            this.profileRepository = new ProfileRepository();
        }

        public void Add(UserProfile entity, string invitedDelegationId = null)
        {           
            var collectionVaultInformation = profileRepository.GetCollection("InformationVault");

            var informationVaultTemplate = GetInformationVaultTemplate();
            if(informationVaultTemplate.Contains("_id"))
            {
                informationVaultTemplate.Remove("_id");
            }

            String json = informationVaultTemplate.ToJson();
            var document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            collectionVaultInformation.InsertOne(document);

            var viid = document.GetElement("_id");
            entity.VaultInformationId = viid.Value.ToString();
            entity.Delegations = new List<DelegationItemTemplate>();
            entity.Friends = new List<Friend>();
            entity.Id = ObjectId.GenerateNewId();
            profileRepository.Add(entity);

            //Update delegation if invited
            if(!string.IsNullOrEmpty(invitedDelegationId))
            {
                //Update delegation for delegatOR
                var fromUserProfile = profileRepository.Single(p => p.Delegations != null && p.Delegations.Any(d => d.DelegationId.Equals(invitedDelegationId)));
                DelegationItemTemplate delegation = null;
                if (fromUserProfile!=null)
                {
                    delegation = fromUserProfile.Delegations.FirstOrDefault(d => d.DelegationId.Equals(invitedDelegationId));
                    delegation.ToAccountId = entity.Id.ToString();
                    delegation.ToUserDisplayName = string.Format("{0} {1}",entity.FirstName, entity.LastName);
                    Update(fromUserProfile);

                    //Add delegation for delegatEE
                    var toProfile = GetProfileFromId(entity.Id.ToString());
                    if(toProfile!=null)
                    {
                        List<DelegationItemTemplate> delegations = null;
                        if(toProfile.Delegations!= null)
                        {
                            delegations = toProfile.Delegations.ToList();
                            delegations.Add(delegation);
                            delegation.Direction = EnumDelegationDirection.DelegationIn;
                            toProfile.Delegations = delegations;
                            Update(toProfile);
                        }
                    }
                }

                //Send notification
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.DelegationRequest;
                notificationMessage.FromAccountId = fromUserProfile.Id.ToString();
                notificationMessage.FromUserDisplayName = string.Format("{0} {1}", fromUserProfile.FirstName, fromUserProfile.LastName);
                notificationMessage.ToAccountId = entity.Id.ToString();
                notificationMessage.ToUserDisplayName = string.Format("{0} {1}", entity.FirstName, entity.LastName); ;
                notificationMessage.Content = delegation.Message;
                notificationMessage.PreserveBag = delegation.DelegationId;
                //var notificationBus = new NotificationBusinessLogic();
                //notificationBus.SendNotification(notificationMessage);
            }
        }

        public void Update(UserProfile profile)
        {
            profileRepository.Update(profile);
        }

        private BsonDocument GetInformationVaultTemplate()
        {
            var settings = profileRepository.GetCollection("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "informationVault");
            var informationVaultTemplate = settings.Find(filter).FirstOrDefault();

            return informationVaultTemplate["value"] as BsonDocument;
        }

        public IList<UserProfile> GetAll()
        {
            return profileRepository.Many(x => true).ToList();
        }
        public IList<UserProfile> GetPagingAll(int page, int pageSize, out int totalrows)
        {
            totalrows = 0;
            totalrows = profileRepository.Many(x => true).Count();
            return profileRepository.Many(x => true, page, pageSize).ToList();
        }
        public UserProfile GetProfileFromId(string id)
        {
            //var acc = new IInfomationVaultRepository();
            var userProfile = new UserProfile();
            userProfile = profileRepository.Single(x => x.Id == ObjectId.Parse(id));
            return userProfile;
        }
        //public UserProfile GetProfileFromId(string id)
        //{
        //    
        //    return profileRepository.Single(ObjectId.Parse(id));
        //}
        public UserProfile VerifyProfile(string username,string password)
        {
            /*
            var userProfile = profileRepository.Single(x => x.UserName == username && x.Password == password);
            //Add Activity Log
            var activityLogBus = new ActivityLogBusinessLogic();
            var activityLog = new ActivityLog();
            activityLog.Id = ObjectId.GenerateNewId();
            activityLog.DateTime = DateTime.Today.ToString("yyyy-MM-dd-hh-mm-ss");
            activityLog.ActivityType = EnumActivityType.Login;
            activityLog.Title = "You have logged into Regit at " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            activityLog.FromUserId = userProfile == null ? null : userProfile.Id.ToString();
            activityLog.FromUserName = userProfile == null ? string.Format("{0} {1}",userProfile.FirstName,userProfile.LastName) : "";
            activityLogBus.AddActivityLog(activityLog);

            return userProfile != null ? userProfile : null;
            */

            return null;
        }


        public string sendMessagePin(string userid)
        {
            /*
            UserProfile profile = GetProfileFromId(userid);
            string pin = SMSMessageHelp.CreateSMSMessagePin();
            string stringsmsmessagePin = string.Format(ConfigHelp.GetStringValue("SMSmessagePin"), pin);
            string phonenumber = profile.PhoneNumber;
            bool result = true;
            /*todo
               result= resultsend=serviceSendSMS.send(stringsmsmessagePin,phonenumber);
            */
            //return !result? "": pin;

            return "";
            
        }

        //public UserProfile GetProfileByAccountId(string userId)
        //{

        //    var userProfile = new UserProfile();
        //    userProfile = profileRepository.Single(x => x.AccountId== userId);

           
        //    return userProfile;
        //}

    }
}


