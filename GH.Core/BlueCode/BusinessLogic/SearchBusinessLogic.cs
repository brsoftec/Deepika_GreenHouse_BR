using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Common;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using GH.Core.Services;
using GH.Core.Models;
using System.Text.RegularExpressions;
using GH.Core.BlueCode.Entity.Search;
using GH.Core.BlueCode.Entity.ProfilePrivacy;
using System;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class SearchBusinessLogic
    {
        private MongoRepository<ProfilePrivacy> _privacyRepository;

        //user
        public DataList<UserSearchResult> SearchMainUserUser(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, string userid = "", string useraccountid = "")
        {
            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();

            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;

            var criteria =
                ((filter.Regex("Profile.DisplayName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.FirstName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.LastName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.Description",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 ) & filter.Eq("AccountType", 0))
                | (filter.Regex("Profile.Email",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 0) & !filter.AnyIn("AccountId", listuserEmail))
                | (filter.Regex("Profile.Phone",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 0) & !filter.AnyIn("AccountId", listuserPhone)
                );

            var projection = Builders<BsonDocument>.Projection.Include("AccountId");

            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("Profile.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.Status");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            var listuser = userCollection.Find(criteria).Project(projection);
            var totalItems = (int) listuser.Count();

            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();
            var ids = listpaging.Select(y => y["_id"].ToString()).ToList();
            var userids = listpaging.Select(y => y["AccountId"].ToString()).ToList();

            var NetworkCollection = MongoDBConnection.Database.GetCollection<Network>("Network");
            var filternetwork = Builders<Network>.Filter;
            var filterusercurrentnetwork = filternetwork.Where(x => (x.NetworkOwner.Equals(ObjectId.Parse(userid))));
            filterusercurrentnetwork = filterusercurrentnetwork &
                                       filternetwork.ElemMatch(x => x.Friends, g => userids.Contains(g.UserId));
            filterusercurrentnetwork = filterusercurrentnetwork |
                                       (filternetwork.In("NetworkOwner", ids.Select(x => ObjectId.Parse(x)).ToList()) &
                                        filternetwork.ElemMatch(x => x.Friends, g => g.UserId == useraccountid));

            var listusercurrentnetworks = NetworkCollection.Find(filterusercurrentnetwork).ToList();
            var FriendCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("FriendInvitation");
            var filterinvitefriend = Builders<BsonDocument>.Filter;
            var filterusercurrentinvitefriend = filterinvitefriend.Eq("From", ObjectId.Parse(userid));
            filterusercurrentinvitefriend =
                filterusercurrentinvitefriend &
                filterinvitefriend.In("To", ids.Select(x => ObjectId.Parse(x)).ToList()) |
                (filterinvitefriend.Eq("To", ObjectId.Parse(userid)) &
                 filterinvitefriend.Eq("From", ids.Select(x => ObjectId.Parse(x)).ToList()));
            var listusercurrentinvitedfriends = FriendCollection.Find(filterusercurrentinvitefriend).ToList();
            var listuserresult = new List<UserSearchResult>();

            ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;
                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["Profile"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.Status = BsonHelper.GetvaluestringFromObject(user["Profile"]["Status"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);
                    var rs = new ProfilePrivacy();
                    rs = _profiePrivacy.GetProfilePrivacyByAccountId(userresult.UserAcccountid);
                    if (rs != null)
                    {
                        foreach (var field in rs.ListField)
                        {
                            if (field.Field == "PhotoUrl" && field.Role != "public")
                                userresult.PhotoUrl = "";

                            if (field.Field == "Email" && field.Role != "public")
                                userresult.Email = "";


                            if (field.Field == "Profile" && field.Role != "public")
                            {
                                userresult.PhotoUrl = "";
                                userresult.Email = "";
                            }
                        }
                    }
                    //Phone
                    var network = listusercurrentnetworks.Where(x =>
                        (x.NetworkOwner.ToString() == userresult.Userid &&
                         x.Friends.Any(y => y.UserId == useraccountid))
                        ||
                        x.NetworkOwner.ToString() == userid && x.Friends.Any(y => y.UserId == userresult.UserAcccountid)
                    ).FirstOrDefault();

                    if (network != null)
                    {
                        userresult.StatusFriend = network.Code.ToLower();
                    }
                    else
                    {
                        var listinvitedfriend = listusercurrentinvitedfriends.Where(
                            x => (x["From"].AsObjectId.Equals(ObjectId.Parse(userid)) &&
                                  x["To"].AsObjectId.ToString().Equals(userresult.Userid))
                                 ||
                                 (x["To"].AsObjectId.Equals(ObjectId.Parse(userid)) &&
                                  x["From"].AsObjectId.ToString().Equals(userresult.Userid))
                        );
                        if (listinvitedfriend.Count() > 0)
                        {
                            userresult.StatusFriend = "pending";
                        }
                    }
                    listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            //filter.Where()
            return dataList;
        }

        //User search Bus Vu
        public DataList<UserSearchResult> SearchMainUserBus(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, string userid = "", string useraccountid = "")
        {
            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;

            // Vu
            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            //


            var criteria =
                ((filter.Regex("Profile.DisplayName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.FirstName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.LastName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.Description",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 ) & filter.Eq("AccountType", 1))
                | (filter.Regex("CompanyDetails.Email",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 1) & !filter.AnyIn("AccountId", listuserEmail))
                | (filter.Regex("CompanyDetails.Phone",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 1) & !filter.AnyIn("AccountId", listuserPhone)
                );


            //filter.
            var projection = Builders<BsonDocument>.Projection.Include("AccountId");

            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("CompanyDetails.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            var listuser = userCollection.Find(criteria).Project(projection);
            var totalItems = (int) listuser.Count();
            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();
            var ids = listpaging.Select(y => y["_id"].ToString()).ToList();
            var userids = listpaging.Select(y => y["AccountId"].ToString()).ToList();

            var listuserbusmembers = new AccountService().GetByAccountId(useraccountid).Followees;

            var listuserresult = new List<UserSearchResult>();
            ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;

                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["CompanyDetails"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);

                    var rs = new ProfilePrivacy();
                    rs = _profiePrivacy.GetProfilePrivacyByAccountId(userresult.UserAcccountid);
                    if (rs != null)
                    {
                        foreach (var field in rs.ListField)
                        {
                            if (field.Field == "PhotoUrl" && field.Role != "public")
                                userresult.PhotoUrl = "";

                            if (field.Field == "Email" && field.Role != "public")
                                userresult.Email = "";


                            if (field.Field == "Profile" && field.Role != "public")
                            {
                                userresult.PhotoUrl = "";
                                userresult.Email = "";
                            }
                        }
                    }

                    //Phone
                    var following = new InteractionService().IsFollowing(useraccountid, userresult.UserAcccountid);
//                    if (listuserbusmembers.Any(x => x.AccountId.ToString() == userresult.Userid))
                    if (following)
                        userresult.StatusFriend = "Followed";
                    else
                        userresult.StatusFriend = "";
                    listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            //filter.Where()
            return dataList;
        }

        //Bus
        public DataList<UserSearchResult> SearchMainBusUser(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, string userid = "", string useraccountid = "")
        {
            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();

            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;

            var criteria =
                ((filter.Regex("Profile.DisplayName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.FirstName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.LastName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.Description",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 ) & filter.Eq("AccountType", 0))
                | (filter.Regex("Profile.Email",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 0) & !filter.AnyIn("AccountId", listuserEmail))
                | (filter.Regex("Profile.Phone",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 0) & !filter.AnyIn("AccountId", listuserPhone)
                );

            var projection = Builders<BsonDocument>.Projection.Include("AccountId");
            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("Profile.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.Status");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            projection = projection.Include("Followees");
            var listuser = userCollection.Find(criteria).Project(projection);
            var totalItems = (int) listuser.Count();
            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();
            var ids = listpaging.Select(y => y["_id"].ToString()).ToList();
            var userids = listpaging.Select(y => y["AccountId"].ToString()).ToList();
            var objectiduserid = ObjectId.Parse(userid);

            var listuserresult = new List<UserSearchResult>();

            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;
                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["Profile"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.Status = BsonHelper.GetvaluestringFromObject(user["Profile"]["Status"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);

                    ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
                    var rs = new ProfilePrivacy();
                    rs = _profiePrivacy.GetProfilePrivacyByAccountId(userresult.UserAcccountid);
                    if (rs != null)
                    {
                        foreach (var field in rs.ListField)
                        {
                            if (field.Field == "PhotoUrl" && field.Role != "public")
                                userresult.PhotoUrl = "";
                            if (field.Field == "Email" && field.Role != "public")
                                userresult.Email = "";

                            if (field.Field == "Profile" && field.Role != "public")
                            {
                                userresult.PhotoUrl = "";
                                userresult.Email = "";
                            }
                        }
                    }

                    var followees = user["Followees"].AsBsonArray;
                    if (followees.Count > 0 && followees.Any(x => x["AccountId"].AsObjectId.ToString().Equals(userid)))
                        userresult.StatusFriend = "Followed";
                    else
                        userresult.StatusFriend = "";
                    listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            return dataList;
        }

        public DataList<UserSearchResult> SearchMainBusBus(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, string userid = "", string useraccountid = "")
        {
            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;

            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();

            var criteria =
                ((filter.Regex("Profile.DisplayName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.FirstName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.LastName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.Description",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 ) & filter.Eq("AccountType", 1))
                | (filter.Regex("CompanyDetails.Email",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 1) & !filter.AnyIn("AccountId", listuserEmail))
                | (filter.Regex("CompanyDetails.Phone",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", 1) & !filter.AnyIn("AccountId", listuserPhone)
                );


            var projection = Builders<BsonDocument>.Projection.Include("AccountId");
            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("CompanyDetails.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            projection = projection.Include("Followees");

            var listuser = userCollection.Find(criteria).Project(projection);
            var totalItems = (int) listuser.Count();
            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();


            var listuserresult = new List<UserSearchResult>();

            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;
                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["CompanyDetails"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);

                    try
                    {
                        ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
                        var rs = new ProfilePrivacy();
                        rs = _profiePrivacy.GetProfilePrivacyByAccountId(userresult.UserAcccountid);
                        if (rs != null)
                        {
                            foreach (var field in rs.ListField)
                            {
                                if (field.Field == "PhotoUrl" && field.Role != "public")
                                    userresult.PhotoUrl = "";

                                if (field.Field == "Email" && field.Role != "public")
                                    userresult.Email = "";

                                if (field.Field == "Profile" && field.Role != "public")
                                {
                                    userresult.PhotoUrl = "";
                                    userresult.Email = "";
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            //filter.Where()
            return dataList;
        }

        //public
        public DataList<UserSearchResult> SearchMainPublic(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, bool isbus = false)
        {
            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            int indextypeuser = isbus ? 1 : 0;
            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();

            var criteria =
                ((filter.Regex("Profile.DisplayName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.FirstName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.LastName",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                  | filter.Regex("Profile.Description",
                      BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 ) & filter.Eq("AccountType", indextypeuser) & !filter.Eq("Status", EnumAccount.CloseAccount))
                | (filter.Regex("CompanyDetails.Email",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", indextypeuser) & !filter.AnyIn("AccountId", listuserEmail))
                | (filter.Regex("CompanyDetails.Phone",
                       BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                   & filter.Eq("AccountType", indextypeuser) & !filter.Eq("Status", EnumAccount.CloseAccount) &
                   !filter.AnyIn("AccountId", listuserPhone)
                );

            var projection = Builders<BsonDocument>.Projection.Include("AccountId");

            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("Profile.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            var listuser = userCollection.Find(criteria).Project(projection);
            var totalItems = (int) listuser.Count();
            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();
            var ids = listpaging.Select(y => y["_id"].ToString()).ToList();
            var userids = listpaging.Select(y => y["AccountId"].ToString()).ToList();


            var listuserresult = new List<UserSearchResult>();

            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;
                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["Profile"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);
                    userresult.StatusFriend = "";

                    ProfilePrivacyBusinessLogic _profiePrivacy = new ProfilePrivacyBusinessLogic();
                    var rs = new ProfilePrivacy();
                    rs = _profiePrivacy.GetProfilePrivacyByAccountId(userresult.UserAcccountid);
                    if (rs != null)
                    {
                        foreach (var field in rs.ListField)
                        {
                            if (field.Field == "PhotoUrl" && field.Role != "public")
                                userresult.PhotoUrl = "";

                            if (field.Field == "Email" && field.Role != "public")
                                userresult.Email = "";

                            if (field.Field == "Profile" && field.Role != "public")
                            {
                                userresult.PhotoUrl = "";
                                userresult.Email = "";
                            }
                        }
                    }


                    listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            //filter.Where()
            return dataList;
        }

        public DataList<UserSearchResult> GetBusinessUsers(string keywordsearch = "", int pageindex = 1,
            int pagesize = 10, string userid = "", string useraccountid = "")
        {
            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;
            var listuserbusmembers = new AccountService().GetByAccountId(useraccountid).Followees;
            _privacyRepository = new MongoRepository<ProfilePrivacy>();
            var listuserEmail = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Email" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();
            var listuserPhone = _privacyRepository
                .Many(l => l.ListField.Any(a => a.Field == "Phone" && a.Role == "hidden")).Select(b => b.AccountId)
                .ToList();

            var criteria =
                (filter.Regex("Profile.Country",
                     BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 | filter.Regex("CompanyDetails.Country",
                     BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase))))
                & filter.Eq("AccountType", 1);

            var projection = Builders<BsonDocument>.Projection.Include("AccountId");
            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("CompanyDetails.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");
            var listuser = userCollection.Find(criteria).Project(projection);

            var totalItems = (int) listuser.Count();
            var listpaging = listuser.Skip(pageindex - 1).Limit(pagesize).ToList();
            var ids = listpaging.Select(y => y["_id"].ToString()).ToList();
            var userids = listpaging.Select(y => y["AccountId"].ToString()).ToList();

            var listuserresult = new List<UserSearchResult>();
            foreach (var user in listpaging)
            {
                var userresult = new UserSearchResult();
                var checkAdd = true;
                try
                {
                    userresult.UserAcccountid = user["AccountId"].AsString;
                    userresult.Userid = user["_id"].AsObjectId.ToString();
                    userresult.Email = BsonHelper.GetvaluestringFromObject(user["CompanyDetails"]["Email"]);
                    userresult.Description = BsonHelper.GetvaluestringFromObject(user["Profile"]["Description"]);
                    userresult.PhotoUrl = BsonHelper.GetvaluestringFromObject(user["Profile"]["PhotoUrl"]);
                    userresult.FirstName = BsonHelper.GetvaluestringFromObject(user["Profile"]["FirstName"]);
                    userresult.LastName = BsonHelper.GetvaluestringFromObject(user["Profile"]["LastName"]);
                    userresult.DisplayName = BsonHelper.GetvaluestringFromObject(user["Profile"]["DisplayName"]);
                    if (listuserbusmembers.Any(x => x.AccountId.ToString() == userresult.Userid))
                    {
                        userresult.StatusFriend = "Followed";
                        checkAdd = false;
                    }

                    else
                        userresult.StatusFriend = "";

                    if (!checkAdd)
                        listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            return dataList;
        }

        public DataList<UserSearchResult> GetAdBusisness(string keywordsearch = "", int pageindex = 1, int pagesize = 5,
            string userid = "", string useraccountid = "", List<UserSearchResult> results = null)
        {
            var userCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Account");
            var filter = Builders<BsonDocument>.Filter;


            var listuserbusmembers = new AccountService().GetByAccountId(useraccountid).Followees;

            var criteria =
                (filter.Regex("Profile.Country",
                     BsonRegularExpression.Create(new Regex(keywordsearch, RegexOptions.IgnoreCase)))
                 | filter.Regex("CompanyDetails.Country", BsonRegularExpression.Create(
                     new Regex(keywordsearch, RegexOptions.IgnoreCase))))
                & filter.Eq("AccountType", 1);

            var projection = Builders<BsonDocument>.Projection.Include("AccountId");
            projection = projection.Include("_id");
            projection = projection.Include("AccountId");
            projection = projection.Include("Profile.DisplayName");
            projection = projection.Include("CompanyDetails.Email");
            projection = projection.Include("Profile.Description");
            projection = projection.Include("Profile.PhotoUrl");
            projection = projection.Include("Profile.FirstName");
            projection = projection.Include("Profile.LastName");

            var lstuser = userCollection.Find(criteria).Project(projection);

            var listuser = lstuser.ToList();

            var totalUser = new List<BsonDocument>();

            foreach (var user in listuser)
            {
                /*if (!listuserbusmembers.Exists(value =>
                    value.AccountId.ToString() == user["_id"].AsObjectId.ToString()))
                    totalUser.Add(user);               
                */
                if (!new InteractionService().IsFollowing(useraccountid,user["AccountId"].AsString))
                    totalUser.Add(user);
            }

            var totalItems = totalUser.Count();

            var listuserresult = new List<UserSearchResult>();
            if (results.Count > 0)
                listuserresult.AddRange(results);

            if (pagesize > totalItems)
                pagesize = totalItems;

            Random rand = new Random();
            for (int i = results.Count; i < pagesize; i++)
            {
                int curValue = rand.Next(0, totalItems);
                var userresult = new UserSearchResult();
                var userAcccountid = totalUser[curValue]["AccountId"].ToString();
                while (listuserresult.Exists(value => value.UserAcccountid == userAcccountid))
                {
                    curValue = rand.Next(0, totalItems);
                    userAcccountid = totalUser[curValue]["AccountId"].ToString();
                }

                try
                {
                    userresult.UserAcccountid = totalUser[curValue]["AccountId"].AsString;
                    userresult.Userid = totalUser[curValue]["_id"].AsObjectId.ToString();
                    userresult.Email =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["CompanyDetails"]["Email"]);
                    userresult.Description =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["Profile"]["Description"]);
                    userresult.PhotoUrl =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["Profile"]["PhotoUrl"]);
                    userresult.FirstName =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["Profile"]["FirstName"]);
                    userresult.LastName =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["Profile"]["LastName"]);
                    userresult.DisplayName =
                        BsonHelper.GetvaluestringFromObject(totalUser[curValue]["Profile"]["DisplayName"]);
//                    if (listuserbusmembers.Any(x => x.AccountId.ToString() == userresult.Userid))
//                        userresult.StatusFriend = "Followed";
                    //else
                        userresult.StatusFriend = "";
                    if (!string.IsNullOrEmpty(userresult.UserAcccountid))
                        listuserresult.Add(userresult);
                }
                catch
                {
                }
            }
            var dataList = new DataList<UserSearchResult>(listuserresult.ToList(), totalItems, pageindex, pagesize);
            return dataList;
        }
    }
}