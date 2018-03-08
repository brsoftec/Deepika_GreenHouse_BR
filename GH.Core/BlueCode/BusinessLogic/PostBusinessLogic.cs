using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Entities = GH.Core.BlueCode.Entity;
using System.Globalization;
using System.Web;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.Services;
using Newtonsoft.Json;
using GH.Util;
using Microsoft.AspNet.Identity;
using RegitSocial.Business.Notification;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class PostBusinessLogic : IPostBusinessLogic
    {
        private readonly MongoRepository<BusinessMember> _businessMemberRepos = new MongoRepository<BusinessMember>();
        private readonly MongoRepository<Post> _postRepos = new MongoRepository<Post>();

        public void RegisterCampaign(Account account, Account businessUserAccount, string capmaignId,
            string campainType,
            string dateadd = "", List<FieldinformationVault> listvaults = null, string delegationId = null,
            string delegateeId = null)
        {
            var fields = listvaults;
            if (listvaults != null)
            {
                foreach (var field in fields)
                {
                    try
                    {
                        if (field.modelarrays == null)
                        {
                            field.modelarrays = "{}";
                        }
                        switch (field.type)
                        {
                            case "history":
                            case "range":
                                var str = JsonConvert.SerializeObject(field.modelarrays);
                                field.modelarraysstr = str;
                                field.modelarrays = null;
                                break;
                            case "doc":
                                str = JsonConvert.SerializeObject(field.modelarrays);
                                field.modelarraysstr = str;
                                field.modelarrays = null;
                                break;
                            case "qa":
                                str = ""; //JsonConvert.SerializeObject(field.modelarrays);
                                field.modelarraysstr = str;
                                field.modelarrays = null;
                                field.options = null;
                                break;
                            case "radio":
                                str = JsonConvert.SerializeObject(field.options);
                                field.modelarraysstr = str;
                                field.options = null;
                                break;
                            case "select":
                                str = JsonConvert.SerializeObject(field.options);
                                field.modelarraysstr = str;
                                field.options = null;
                                break;
                            default:
                                try
                                {
                                    str = JsonConvert.SerializeObject(field.options);
                                    field.modelarraysstr = str;
                                    field.options = null;
                                }
                                catch
                                {
                                }
                                try
                                {
                                    str = JsonConvert.SerializeObject(field.modelarrays);
                                    field.modelarraysstr = str;
                                    field.modelarrays = null;
                                }
                                catch
                                {
                                }
                                break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            var follower = new Follower();
            if (account.AccountType == AccountType.Personal)
            {
                follower = new Follower
                {
                    UserId = account.AccountId,
                    Name = account.Profile.DisplayName,
                    Age = (account.Profile.Birthdate.HasValue)
                        ? (DateTime.Now.Year - account.Profile.Birthdate.Value.Year)
                        : 0,
                    Gender = account.Profile.Gender,
                    CountryName = account.Profile.Country,
                    //CountryId = countryId,
                    CityName = account.Profile.City,
                    //CityId = cityId,
                    //                    FollowedDate = !string.IsNullOrEmpty(dateadd) ? dateadd : DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                    FollowedDate = !string.IsNullOrEmpty(dateadd) ? dateadd : DateTime.Now.ToString("o"),
                    fields = fields
                };
                if (!string.IsNullOrEmpty(delegationId))
                {
                    follower.DelegationId = delegationId;
                    follower.DelegateeId = delegateeId;
                }
            }
            else
            {
                follower = new Follower
                {
                    UserId = account.AccountId,
                    Name = account.Profile.DisplayName,
                    fields = fields
                };
            }
            var postItem = _postRepos.Single(p =>
                p.PostedUserId.Equals(businessUserAccount.AccountId) && p.CampaignId.Equals(capmaignId));
            if (postItem == null)
            {
                var followers = new List<Follower> {follower};
                postItem = new Post
                {
                    Id = ObjectId.GenerateNewId(),
                    PostedUserId = businessUserAccount.AccountId,
                    PostedUserName = businessUserAccount.Profile.DisplayName,
                    CampaignId = capmaignId,
                    PostType = campainType,
                    Followers = followers,
                    Created = DateTime.Now
                };
                _postRepos.Add(postItem);
            }
            else
            {
                List<Follower> members = new List<Follower>();
                if (postItem.Followers == null)
                {
                    members.Add(follower);
                }
                else
                {
                    var list = postItem.Followers.ToList();
                    var existedUser = list.FirstOrDefault(x => x.UserId == account.AccountId);
                    if (existedUser != null)
                    {
                        postItem.Followers.FirstOrDefault(x => x.UserId == account.AccountId).fields = follower.fields;
                        members = (List<Follower>) postItem.Followers;
                    }
                    else
                    {
                        var newList = postItem.Followers.ToList();
                        newList.Add(follower);
                        members = newList;
                    }
                }

                postItem.Followers = members;
                _postRepos.Update(postItem);
                if (delegateeId != null)
                {
                    var delegatee = new AccountService().GetByAccountId(delegateeId);
                    string title = "You registered on behalf of " + account.Profile.DisplayName + "";
                    var notificationMessage = new NotificationMessage();
                    notificationMessage.Id = ObjectId.GenerateNewId();
                    notificationMessage.Type = EnumNotificationType.InteractionParticipateDelegated;
                    notificationMessage.FromAccountId = delegatee.AccountId;
                    notificationMessage.FromUserDisplayName = delegatee.Profile.DisplayName;
                    notificationMessage.ToAccountId = account.AccountId;
                    notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                    notificationMessage.Content = title;
                    notificationMessage.PreserveBag = capmaignId;
                    notificationMessage.Options = delegationId;

                    notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessage);
                }
            }
        }

        public void DeRegisterCampaign(Account account, Account businessUserAccount, string campaignId,
            string delegateeId = null)
        {
            var postItem = _postRepos.Single(p =>
                p.PostedUserId.Equals(businessUserAccount.AccountId) && p.CampaignId.Equals(campaignId));
            if (postItem != null)
            {
                var followers = postItem.Followers.ToList();
                followers.RemoveAll(f => f.UserId.Equals(account.AccountId));
                postItem.Followers = followers;
                _postRepos.Update(postItem);
                if (delegateeId != null)
                {
                    var delegatee = new AccountService().GetByAccountId(delegateeId);
                    string title = "You unregistered on behalf of " + account.Profile.DisplayName + "";
                    var notificationMessage = new NotificationMessage();
                    notificationMessage.Id = ObjectId.GenerateNewId();
                    notificationMessage.Type = EnumNotificationType.InteractionUnparticipate;
                    notificationMessage.FromAccountId = delegatee.AccountId;
                    notificationMessage.FromUserDisplayName = delegatee.Profile.DisplayName;
                    notificationMessage.ToAccountId = account.AccountId;
                    notificationMessage.ToUserDisplayName = account.Profile.DisplayName;
                    notificationMessage.Content = title;
                    notificationMessage.PreserveBag = campaignId;
                    notificationMessage.Options = delegateeId;

                    notificationMessage.DateTime = DateTime.UtcNow.ToString("o");
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessage);
                }
            }
        }

        public DataList<Follower> GetFollowersList(string campaignId, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var camp = new CampaignBusinessLogic();
            var checkRemove = camp.CheckRemoveCampaign(campaignId);
            if (!checkRemove)
            {
                try
                {
                    var post = _postRepos.Single(p => p.CampaignId.Equals(campaignId));
                    if (post != null && post.Followers != null)
                    {
                        var totalItems = post.Followers.Count();
                        var followers = post.Followers.Where(x => !x.Status.Equals("Invite"));
                        var listOfCurrentPage = followers.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                        var dataList = new DataList<Follower>(listOfCurrentPage.ToList(), totalItems, pageIndex,
                            pageSize);
                        return dataList;
                    }
                }
                catch
                {
                }
            }
            return new DataList<Follower>();
        }

        public List<Follower> GetFollowersByCampaignId(string campaignId)
        {
            var rs = new List<Follower>();
            var camp = new CampaignBusinessLogic();
            var checkRemove = camp.CheckRemoveCampaign(campaignId);
            if (!checkRemove)
            {
                var post = _postRepos.Single(p => p.CampaignId.Equals(campaignId));
                if (post != null && post.Followers != null)
                {
                    try
                    {
                        rs = post.Followers.ToList();
                    }
                    catch
                    {
                    }
                }
            }
            return rs;
        }

        public List<DataRegisCampaign> GetDataChartByBusidByTime(string busid, string datestart)
        {
            var dtdatestart = DateTime.ParseExact(datestart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var postCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("BusinessMember");

            var listueridregis = _businessMemberRepos.Many(x => x.BusinessUserId == busid).FirstOrDefault().Members
                .Where(y =>
                    DateTime.Compare(dtdatestart,
                        DateTime.ParseExact(y.FollowedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)) < 0 &&
                    DateTime.Compare(DateTime.Now,
                        DateTime.ParseExact(y.FollowedDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)) >= 0)
                .Select(x => new DataRegisCampaign()
                {
                    datefollow = x.FollowedDate,
                    userid = x.UserId
                }).ToList();

            var listuserid = listueridregis.Select(y => y.userid).ToList();
            var accountCollection = MongoDBConnection.Database.GetCollection<Account>("Account");

            var listueridregisaccount = accountCollection.Find<Account>(x => listuserid.Contains(x.AccountId))
                    .Project<Account>(
                        Builders<Account>.Projection
                            .Include(a => a.AccountId)
                            .Include(a => a.Profile.FirstName)
                            .Include(a => a.Profile.LastName)
                            .Include(a => a.Profile.City)
                            .Include(a => a.Profile.Country)
                            .Include(a => a.Profile.Birthdate)
                            .Include(a => a.Profile.Gender)
                            .Include(a => a.Profile.Email)
                    ).ToList().Select(a => new
                        DataRegisCampaign
                        {
                            dob = a.Profile.Birthdate.HasValue ? a.Profile.Birthdate.Value.ToString("yyyy-MM-dd") : "",
                            userid = a.AccountId,
                            firstname = a.Profile.FirstName,
                            lastname = a.Profile.LastName,
                            gender = a.Profile.Gender,
                            city = a.Profile.City,
                            country = a.Profile.Country,
                            email = a.Profile.Email
                        }
                    ).ToList()
                ;
            listueridregis = (from data1 in listueridregis
                join data2 in listueridregisaccount on data1.userid equals data2.userid
                select new DataRegisCampaign
                {
                    dob = data2.dob,
                    userid = data2.userid,
                    firstname = data2.firstname,
                    lastname = data2.lastname,
                    gender = data2.gender,
                    city = data2.city,
                    country = data2.country,
                    datefollow = data1.datefollow,
                    email = data2.email
                }).ToList();
            var builder = Builders<BsonDocument>.Filter;

            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");

            var filter = builder.AnyIn("userId", listueridregis.Select(x => x.userid));

            var projection = Builders<BsonDocument>.Projection.Include("userId");

            projection = projection.Include("basicInformation.value.firstName.value");
            projection = projection.Include("basicInformation.value.lastName.value");
            projection = projection.Include("basicInformation.value.gender.value");
            projection = projection.Include("basicInformation.value.country.value");
            projection = projection.Include("basicInformation.value.city.value");
            projection = projection.Include("basicInformation.value.dob.value");

            var listvault = vaultCollection.Find(filter).Project(projection).ToList();

            var listueridregisvault = new List<DataRegisCampaign>();
            foreach (var item in listvault)
            {
                listueridregisvault.Add(new DataRegisCampaign
                {
                    firstname_vault =
                        BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["firstName"],
                            "value"),
                    lastname_vault =
                        BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["lastName"],
                            "value"),
                    dob_vault = BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["dob"],
                        "value"),
                    gender_vault =
                        BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["gender"],
                            "value"),
                    country_vault =
                        BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["country"],
                            "value"),
                    city_vault =
                        BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["city"], "value"),
                    userid = item["userId"].AsString,
                    ListFieldsRegis = new List<Entities.InformationVault.FieldinformationVault>()
                });
            }
            listueridregis = (from data1 in listueridregis
                join data2 in listueridregisvault on data1.userid equals data2.userid
                    into a
                from b in a.DefaultIfEmpty()
                select new DataRegisCampaign
                {
                    dob = data1.dob,
                    userid = data1.userid,
                    firstname = data1.firstname,
                    lastname = data1.lastname,
                    gender = data1.gender,
                    city = data1.city,
                    email = data1.email,
                    country = data1.country,
                    datefollow = data1.datefollow,
                    dob_vault = b.dob_vault,
                    firstname_vault = b.firstname_vault,
                    lastname_vault = b.lastname_vault,
                    gender_vault = b.gender_vault,
                    city_vault = b.city_vault,
                    country_vault = b.country_vault,
                    keywords = b.keywords,
                    ListFieldsRegis = b.ListFieldsRegis
                }).ToList();

            return listueridregis;
        }

        public List<DataRegisCampaign> GetDataChartByCampaignByTime(string campaignid, string datestart)
        {
            var postCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Post");
            var listueridregis = new List<DataRegisCampaign>();
            var listFollowed = GetFollowersByCampaignId(campaignid);
            foreach (var follewed in listFollowed)
            {
                if (follewed.Status == "Invite") continue;
                var participatedAt = follewed.ParticipatedAt ?? Convert.ToDateTime(follewed.FollowedDate);
                    
                var userIdRegis = new DataRegisCampaign();
                try
                {
                    var dtdatestart = Convert.ToDateTime(datestart);
                    var fDate = Convert.ToDateTime(follewed.FollowedDate);

                    if (participatedAt >= dtdatestart)
                    {
                        userIdRegis.datefollow = participatedAt.ToString("o");
                        userIdRegis.userid = follewed.UserId;
                        userIdRegis.country = follewed.CountryName;
                        userIdRegis.city = follewed.CityName;
                        userIdRegis.gender = follewed.Gender;
                        userIdRegis.age = follewed.Age;
                        userIdRegis.ListFieldsRegis = follewed.fields.ToList();
                    }
                }
                catch
                {
                }
                if (userIdRegis != null)
                    listueridregis.Add(userIdRegis);
            }


            return listueridregis;


            //var listuserid = listueridregis.Select(y => y.userid).ToList();
            //var accountCollection = MongoDBConnection.Database.GetCollection<Account>("Account");

            //var listueridregisaccount = accountCollection.Find<Account>(x => listuserid.Contains(x.AccountId))
            //        .Project<Account>(
            //            Builders<Account>.Projection
            //                .Include(a => a.AccountId)
            //                .Include(a => a.Profile.FirstName)
            //                .Include(a => a.Profile.LastName)
            //                .Include(a => a.Profile.City)
            //                .Include(a => a.Profile.Country)
            //                .Include(a => a.Profile.Birthdate)
            //                .Include(a => a.Profile.Gender)
            //                .Include(a => a.Profile.Email)
            //        ).ToList().Select(a => new
            //            DataRegisCampaign
            //        {
            //            dob = a.Profile.Birthdate.HasValue ? a.Profile.Birthdate.Value.ToString("yyyy-MM-dd") : "",
            //            userid = a.AccountId,
            //            firstname = a.Profile.FirstName,
            //            lastname = a.Profile.LastName,
            //            gender = a.Profile.Gender,
            //            city = a.Profile.City,
            //            country = a.Profile.Country,
            //            email = a.Profile.Email
            //        }
            //        ).ToList();
            //listueridregis = (from data1 in listueridregis
            //                  join data2 in listueridregisaccount on data1.userid equals data2.userid
            //                  select new DataRegisCampaign
            //                  {
            //                      dob = data2.dob,
            //                      userid = data2.userid,
            //                      firstname = data2.firstname,
            //                      lastname = data2.lastname,
            //                      gender = data2.gender,
            //                      city = data2.city,
            //                      country = data2.country,
            //                      datefollow = data1.datefollow,
            //                      email = data2.email,
            //                      ListFieldsRegis = data1.ListFieldsRegis
            //                  }).ToList();
            //var builder = Builders<BsonDocument>.Filter;
            //var campaigntCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            //var filtercampaign = builder.Eq("_id", campaignid);
            //var campaign = campaigntCollection.Find(filtercampaign).FirstOrDefault();

            //var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");

            //var filter = builder.AnyIn("userId", listueridregis.Select(x => x.userid));

            //var projection = Builders<BsonDocument>.Projection.Include("userId");

            //projection = projection.Include("basicInformation.value.firstName.value");
            //projection = projection.Include("basicInformation.value.lastName.value");
            //projection = projection.Include("basicInformation.value.gender.value");
            //projection = projection.Include("basicInformation.value.country.value");
            //projection = projection.Include("basicInformation.value.city.value");
            //projection = projection.Include("basicInformation.value.dob.value");

            //projection = projection.Include("others.value.preference.value.interests.value");
            //projection = projection.Include("others.value.preference.value.food.value");
            //projection = projection.Include("others.value.preference.value.seat.value");
            //projection = projection.Include("others.value.preference.value.season.value");

            //projection = projection.Include("others.value.favourite.value.colour.value");
            //projection = projection.Include("others.value.favourite.value.music_type.value");
            //projection = projection.Include("others.value.favourite.value.holiday.value");
            //projection = projection.Include("others.value.favourite.value.movie.value");
            //projection = projection.Include("others.value.favourite.value.song.value");
            //projection = projection.Include("others.value.favourite.value.tv_show.value");

            //BsonArray fieldregis = new BsonArray();
            //if (!(campaign["campaign"]["type"].AsString == "Advertising"))
            //{
            //    fieldregis = campaign["campaign"]["fields"].AsBsonArray;
            //    var listmemberhsipfields = fieldregis.Where(x => x["jsPath"].AsString.StartsWith(".membership"));
            //    if (listmemberhsipfields.Count() > 0)
            //    {
            //        projection = projection.Include("membership");
            //    }
            //    var listaddressfields = fieldregis.Where(x => x["jsPath"].AsString.StartsWith(".address"));
            //    if (listaddressfields.Count() > 0)
            //    {
            //        projection = projection.Include("groupAddress");
            //    }
            //    var listgovernmentIDfields = fieldregis.Where(x => x["jsPath"].AsString.StartsWith(".governmentID"));
            //    projection = projection.Include("document");
            //    if (listgovernmentIDfields.Count() > 0)
            //    {
            //        projection = projection.Include("groupGovernmentID");
            //    }
            //    var listemploymentfields = fieldregis.Where(x => x["jsPath"].AsString.StartsWith(".employment"));
            //    if (listemploymentfields.Count() > 0)
            //    {
            //        projection = projection.Include("employment");
            //    }
            //    var listeducationfields = fieldregis.Where(x => x["jsPath"].AsString.StartsWith(".education"));
            //    if (listeducationfields.Count() > 0)
            //    {
            //        projection = projection.Include("education");
            //    }

            //    foreach (var field in fieldregis.Where(x => !x["path"].AsString.StartsWith("Custom")
            //                                                && !x["jsPath"].AsString.StartsWith(".membership")
            //                                                && !x["jsPath"].AsString.StartsWith(".address")).ToList())
            //    {
            //        string jsonpath = field["jsPath"].AsString;
            //        List<string> arrpaths = jsonpath.Trim('.').Split('.').ToList();
            //        string jsonpathreal = string.Join(".value.", arrpaths);
            //        string type = field["type"].AsString;
            //        switch (type)
            //        {
            //            case "Location":
            //                arrpaths = jsonpath.Trim('.').Split('.').ToList();
            //                if (arrpaths.Count() > 1)
            //                {
            //                    arrpaths.RemoveAt(arrpaths.Count() - 1);
            //                    jsonpathreal = string.Join(".value.", arrpaths);
            //                    projection = projection.Include(jsonpathreal + ".value.country.value");
            //                    projection = projection.Include(jsonpathreal + ".value.city.value");
            //                }
            //                continue;
            //            case "address":
            //                arrpaths = jsonpath.Trim('.').Split('.').ToList();
            //                if (arrpaths.Count() > 1)
            //                {
            //                    arrpaths.RemoveAt(arrpaths.Count() - 1);
            //                    jsonpathreal = string.Join(".value.", arrpaths);
            //                    projection = projection.Include(jsonpathreal + ".value.addressLine1.value");
            //                    projection = projection.Include(jsonpathreal + ".value.addressLine2.value");
            //                }
            //                continue;
            //            case "numinput":
            //                arrpaths = jsonpath.Trim('.').Split('.').ToList();
            //                jsonpathreal = string.Join(".value.", arrpaths);
            //                projection = projection.Include(jsonpathreal + ".value");
            //                projection = projection.Include(jsonpathreal + ".unit");

            //                continue;
            //        }

            //        switch (jsonpath)
            //        {
            //            case ".contact.phone":
            //                projection = projection.Include("contact.default");
            //                continue;
            //            default:
            //                arrpaths = jsonpath.Trim('.').Split('.').ToList();
            //                jsonpathreal = string.Join(".value.", arrpaths);
            //                projection = projection.Include(jsonpathreal + ".value");
            //                continue;
            //        }
            //    }
            //}
            //var listvault = vaultCollection.Find(filter).ToList();

            //var keywords = campaign["campaign"]["criteria"]["keywords"].AsBsonArray.Select(y => y.AsString).ToArray();
            //var listueridregisvault = new List<DataRegisCampaign>();
            //foreach (var item in listvault)
            //{
            //    var chartkeywords = new List<string>();

            //    var colour = item["others"]["value"]["favourite"]["value"]["colour"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var music_type = item["others"]["value"]["favourite"]["value"]["music_type"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var holiday = item["others"]["value"]["favourite"]["value"]["holiday"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var movie = item["others"]["value"]["favourite"]["value"]["movie"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var song = item["others"]["value"]["favourite"]["value"]["colour"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var tv_show = item["others"]["value"]["favourite"]["value"]["tv_show"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();

            //    var interesting = item["others"]["value"]["preference"]["value"]["interests"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var food = item["others"]["value"]["preference"]["value"]["food"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var seat = item["others"]["value"]["preference"]["value"]["seat"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();
            //    var season = item["others"]["value"]["preference"]["value"]["season"]["value"].AsBsonArray
            //        .Select(x => x.AsString).ToList();

            //    if (keywords.Count() > 0)
            //        chartkeywords = keywords.Where(
            //            x => colour.Contains(x) || music_type.Contains(x) ||
            //                 holiday.Contains(x) || movie.Contains(x) ||
            //                 song.Contains(x) || tv_show.Contains(x) ||
            //                 interesting.Contains(x) || food.Contains(x) ||
            //                 seat.Contains(x) || seat.Contains(x)).ToList();
            //    var listfields = new InfomationVaultBusinessLogic().getInformationvaultforcampaign(item, fieldregis,
            //        listueridregis.FirstOrDefault(x => x.userid == item["userId"].AsString).ListFieldsRegis);
            //    listueridregisvault.Add(new DataRegisCampaign
            //    {
            //        firstname_vault =
            //            BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["firstName"],
            //                "value"),
            //        lastname_vault =
            //            BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["lastName"],
            //                "value"),
            //        dob_vault = BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["dob"],
            //            "value"),
            //        gender_vault =
            //            BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["gender"],
            //                "value"),
            //        country_vault =
            //            BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["country"],
            //                "value"),
            //        city_vault =
            //            BsonHelper.GetvaluestringFromObjectOnelevel(item["basicInformation"]["value"]["city"], "value"),
            //        keywords = string.Join(",", chartkeywords),
            //        userid = item["userId"].AsString,
            //        ListFieldsRegis = listfields
            //    });
            //}


            //listueridregis = (from data1 in listueridregis
            //                  join data2 in listueridregisvault on data1.userid equals data2.userid
            //                      into a
            //                  from b in a.DefaultIfEmpty()
            //                  select new DataRegisCampaign
            //                  {
            //                      dob = data1.dob,
            //                      userid = data1.userid,
            //                      firstname = data1.firstname,
            //                      lastname = data1.lastname,
            //                      gender = data1.gender,
            //                      city = data1.city,
            //                      email = data1.email,
            //                      country = data1.country,
            //                      datefollow = data1.datefollow,
            //                      dob_vault = b.dob_vault,
            //                      firstname_vault = b.firstname_vault,
            //                      lastname_vault = b.lastname_vault,
            //                      gender_vault = b.gender_vault,
            //                      city_vault = b.city_vault,
            //                      country_vault = b.country_vault,
            //                      keywords = b.keywords,
            //                      ListFieldsRegis = b.ListFieldsRegis
            //                  }).ToList();
        }

        public DataList<Follower> GetFollowedBusinessByUserId(string userId, int pageIndex = 1,
            int pageSize = int.MaxValue)
        {
            var posts = _postRepos.Many(p => p.Followers != null && p.Followers.Any(f => f.UserId.Equals(userId)));
            var totalItems = posts.Count();
            var listOfCurrentPage = posts.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(p => new Follower
            {
                UserId = p.PostedUserId,
                Name = p.PostedUserName
            }).AsEnumerable();

            var dataList = new DataList<Follower>(listOfCurrentPage.ToList(), totalItems, pageIndex, pageSize);
            return dataList;
        }

        public List<Post> GetPostList()
        {
            var postCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Post");
            var postEnum = postCollection.Find(new BsonDocument()).ToEnumerable();
            var list = new List<Post>();

            foreach (var postItem in postEnum)
            {
                var post = new Post();
                post.PostType = postItem["PostType"].AsString;
                post.PostedUserId = postItem["PostedUserId"].AsString;
                post.CampaignId = postItem["CampaignId"].AsString;

                list.Add(post);
            }

            return list;
        }

        public List<Post> GetPostListbyUserId(string userId)
        {
            var posts = _postRepos.Many(p => p.Followers != null && p.Followers.Any(f => f.UserId.Equals(userId)));
            var list = new List<Post>();
            if (posts != null)
                list = posts.ToList();

            return list;
        }

        public List<FieldinformationVault> GetDataRegisofUser(string campaignid, string userid)
        {
            List<FieldinformationVault> fields = new List<FieldinformationVault>();
            try
            {
                var post = _postRepos.Single(x => x.CampaignId == campaignid);
                if (post != null)
                {
                    var follower = post.Followers.FirstOrDefault(y => y.UserId == userid);
                    if (follower != null)
                        fields = follower.fields == null ? new List<FieldinformationVault>() : follower.fields.ToList();
                }
            }
            catch
            {
            }

            return fields;
        }

        public List<FieldinformationVault> checkQA(string campaignId, string userid,
            List<FieldinformationVault> fieldsData)
        {
            var rs = new List<FieldinformationVault>();
            var FieldsPost = GetDataRegisofUser(campaignId, userid);
            if (FieldsPost == null || FieldsPost.Count == 0)
            {
                FieldsPost = CheckGroupField(fieldsData, campaignId);
                return FieldsPost;
            }


            FieldsPost = CheckGroupField(FieldsPost, campaignId);
            foreach (var item in FieldsPost)
            {
                try
                {
                    var field = new FieldinformationVault();
                    if (item.jsPath.StartsWith("Custom"))
                        field = item;
                    else
                    {
                        foreach (var f in fieldsData)
                        {
                            if (f != null && f.jsPath == item.jsPath)
                                field = f;
                        }
                    }
                    if (field != null)
                    {
                        var lsModel = new List<string>();
                        if (!string.IsNullOrEmpty(field.modelarraysstr) && field.modelarraysstr != "null")
                        {
                            try
                            {
                                string[] arr = field.modelarraysstr.Split(',');
                                foreach (var a in arr)
                                {
                                    var s = a.Replace("[", "").Replace("\\", "").Replace("\"", "").Replace("]", "");

                                    lsModel.Add(s);
                                }
                                if (lsModel != null)
                                    field.modelarrays = lsModel;
                            }
                            catch
                            {
                            }
                        }
                        rs.Add(field);
                    }
                }
                catch
                {
                }
            }
            return rs;
        }

        //public List<FieldinformationVault> checkQA(string campaignId, string userid, List<FieldinformationVault> fieldsData)
        //{
        //    var rs = new List<FieldinformationVault>();
        //    var FieldsPost = GetDataRegisofUser(campaignId, userid);
        //    if (FieldsPost == null || FieldsPost.Count == 0)
        //    {
        //        FieldsPost = CheckGroupField(fieldsData, campaignId);
        //        return FieldsPost;
        //    }


        //    FieldsPost = CheckGroupField(FieldsPost, campaignId);
        //    foreach (var item in FieldsPost)
        //    {
        //        try
        //        {
        //            var field = new FieldinformationVault();
        //            if (item.jsPath.StartsWith("Custom"))
        //                field = item;
        //            else
        //            {
        //                foreach (var f in fieldsData)
        //                {
        //                    if (f != null && f.jsPath == item.jsPath)
        //                        field = f;
        //                }
        //            }
        //            if (field != null)
        //            {
        //                var lsModel = new List<string>();
        //                if (!string.IsNullOrEmpty(field.modelarraysstr) && field.modelarraysstr != "null")
        //                {
        //                    try
        //                    {
        //                        string[] arr = field.modelarraysstr.Split(',');
        //                        foreach (var a in arr)
        //                        {
        //                            var s = a.Replace("[", "").Replace("\\", "").Replace("\"", "").Replace("]", "");

        //                            lsModel.Add(s);
        //                        }
        //                        if (lsModel != null)
        //                            field.modelarrays = lsModel;
        //                    }
        //                    catch { }
        //                }
        //                rs.Add(field);
        //            }
        //        }
        //        catch { }
        //    }
        //    return rs;
        //}

        public List<FieldinformationVault> CheckGroupField(List<FieldinformationVault> fields, string campaignId)
        {
            var rs = new List<FieldinformationVault>();
            var dataGroup = EnumListJsPathVaultForm.ListJsPathVaultForm;
            var camp = new CampaignBusinessLogic();
            var lstFielCamp = camp.GetListFieldByCampaignId(campaignId);


            foreach (var group in dataGroup)
            {
                foreach (var fieldCamp in lstFielCamp)
                {
                    var f = new FieldinformationVault();
                    if (fieldCamp.jsPath.StartsWith(group))
                    {
                        f = fieldCamp;
                        try
                        {
                            foreach (var field in fields)
                            {
                                if (fieldCamp.jsPath == field.jsPath)
                                {
                                    f = field;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                        }
                        if (f != null)
                            rs.Add(f);
                    }
                }
            }

            return rs;
        }

        public void DeletePostByCampaign(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Post");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            campaignCollection.DeleteMany(filter);
        }

        #region V2

        public Post GetPost(string postType, string campaignId)
        {
            var rs = new Post();
            try
            {
                if (string.IsNullOrEmpty(postType))
                    rs = _postRepos.Many(p => p.CampaignId == campaignId).FirstOrDefault();
                else
                    rs = _postRepos.Many(p => p.CampaignId == campaignId && p.PostType == postType).FirstOrDefault();
            }
            catch
            {
            }

            return rs;
        }

        public Follower GetFollowerPost(string userId, string campaignId)
        {
            var rs = new Follower();
            try
            {
                var post = _postRepos.Many(p => p.CampaignId == campaignId).FirstOrDefault();
                foreach (var item in post.Followers)
                {
                    if (item.UserId == userId)
                        rs = item;
                }
            }
            catch
            {
            }

            return rs;
        }

        public string AddPost(string postType, string campaignId, string FromUserId, Follower follower = null)
        {
            try
            {
                var existPost = _postRepos.Many(p => p.CampaignId == campaignId).FirstOrDefault();
                if (existPost != null)
                {
                    var check = true;
                    var fol = existPost.Followers.ToList();

                    for (var i = 0; i < existPost.Followers.Count(); i++)
                    {
                        if (follower.UserId == fol[i].UserId)
                        {
                            fol[i] = follower;
                            check = false;
                            break;
                        }
                    }
                    if (check)
                        fol.Add(follower);
                    existPost.Followers = fol;
                    return UpdatePost(existPost);
                }
                else
                {
                    var campaign = new CampaignBusinessLogic();
                    var userId = "";
                    if (string.IsNullOrEmpty(FromUserId))
                        userId = FromUserId;
                    else
                        userId = campaign.UserIdByCampaignId(campaignId);
                    var Id = ObjectId.GenerateNewId();
                    var followers = new List<Follower>();
                    if (follower != null)
                        followers.Add(follower);
                    var post = new Post
                    {
                        Id = ObjectId.GenerateNewId(),
                        PostedUserId = userId,
                        PostedUserName = "",
                        CampaignId = campaignId,
                        PostType = postType,
                        Followers = followers,
                        Created = DateTime.Now
                    };
                    _postRepos.Add(post);

                    return Id.ToString();
                }
            }
            catch
            {
            }


            return null;
        }

        public string UpdatePost(Post post)
        {
            try
            {
                _postRepos.Update(post);
                return post.Id.ToString();
            }
            catch
            {
            }
            return null;
        }

        public string RegisPost(string campaignId, Follower follower, Account businessUserAccount)
        {
            try
            {
                var account = new AccountService().GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                follower.UserId = account.AccountId;
                follower.Name = account.Profile.DisplayName;
                follower.Age = (account.Profile.Birthdate.HasValue)
                    ? (DateTime.Now.Year - account.Profile.Birthdate.Value.Year)
                    : 0;
                follower.Gender = account.Profile.Gender;
                follower.CountryName = account.Profile.Country;
                follower.CityName = account.Profile.City;

                var post = _postRepos.Many(p => p.CampaignId == campaignId).FirstOrDefault();
                if (post != null)
                {
                    try
                    {
                        var followers = post.Followers.ToList();
                        var AddNew = false;
                        for (var i = 0; i < followers.Count; i++)
                        {
                            if (followers[i].UserId == follower.UserId)
                            {
                                followers[i] = follower;
                                AddNew = true;
                                break;
                            }
                        }
                        if (AddNew == false)
                            followers.Add(follower);
                        post.Followers = followers;
                        _postRepos.Update(post);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    BsonDocument c = new BusinessInteractionService().GetInteraction(campaignId);
                    var businessId = c["userId"].AsString;

                    var businessAccount = new AccountService().GetByAccountId(businessId);

                    var followers = new List<Follower> {follower};
                    post = new Post
                    {
                        Id = ObjectId.GenerateNewId(),
                        PostedUserId = businessId,
                        PostedUserName = businessAccount.Profile.DisplayName,
                        CampaignId = campaignId,
                        PostType = "SRFI",
                        Followers = followers,
                        Created = DateTime.Now,
                    };
                    _postRepos.Add(post);
                }

                return post.Id.ToString();
            }
            catch
            {
            }
            return null;
        }

        public string Delete(Post post)
        {
            try
            {
                _postRepos.Delete(post);
                return post.Id.ToString();
            }
            catch
            {
            }
            return null;
        }

        #endregion V2
    }
}