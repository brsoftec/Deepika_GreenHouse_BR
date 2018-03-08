using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ProfilePrivacy;
using GH.Core.Exceptions;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class ProfilePrivacyBusinessLogic : IProfilePrivacyBusinessLogic
    {

        private IAccountService _accountService;
        private MongoRepository<ProfilePrivacy> _repository;
    
        public ProfilePrivacyBusinessLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<ProfilePrivacy>();
        }

        public string InsertProfilePrivacy(ProfilePrivacy profilePrivacy)
        {
            profilePrivacy.Id = ObjectId.GenerateNewId();
            try
            {
                if (!string.IsNullOrEmpty(profilePrivacy.AccountId))
                    _repository.Add(profilePrivacy);

              
            }
            catch (Exception ex)
            {
               
            }
            return profilePrivacy.Id.ToString();
        }
        public Privacy InsertFieldPrivacy(string accountId, Privacy field)
        {
            var rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
            if(rs == null)
            {
                var profilePrivacy = new ProfilePrivacy();
                profilePrivacy.AccountId = accountId;
                var fields = new List<Privacy>();
                fields.Add(field);
                InsertProfilePrivacy(profilePrivacy);
            }
            else
            {
                var fields = rs.ListField;
                fields.Add(field);
                rs.ListField = fields;
                UpdateProfilePrivacy(rs);
            }

            return field;
        }
        public ProfilePrivacy GetProfilePrivacyById(string id)
        {
          
                var Id = new MongoDB.Bson.ObjectId(id);
                var filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                return filterQuery;
           
        }
        public ProfilePrivacy GetProfilePrivacyByAccountId(string accountId)
        {
            var rs = new ProfilePrivacy();
            try
            {
                    rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
                if(rs == null)
                {
                    var profilePr = new ProfilePrivacy();
                    var  listField = new List<Privacy>();
                    profilePr.AccountId = accountId;
                    profilePr.ListField = listField;
                    profilePr.Id = ObjectId.GenerateNewId();
                    _repository.Add(profilePr);
                    return profilePr;
                }
             
             
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
            return rs;
        }
        public Privacy GetRequestHandshakePrivacy(string accountId, string field)
        {
            var rs = new ProfilePrivacy();
            var result = new Privacy();
            rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
            if (rs == null)
            {
                result = InsertFieldPrivacy(accountId, result);
            }
            else
            {
                result = GetRoleFieldPrivacy(accountId, field);
            }
             if(string.IsNullOrEmpty(result.Role))
                result.Role = EnumPrivacyRole.On;

            return result;
        }
        public Privacy GetRoleFieldPrivacy(string accountId, string field)
        {
            var rs = new ProfilePrivacy();
            var result = new Privacy();
          
            result.Field = field;
            rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
            if (rs == null)
            {
                result = InsertFieldPrivacy(accountId, result);
            }
            else
            {
                if(rs.ListField != null)
                {
                    foreach (var item in rs.ListField)
                    {
                        if (item.Field == field)
                            result.Role = item.Role;
                    }
                }
               
                else
                 UpdateFieldPrivacy(accountId, result);

            }
            return result;
        }


        public Privacy UpdateRequestHandshakePrivacy(string accountId, string field, string role = null)
        {
            var rs = new ProfilePrivacy();
            role = role ?? EnumPrivacyRole.On;
            var result = new Privacy();
            result.Role = role;
            result.Field = field;
             rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
            if (rs == null)
            {
                result = InsertFieldPrivacy(accountId, result);
            }
            else
            {
            result = UpdateFieldPrivacy(accountId, result);
            }
          
            return result;
        }

        public Privacy UpdateFieldPrivacy(string accountId, Privacy field)
        {
            var rs = _repository.Many(l => l.AccountId.Equals(accountId)).FirstOrDefault();
            if (rs == null)
            {
                InsertFieldPrivacy(accountId, field);
            }
            else
            {
                var fields = new List<Privacy>();
                if (rs.ListField != null)
                {
                    foreach (var item in rs.ListField)
                    {
                        if (item.Field == field.Field)
                        {
                            item.Role = field.Role;
                        }
                        fields.Add(item);
                    }
                }
                else
                    fields.Add(field);

                rs.ListField = fields;
                UpdateProfilePrivacy(rs);
            }
            return field;
        }
        public string UpdateProfilePrivacy(ProfilePrivacy profilePrivacy)
        {
            var rs = "";

            var profilePrivacyCollection = MongoDBConnection.Database.GetCollection<ProfilePrivacy>(RegitTable.ProfilePrivacy);
            try
            {
                var builder = Builders<ProfilePrivacy>.Filter;
                var filter = builder.Eq(c => c.AccountId, profilePrivacy.AccountId);

                var update = Builders<ProfilePrivacy>.Update.Set(f => f.ListField, profilePrivacy.ListField);
                profilePrivacyCollection.UpdateOne(filter, update);
                rs = profilePrivacy.AccountId.ToString();
            }
            catch { }
            return rs;
        }
        public void DeleteProfilePrivacyById(string id)
        {
            try
            {
                var Id = new MongoDB.Bson.ObjectId(id);
                ProfilePrivacy filterQuery = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                _repository.Delete(filterQuery);
            }
            catch { }
        }

     
    }
}