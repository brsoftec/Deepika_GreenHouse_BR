using GH.Core.BlueCode.DataAccess;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.Services;
using GH.Core.BlueCode.Entity.InformationVault;
using Newtonsoft.Json;
using System.Threading.Tasks;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.Entity.PostHandShake;
using System.Web.Hosting;
using GH.Core.ViewModels;
using GH.Core.BlueCode.Entity.Outsite;
using OfficeOpenXml;
using GH.Core.Exceptions;
using GH.Core.BlueCode.Entity.Request;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class PostHandShakeBusinessLogic
    {
        public string InsertPostHandshake(string campaignid, string posthandshakecomment, string busid, string userId,
            List<FieldinformationVault> listOfFields)
        {
            // postHandShakeModel.posthandshakecomment;
            string postHandshakeid = BsonHelper.GenerateObjectIdString();
            string jsondata = JsonConvert.SerializeObject(listOfFields);
            try
            {
                var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var fil = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                fil = fil & Builders<BsonDocument>.Filter.Eq("userId", userId);
                fil = fil & Builders<BsonDocument>.Filter.Eq("Status", "Terminate");
                if (postHandShakeCollection.Find(fil).Count() > 0)
                {
                    postHandShakeCollection.DeleteMany(fil);
                }
                var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                if (postHandShakeCollection.Find(filter).Count() > 0)
                {
                    return campaignid;
                }
                var posthandshake = new BsonDocument
                {
                    {"_id", postHandshakeid},
                    {"userId", userId},
                    {"busid", busid},
                    {"isjoin", "0"},
                    {"campaignid", campaignid},
                    {"comment", posthandshakecomment},
                    {"jsondata", jsondata},
                    {"jsondataold", jsondata},
                    {"Status", "Not acknowledged"},
                    {"dateupdatejson", ""},
                    {"ischange", "0"},
                    {"isaccecpt", "0"},
                };
                postHandShakeCollection.InsertOne(posthandshake);
            }
            catch
            {
            }
            return campaignid;
        }

        public string InsertPostHandshakeNew(string campaignid, string posthandshakecomment, string busid,
            string userId, List<FieldinformationVault> listOfFields)
        {
            // postHandShakeModel.posthandshakecomment;
            string postHandshakeid = BsonHelper.GenerateObjectIdString();
            string jsondata = JsonConvert.SerializeObject(listOfFields);
            try
            {
                var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var fil = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                fil = fil & Builders<BsonDocument>.Filter.Eq("userId", userId);
                fil = fil & Builders<BsonDocument>.Filter.Eq("Status", "Terminate");
                if (postHandShakeCollection.Find(fil).Count() > 0)
                {
                    postHandShakeCollection.DeleteMany(fil);
                }
                var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                if (postHandShakeCollection.Find(filter).Count() > 0)
                {
                    return campaignid;
                }
                var posthandshake = new BsonDocument
                {
                    {"_id", postHandshakeid},
                    {"userId", userId},
                    {"busid", busid},
                    {"isjoin", "0"},
                    {"campaignid", campaignid},
                    {"comment", posthandshakecomment},
                    {"jsondata", jsondata},
                    {"jsondataold", jsondata},
                    {"Status", "Not acknowledged"},
                    {"dateupdatejson", ""},
                    {"ischange", "0"},
                    {"isaccecpt", "0"}
                };
                postHandShakeCollection.InsertOne(posthandshake);
            }
            catch
            {
            }
            return campaignid;
        }

        public string InsertPostHandshakeCheck(string campaignid, string busid, string userId,
            List<FieldinformationVault> listOfFields)
        {
            string postHandshakeid = BsonHelper.GenerateObjectIdString();
            string jsondata = JsonConvert.SerializeObject(listOfFields);
            try
            {
                var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var fil = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                fil = fil & Builders<BsonDocument>.Filter.Eq("userId", userId);
                fil = fil & Builders<BsonDocument>.Filter.Eq("Status", "Terminate");
                if (postHandShakeCollection.Find(fil).Count() > 0)
                {
                    postHandShakeCollection.DeleteMany(fil);
                }

                var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                if (postHandShakeCollection.Find(filter).Count() > 0)
                {
                    return campaignid;
                }
                var posthandshake = new BsonDocument
                {
                    {"_id", postHandshakeid},
                    {"userId", userId},
                    {"busid", busid},
                    {"isjoin", "0"},
                    {"campaignid", campaignid},
                    {"jsondata", jsondata},
                    {"jsondataold", jsondata},
                    {"Status", "Not acknowledged"},
                    {"dateupdatejson", ""},
                    {"ischange", "0"},
                    {"isaccecpt", "0"},
                };
                postHandShakeCollection.InsertOne(posthandshake);
            }
            catch
            {
            }

            return campaignid;
        }

        public string InsertPostHandshakeOutSite(string campaignid, string busid, string userId,
            List<FieldinformationVault> listOfFields)
        {
            string postHandshakeid = BsonHelper.GenerateObjectIdString();
            string jsondata = JsonConvert.SerializeObject(listOfFields);
            DateTime DateCreate = DateTime.Now;
            try
            {
                var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("OutSite");
                var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
                filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & !Builders<BsonDocument>.Filter.Eq("Status", "Terminate");
                if (postHandShakeCollection.Find(filter).Count() > 0)
                {
                    return campaignid;
                }
                var posthandshake = new BsonDocument
                {
                    {"_id", postHandshakeid},
                    {"userId", userId},
                    {"busid", busid},
                    {"isjoin", "0"},
                    {"campaignid", campaignid},
                    {"jsondata", jsondata},
                    {"jsondataold", jsondata},
                    {"Status", "Not acknowledged"},
                    {"dateupdatejson", ""},
                    {"ischange", "0"},
                    {"isaccecpt", "0"},
                    {"datecreate", DateCreate}
                };
                postHandShakeCollection.InsertOne(posthandshake);
            }
            catch
            {
            }

            return campaignid;
        }

        public List<string> Getlisthandshakefollow(string userid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("isjoin", "1");
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable().Select(x => x["campaignid"].AsString)
                .ToList();
            return totalList;
        }

        public long CountUserInvitedHandshake(string businessid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("busid", businessid);
            var insiteuser = postHandShakeCollection.Find(criteria).Count();
            var outsiteCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Outsite");
            var filter1 = Builders<BsonDocument>.Filter;
            var criteria1 = filter1.Eq("FromUserId", businessid);
            criteria1 = criteria1 & filter1.Eq("Type", "Invited Handshake Outsite");
            var outsiteuser = outsiteCollection.Find(criteria1).Count();
            return insiteuser + outsiteuser;
        }

        public List<string> Getlisthandshakefollowcampaign(string campaignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("campaignid", campaignid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable().Select(x => x["campaignid"].AsString)
                .ToList();
            return totalList;
        }

        public int GetCountUserhoidHandshakebycmapaignid(string campaignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("campaignid", campaignid);
            criteria = criteria & filter.Eq("isjoin", "1");
            criteria = criteria & !filter.Eq("Status", "Terminate");
            return (int) postHandShakeCollection.Find(criteria).Count();
        }

        public int GetCountUserInvitedPending(string campaignid)
        {
            try
            {
                var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
                var filter = Builders<BsonDocument>.Filter;
                var criteria = filter.Eq("campaignid", campaignid);
                criteria = criteria & filter.Eq("isaccecpt", "0");
                criteria = criteria & !filter.Eq("Status", "Terminate");
                return (int) postHandShakeCollection.Find(criteria).Count();
            }
            catch
            {
                return 0;
            }
        }

        public List<PostHandShake> GetPostHandShakeByCamapignId(string campaignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("campaignid", campaignid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                try
                {
                    var checkTerminate = false;
                    var user = new AccountService().GetByAccountId(post["userId"].AsString);
                    PostHandShake newPostHandShake = new PostHandShake();
                    if (!post.TryGetValue("comment", out BsonValue cm) || cm == BsonNull.Value)
                        cm = string.Empty;
                      

                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? user.Profile.FirstName + " " + user.Profile.LastName
                        : user.Profile.DisplayName;
                    newPostHandShake.CampaignId = campaignid;
                    newPostHandShake.Comment = cm.AsString;

                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Status = post["Status"].AsString;
                    if (newPostHandShake.Status == "Terminate")
                        checkTerminate = true;
                    newPostHandShake.IsChange = post["ischange"].AsString == "1" ? true : false;
                    newPostHandShake.JsondataOld = post["jsondataold"].AsString;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                    if (!checkTerminate)
                        list.Add(newPostHandShake);
                }
                catch
                {
                }
            }
            return list;
        }

        public List<PostHandShake> GetPostHandShakeTerminateByCamapignId(string campaignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("campaignid", campaignid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                try
                {
                    var checkTerminate = false;
                    var user = new AccountService().GetByAccountId(post["userId"].AsString);
                    var dt = new DateTime();

                    var cm = "";
                    try
                    {
                        if (!string.IsNullOrEmpty(post["comment"].AsString))
                            cm = post["comment"].AsString;
                        dt = post["dateTerminate"].AsDateTime;
                    }
                    catch
                    {
                    }

                    PostHandShake newPostHandShake = new PostHandShake();
                    if (!string.IsNullOrEmpty(dt.ToString()))
                    {
                        newPostHandShake.DateTerminate = dt.ToString();
                    }
                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? user.Profile.FirstName + " " + user.Profile.LastName
                        : user.Profile.DisplayName;
                    newPostHandShake.CampaignId = campaignid;

                    newPostHandShake.Comment = cm;
                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Status = post["Status"].AsString;
                    if (newPostHandShake.Status == "Terminate")
                        checkTerminate = true;
                    newPostHandShake.IsChange = post["ischange"].AsString == "1" ? true : false;
                    newPostHandShake.JsondataOld = post["jsondataold"].AsString;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                    if (checkTerminate)
                        list.Add(newPostHandShake);
                }
                catch
                {
                }
            }
            return list;
        }

        public List<PostHandShake> GetPostHandShakeByuserId(string userid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                try
                {
                    var checkTerminate = false;
                    var user = new AccountService().GetByAccountId(post["userId"].AsString);
                    PostHandShake newPostHandShake = new PostHandShake();
                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.CampaignId = post["campaignid"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? ""
                        : user.Profile.FirstName + " " + user.Profile.LastName;
                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Status = post["Status"].AsString;
                    if (newPostHandShake.Status == "Terminate")
                        checkTerminate = true;
                    newPostHandShake.IsChange = post["ischange"].AsString == "1" ? true : false;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                    if (!checkTerminate)
                        list.Add(newPostHandShake);
                }
                catch
                {
                }
            }
            return list;
        }

        public List<PostHandShake> GetAllPostHandShakeByuserId(string userid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                try
                {
                    var user = new AccountService().GetByAccountId(post["userId"].AsString);
                    PostHandShake newPostHandShake = new PostHandShake();
                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.CampaignId = post["campaignid"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? ""
                        : user.Profile.FirstName + " " + user.Profile.LastName;
                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Status = post["Status"].AsString;
                    newPostHandShake.IsChange = post["ischange"].AsString == "1" ? true : false;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                    list.Add(newPostHandShake);
                }
                catch
                {
                }
            }
            return list;
        }

        public List<PostHandShake> GetPostHandShakeTerminateByuserId(string userid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");

            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                try
                {
                    var dt = new DateTime();
                    var cm = "";
                    try
                    {
                        if (!string.IsNullOrEmpty(post["comment"].AsString))
                            cm = post["comment"].AsString;
                        dt = post["dateTerminate"].AsDateTime;
                    }
                    catch
                    {
                    }

                    var checkTerminate = false;
                    var user = new AccountService().GetByAccountId(post["userId"].AsString);
                    PostHandShake newPostHandShake = new PostHandShake();

                    if (!string.IsNullOrEmpty(dt.ToString()))
                    {
                        newPostHandShake.DateTerminate = dt.ToString();
                    }
                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.CampaignId = post["campaignid"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? ""
                        : user.Profile.FirstName + " " + user.Profile.LastName;
                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Comment = cm;
                    newPostHandShake.Status = post["Status"].AsString;
                    if (newPostHandShake.Status == "Terminate")
                        checkTerminate = true;
                    newPostHandShake.IsChange = post["ischange"].AsString == "1" ? true : false;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                    if (checkTerminate)
                        list.Add(newPostHandShake);
                }
                catch
                {
                }
            }
            return list;
        }

        public PostHandShake GetPostHandShakeByuserIdandCamapignid(string userid, string camapignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("campaignid", camapignid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                var checkTerminate = false;
                var user = new AccountService().GetByAccountId(post["userId"].AsString);
                PostHandShake newPostHandShake = new PostHandShake();
                try
                {
                    newPostHandShake.BusId = post["busid"].AsString;
                    newPostHandShake.Id = post["_id"].AsString;
                    newPostHandShake.UserId = post["userId"].AsString;
                    newPostHandShake.CampaignId = post["campaignid"].AsString;
                    newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                        ? ""
                        : user.Profile.FirstName + " " + user.Profile.LastName;
                    newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                    newPostHandShake.JsondataOld = post["jsondataold"].AsString;
                    newPostHandShake.Jsondata = post["jsondata"].AsString;
                    newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                    newPostHandShake.Status = post["Status"].AsString;
                    if (newPostHandShake.Status == "Terminate")
                        checkTerminate = true;
                    try
                    {
                        newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }

                if (!checkTerminate && newPostHandShake != null)
                    list.Add(newPostHandShake);
            }
            return list.FirstOrDefault();
        }

        public BsonArray GetArrayFieldsHandShakeByUserIdCamapignIdFull(string userid, string camapignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("campaignid", camapignid);
            var hs = postHandShakeCollection.Find(criteria).FirstOrDefault();
            var arr = new BsonArray();
            arr = hs["fields"].AsBsonArray;

            return arr;
        }

        public PostHandShake GetPostHandShakeByuserIdandCamapignidForDelete(string userid, string camapignid)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("campaignid", camapignid);
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            var list = new List<PostHandShake>();
            foreach (var post in totalList)
            {
                var user = new AccountService().GetByAccountId(post["userId"].AsString);
                PostHandShake newPostHandShake = new PostHandShake();
                newPostHandShake.BusId = post["busid"].AsString;
                newPostHandShake.Id = post["_id"].AsString;
                newPostHandShake.UserId = post["userId"].AsString;
                newPostHandShake.CampaignId = post["campaignid"].AsString;
                newPostHandShake.UserName = string.IsNullOrEmpty(user.Profile.DisplayName)
                    ? ""
                    : user.Profile.FirstName + " " + user.Profile.LastName;
                newPostHandShake.IsJoin = post["isjoin"].AsString == "1" ? true : false;
                newPostHandShake.JsondataOld = post["jsondataold"].AsString;
                newPostHandShake.Jsondata = post["jsondata"].AsString;
                newPostHandShake.DateUpdateJson = post["dateupdatejson"].AsString;
                newPostHandShake.Status = post["Status"].AsString;

                try
                {
                    newPostHandShake.isaccecpt = post["isaccecpt"].AsString == "1" ? true : false;
                }
                catch
                {
                }
                list.Add(newPostHandShake);
            }
            return list.FirstOrDefault();
        }

        public void DeletePostHandshake(string campaignId, string userid)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            campaignCollection.FindOneAndDelete(filter);
        }

        public void DeletePostHandByCampaign(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            campaignCollection.DeleteMany(filter);
        }

        public void TerminatePostHandshake(string campaignId, string userid)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            var update = Builders<BsonDocument>.Update.Set("Status", "Terminate");
            campaignCollection.UpdateOne(filter, update);
            var update1 = Builders<BsonDocument>.Update.Set("isjoin", "0");
            campaignCollection.UpdateOne(filter, update1);
            DateTime DateTerminate = DateTime.Now;
            var update2 = Builders<BsonDocument>.Update.Set("dateTerminate", DateTerminate);
            campaignCollection.UpdateOne(filter, update2);
        }

        public void TerminatePostHandshakeByCampaignId(string campaignId)
        {
            var lstHS = new List<PostHandShake>();
            var _accountService = new AccountService();
            try
            {
                lstHS = GetPostHandShakeByCamapignId(campaignId);
                if (lstHS.Count > 0)
                {
                    var busUser = _accountService.GetByAccountId(lstHS[0].BusId);
                    var notificationMessageBus = new NotificationMessage();
                    notificationMessageBus.Id = ObjectId.GenerateNewId();
                    notificationMessageBus.Type = EnumNotificationType.NotifyExpiredDateHandshake;
                    notificationMessageBus.FromAccountId = "";
                    notificationMessageBus.FromUserDisplayName = "";
                    notificationMessageBus.ToAccountId = busUser.AccountId;
                    notificationMessageBus.ToUserDisplayName = busUser.Profile.DisplayName;
                    notificationMessageBus.PreserveBag = lstHS[0].CampaignId;
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessageBus);

                    for (var i = 0; i < lstHS.Count; i++)
                    {
                        try
                        {
                            var userfrom = _accountService.GetByAccountId(lstHS[i].BusId);

                            var userto = _accountService.GetByAccountId(lstHS[i].UserId);

                            new PostHandShakeBusinessLogic().TerminatePostHandshake(lstHS[i].CampaignId,
                                lstHS[i].UserId);
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyTerminateHandshake;
                            notificationMessage.FromAccountId = userfrom.AccountId;
                            notificationMessage.FromUserDisplayName = userfrom.Profile.DisplayName;
                            notificationMessage.ToAccountId = userto.AccountId;
                            notificationMessage.ToUserDisplayName = userto.Profile.DisplayName;
                            notificationMessage.PreserveBag = lstHS[i].CampaignId;
                            //  var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }

            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);

            var update = Builders<BsonDocument>.Update.Set("Status", "Terminate");
            campaignCollection.UpdateMany(filter, update);
            var update1 = Builders<BsonDocument>.Update.Set("isjoin", "0");
            campaignCollection.UpdateMany(filter, update1);
            DateTime DateTerminate = DateTime.Now;
            var update2 = Builders<BsonDocument>.Update.Set("dateTerminate", DateTerminate);
            campaignCollection.UpdateOne(filter, update2);
        }

        public void UserUnjoinorjoinHandshake(string campaignid, string userid, bool isjoin)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            var update = Builders<BsonDocument>.Update.Set("isjoin", isjoin ? "0" : "1");
            campaignCollection.UpdateOne(filter, update);
            var update1 = Builders<BsonDocument>.Update.Set("isaccecpt", "1");
            campaignCollection.UpdateOne(filter, update1);
        }

        public void UserUnjoinorjoinHandshakeListField(string campaignid, string userid, bool isjoin,
            List<FieldinformationVault> lstField)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            var update = Builders<BsonDocument>.Update.Set("isjoin", isjoin ? "0" : "1");
            campaignCollection.UpdateOne(filter, update);
            var update1 = Builders<BsonDocument>.Update.Set("isaccecpt", "1");
            campaignCollection.UpdateOne(filter, update1);
            try
            {
                var update2 = Builders<BsonDocument>.Update.Set("fields", lstField);
                campaignCollection.UpdateOne(filter, update2);
            }
            catch
            {
            }
        }

        public void AcknowledgeHandshake(string campaignid, string userid, bool isAcknowledge)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignid);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            var update =
                Builders<BsonDocument>.Update.Set("Status", isAcknowledge ? "Not acknowledged" : "acknowledged");
            campaignCollection.UpdateOne(filter, update);
        }

        public void UpdatePostHandshakeJsonData(string campaignId, string userid, string jsondata, string jsondataold,
            string dateupdate)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userid);
            PostHandShakeBusinessLogic postHS = new PostHandShakeBusinessLogic();
            var listData = JsonConvert.DeserializeObject<List<FieldinformationVault>>(jsondata);
            var fieldsOfHandShake = new BsonArray();
            var newData = new List<FieldinformationVault>();
            try
            {
                fieldsOfHandShake = postHS.GetArrayFieldsHandShakeByUserIdCamapignIdFull(userid, campaignId);
                foreach (var item in fieldsOfHandShake)
                {
                    for (var i = 0; i < listData.Count(); i++)
                    {
                        if (listData[i].jsPath == item["jsPath"].AsString)
                        {
                            newData.Add(listData[i]);
                        }
                    }
                }
                if (newData.Count > 0)
                {
                    jsondata = JsonConvert.SerializeObject(newData);
                }
            }
            catch
            {
            }

            var update = Builders<BsonDocument>.Update.Set("jsondata", jsondata);
            campaignCollection.UpdateOne(filter, update);
            update = Builders<BsonDocument>.Update.Set("jsondataold", jsondataold);
            campaignCollection.UpdateOne(filter, update);
            update = Builders<BsonDocument>.Update.Set("dateupdatejson", dateupdate);
            campaignCollection.UpdateOne(filter, update);
            update = Builders<BsonDocument>.Update.Set("ischange", "1");
            campaignCollection.UpdateOne(filter, update);
            update = Builders<BsonDocument>.Update.Set("Status", "Not acknowledged");
            campaignCollection.UpdateOne(filter, update);
        }

        private static bool CompareObjects(JObject source, JObject target)
        {
            foreach (KeyValuePair<string, JToken> sourcePair in source)
            {
                if (sourcePair.Value.Type == JTokenType.Object)
                {
                    if (target.GetValue(sourcePair.Key) == null)
                    {
                        return true;
                    }
                    else if (target.GetValue(sourcePair.Key).Type != JTokenType.Object)
                    {
                        return true;
                    }
                    else
                    {
                        if (CompareObjects(sourcePair.Value.ToObject<JObject>(),
                            target.GetValue(sourcePair.Key).ToObject<JObject>())) return true;
                    }
                }
                else if (sourcePair.Value.Type == JTokenType.Array)
                {
                    if (target.GetValue(sourcePair.Key) == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (CompareArrays(sourcePair.Value.ToObject<JArray>(),
                            target.GetValue(sourcePair.Key).ToObject<JArray>(), sourcePair.Key)) return true;
                    }
                }
                else
                {
                    JToken expected = sourcePair.Value;
                    var actual = target.SelectToken(sourcePair.Key);
                    if (actual == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (!JToken.DeepEquals(expected, actual))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool CompareArrays(JArray source, JArray target, string arrayName = "")
        {
            if (source.Count != target.Count) return true;
            for (var index = 0; index < source.Count; index++)
            {
                var expected = source[index];
                if (expected.Type == JTokenType.Object)
                {
                    JObject here = expected.ToObject<JObject>();
                    string jsPath = here.GetValue("jsPath").ToString();
                    JObject there = target.Children<JObject>()
                        .FirstOrDefault(o => o.GetValue("jsPath").ToString().Equals(jsPath));
                    if (CompareObjects(here, there)) return true ;
                }
                else
                {
                    var actual = target[index];
                    if (!JToken.DeepEquals(expected, actual))
                    {
                        if (String.IsNullOrEmpty(arrayName))
                        {
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private void CheckUpdateVaultHandshake(string userid, bool firstReg = false)
        {
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", userid);
            criteria = criteria & filter.Eq("isjoin", "1") & !filter.Eq("Status", "Terminate");
            var totalList = postHandShakeCollection.Find(criteria).ToEnumerable();
            foreach (var posthandshake in totalList)
            {
                var campaignid = posthandshake["campaignid"].AsString;
                var camp = new CampaignBusinessLogic();
                var checkRemove = camp.CheckRemoveCampaign(campaignid);
                if (!checkRemove)
                {
                    var checkEndDate = camp.CheckEndDateCampaign(campaignid);
                    if (!checkEndDate)
                    {
                        var busid = posthandshake["busid"].AsString;
                        string jsondataofposthandshare = posthandshake["jsondata"].AsString;

                        List<FieldinformationVault> listOfFields =
                            new InfomationVaultBusinessLogic().getInformationvaultforcampaign(userid, campaignid);

                        string jsondata = JsonConvert.SerializeObject(listOfFields);

                        bool diff = false;

                        if (string.IsNullOrEmpty(jsondataofposthandshare) || jsondataofposthandshare.Length < 3)
                        {
                            firstReg = true;
                            diff = true;
                        }
                        else
                        {
                            JArray source = JsonConvert.DeserializeObject<JArray>(jsondata);

                            JArray target = JsonConvert.DeserializeObject<JArray>(jsondataofposthandshare);

//                            var compare = 

                            diff = CompareArrays(source, target);
                            //diff = !JToken.DeepEquals(source, target);
                        }

//                        if (!jsondata.Equals(jsondataofposthandshare))
                        if (diff)
                        {
                            UpdatePostHandshakeJsonData(campaignid, userid, jsondata,
                                posthandshake["jsondata"].AsString, DateTime.Now.ToString("yyyy/MM/dd"));
                            var notificationMessage = new NotificationMessage();
                            notificationMessage.Id = ObjectId.GenerateNewId();
                            notificationMessage.Type = EnumNotificationType.NotifyHandShakeVaultChanged;
                            notificationMessage.FromAccountId = userid;
                            notificationMessage.FromUserDisplayName =
                                new AccountService().GetByAccountId(userid).Profile.DisplayName;
                            notificationMessage.ToAccountId = busid;
                            notificationMessage.ToUserDisplayName =
                                new AccountService().GetByAccountId(busid).Profile.DisplayName;
                            notificationMessage.PreserveBag = campaignid;
                            var notificationBus = new NotificationBusinessLogic();
                            notificationBus.SendNotification(notificationMessage);
                        }
                        ExportToSendMailHandShake(userid, busid, campaignid);
                    }
                }
            }
        }

        public void TaskCheckUpdateVaultHandshake(string userid, bool firstReg = false)
        {
            Task taskA = new Task(() =>
                CheckUpdateVaultHandshake(userid, firstReg)
            );
            taskA.Start();
        }

        public async Task ExportToSendMailHandShake(string userId, string busUserId, string campaignId)
        {
            try
            {
                var baseUrl = "";
                var _outsiteBusinessLogic = new OutsiteBusinessLogic();
                var type = EnumNotificationType.NotifySyncHandshakeToMailOutsite;

                var outsite = new Outsite();
                outsite = _outsiteBusinessLogic.GetOutsiteByCompnentId(campaignId, type);
                var email = outsite.Email;
                var lstEmail = outsite.ListEmail;
                var option = outsite.Option;

                var vm = new HandShakeViewModel();
                var _accountService = new AccountService();
                var _infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
                var userAccount = _accountService.GetByAccountId(userId);

                var CompanyName = "";
                var businessUserAccount = _accountService.GetByAccountId(busUserId);
                CompanyName = businessUserAccount.CompanyDetails.CompanyName;
                var CampaignBus = new CampaignBusinessLogic();
                var Campaign = CampaignBus.GetCampaignInfor(campaignId);
                var CampaignName = Campaign.CampaignName;
                vm.ListOfFields = _infomationVaultBusinessLogic.getInformationvaultforcampaign(userId, campaignId);

                //check option
                PostHandShakeBusinessLogic postHS = new PostHandShakeBusinessLogic();
                var listData = _infomationVaultBusinessLogic.getInformationvaultforcampaign(userId, campaignId);
                var fieldsOfHandShake = new BsonArray();
                var newData = new List<FieldinformationVault>();
                try
                {
                    fieldsOfHandShake = postHS.GetArrayFieldsHandShakeByUserIdCamapignIdFull(userId, campaignId);
                    foreach (var item in fieldsOfHandShake)
                    {
                        for (var i = 0; i < listData.Count(); i++)
                        {
                            if (listData[i].jsPath == item["jsPath"].AsString)
                            {
                                newData.Add(listData[i]);
                            }
                        }
                    }
                    if (newData.Count > 0)
                    {
                        vm.ListOfFields = newData;
                    }
                }
                catch
                {
                }
                // End check option

                var posthandshake =
                    new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(userId, campaignId);
                if (posthandshake != null)
                {
                    vm.ListOfFieldsOld =
                        JsonConvert.DeserializeObject<List<FieldinformationVault>>(posthandshake.JsondataOld);
                }
                var HandShakeId = posthandshake.Id;
                var DisplayName = userAccount.Profile.DisplayName;
                var fileName = DisplayName + ".xlsx";
                var fileSave =
                    HostingEnvironment.MapPath(System.IO.Path.Combine("~/Content/vault/HandShake/", HandShakeId,
                        fileName));
                if (System.IO.File.Exists(fileSave))
                {
                    try
                    {
                        System.IO.File.Delete(fileSave);
                    }
                    catch (Exception ex)
                    {
                        throw new CustomException("Error, delete file: " + ex.ToString());
                    }
                }
                var directory = new System.IO.DirectoryInfo(
                    HostingEnvironment.MapPath(System.IO.Path.Combine("~/Content/vault/HandShake/", HandShakeId)));

                if (!directory.Exists)
                {
                    try
                    {
                        directory.Create();
                    }
                    catch
                    {
                    }
                }

                var lstField = new List<FieldHandShakeViewModel>();
                var dateUpdate = DateTime.Now;
                if (vm.ListOfFieldsOld.Count == 0)
                {
                    for (int i = 0; i < vm.ListOfFields.Count; i++)
                    {
                        var field = new FieldHandShakeViewModel();
                        field.Id = i + 1;
                        field.Name = vm.ListOfFields[i].label;
                        field.NewValue = vm.ListOfFields[i].model;
                        field.OldValue = "";
                        field.UpdateDate = dateUpdate;
                        field.IsChange = false;
                        lstField.Add(field);
                    }
                }
                else
                {
                    for (int i = 0; i < vm.ListOfFields.Count; i++)
                    {
                        for (int j = 0; j < vm.ListOfFieldsOld.Count; j++)
                        {
                            var field = new FieldHandShakeViewModel();
                            if (vm.ListOfFieldsOld[j].label == vm.ListOfFieldsOld[i].label)
                            {
                                field.Id = i + 1;
                                field.Name = vm.ListOfFields[i].label;
                                field.NewValue = vm.ListOfFields[i].model;
                                field.OldValue = vm.ListOfFieldsOld[j].model;
                                field.UpdateDate = dateUpdate;
                                if (field.NewValue == field.OldValue)
                                    field.IsChange = false;
                                else
                                    field.IsChange = true;
                                lstField.Add(field);
                            }
                        }
                    }
                }

                // End
                if (outsite.SendMe == false)
                {
                    email = "";
                }
                var networkService = new NetworkService();
                if (outsite.Option == "attachment")
                {
                    var contentHtml = "";
                    var filePath = DisplayName + ".xlsx";
                    GenerateXLS(lstField, fileSave);
                    networkService.SyncListMailHandShake(HandShakeId, DisplayName, email, lstEmail, userId, filePath,
                        contentHtml, baseUrl, CompanyName, CampaignName);
                }
                else if (outsite.Option == "data")
                {
                    var contentHtml = ExportToHtml(lstField);
                    var filePath = "";

                    networkService.SyncListMailHandShake(HandShakeId, DisplayName, email, lstEmail, userId, filePath,
                        contentHtml, baseUrl, CompanyName, CampaignName);
                }
                else if (outsite.Option == "text")
                {
                    var contentHtml = "";
                    var filePath = "";
                    networkService.SyncListMailHandShake(HandShakeId, DisplayName, email, lstEmail, userId, filePath,
                        contentHtml, baseUrl, CompanyName, CampaignName);
                }
            }
            catch
            {
            }
        }

        public string ExportToHtml(List<FieldHandShakeViewModel> lstField)
        {
            var ContentHtml =
                "<table><tr><th>Id</th><th>Field Name</th><th>Old value</th><th>New Value</th> <th>Update Value</th> <th>Update Date</th></tr>";
            for (int i = 0; i < lstField.Count; i++)
            {
                ContentHtml += "<tr><td>" + lstField[i].Id + "</td><td>" + lstField[i].Name + "</td><td>" +
                               lstField[i].OldValue +
                               "</td> <td>" + lstField[i].NewValue + "</td> <td>" + lstField[i].IsChange +
                               "</td> <td>" + lstField[i].UpdateDate + "</td></tr>";
            }
            ContentHtml += "</table>";
            return ContentHtml;
        }

        public string ExportRequestToHtml(List<Request> request)
        {
            var ContentHtml =
                "<table><tr><th>No.</th><th>First Name</th><th>Last Name</th> <th>Email</th> <th>Phone</th><th>Send Date</th></tr>";
            for (int i = 0; i < request.Count; i++)
            {
                var noRow = i + 1;
                ContentHtml += "<tr><td>" + noRow + "</td><td>" + request[i].FirstName + "</td><td>" +
                               request[i].LastName +
                               "</td> <td>" + request[i].Email + "</td> <td>" + request[i].Phone + "</td> <td>" +
                               request[i].CreatedDate.ToShortDateString() + "</td></tr>";
            }
            ContentHtml += "</table>";
            return ContentHtml;
        }

        public void GenerateXLS(List<FieldHandShakeViewModel> lstField, string filePath)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("MailHandshake");
                ws.Cells[2, 2].Value = "No.";
                ws.Cells[2, 3].Value = "Field Name";
                ws.Cells[2, 4].Value = "Old Value";
                ws.Cells[2, 5].Value = "New Value";
                ws.Cells[2, 6].Value = "Update Value";
                ws.Cells[2, 7].Value = "Update Date";
                for (int i = 0; i < lstField.Count; i++)
                {
                    ws.Cells[i + 3, 2].Value = lstField[i].Id;
                    ws.Cells[i + 3, 3].Value = lstField[i].Name;
                    ws.Cells[i + 3, 4].Value = lstField[i].OldValue;
                    ws.Cells[i + 3, 5].Value = lstField[i].NewValue;
                    ws.Cells[i + 3, 6].Value = lstField[i].IsChange;
                    ws.Cells[i + 3, 7].Value = lstField[i].UpdateDate.ToShortDateString();
                }
                pck.SaveAs(new System.IO.FileInfo(filePath));
            }
        }

        public void ExportToSendMailHandShakeRequest(string requestId)
        {
            try
            {
                var requestBus = new RequestBusinessLogic();
                var request = requestBus.GetById(requestId);

                if (request == null) return;

                OutsiteBusinessLogic _outsiteBusinessLogic = new OutsiteBusinessLogic();
                var type = EnumNotificationType.NotifyEmailHandshakeRequest;
                var outsite = _outsiteBusinessLogic.GetOutsiteByUserId(request.ToUserId, type);
                if (outsite == null) return;


                var email = outsite.Email;
                var lstEmail = outsite.ListEmail;
                var option = outsite.Option;
                var toUserRequest = outsite.FromDisplayName ?? request.FirstName + " " + request.LastName;

                var listRequest = new List<Request>();
                listRequest.Add(request);

                var account = new AccountService();
                var accountFromRequest = account.GetByAccountId(request.FromUserId);
                var fromUserRequest = "Anonymous";
                if (accountFromRequest != null)
                    fromUserRequest = accountFromRequest.Profile.DisplayName;

                var fileName = toUserRequest + ".xlsx";
                var fileSave = HostingEnvironment.MapPath(System.IO.Path.Combine("~/Content/vault/HandShakeRequest/",
                    request.ToUserId, fileName));
                if (System.IO.File.Exists(fileSave))
                    System.IO.File.Delete(fileSave);

                var rootFolder =
                    new System.IO.DirectoryInfo(HostingEnvironment.MapPath("~/Content/vault/HandShakeRequest"));
                if (!rootFolder.Exists)
                    rootFolder.Create();

                var directory = new System.IO.DirectoryInfo(
                    HostingEnvironment.MapPath(System.IO.Path.Combine("~/Content/vault/HandShakeRequest/",
                        request.ToUserId)));

                if (!directory.Exists)
                    directory.Create();

                if (outsite.SendMe == false)
                {
                    email = "";
                }

                if (outsite.Option == "attachment")
                {
                    var contentHtml = "";
                    var filePath = outsite.FromDisplayName + ".xlsx";

                    GenerateRequestToXLS(listRequest, fileSave);
                    SyncListMailHandShakeRequest(fromUserRequest, toUserRequest, email, lstEmail, filePath,
                        contentHtml);
                }
                else if (outsite.Option == "data")
                {
                    var contentHtml = ExportRequestToHtml(listRequest);
                    var filePath = "";

                    SyncListMailHandShakeRequest(fromUserRequest, toUserRequest, email, lstEmail, filePath,
                        contentHtml);
                }
                else if (outsite.Option == "text")
                {
                    var contentHtml = "";
                    var filePath = "";
                    SyncListMailHandShakeRequest(fromUserRequest, toUserRequest, email, lstEmail, filePath,
                        contentHtml);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("Error, send mail hand shake request: " + ex.ToString());
            }
        }

        public void GenerateRequestToXLS(List<Request> request, string filePath)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("MailHandshakeRequest");
                ws.Cells[2, 2].Value = "No.";
                ws.Cells[2, 3].Value = "First Name";
                ws.Cells[2, 4].Value = "Last Name";
                ws.Cells[2, 5].Value = "Email";
                ws.Cells[2, 6].Value = "Phone";
                ws.Cells[2, 7].Value = "Send Date";
                for (int i = 0; i < request.Count; i++)
                {
                    ws.Cells[i + 3, 2].Value = i + 1;
                    ws.Cells[i + 3, 3].Value = request[i].FirstName;
                    ws.Cells[i + 3, 4].Value = request[i].LastName;
                    ws.Cells[i + 3, 5].Value = request[i].Email;
                    ws.Cells[i + 3, 6].Value = request[i].Phone;
                    ws.Cells[i + 3, 7].Value = request[i].CreatedDate.ToShortDateString();
                }
                pck.SaveAs(new System.IO.FileInfo(filePath));
            }
        }

        public void SyncListMailHandShakeRequest(string fromUserRequest, string toUserRequest, string toEmail,
            string[] toListEmail, string filePath, string contentHtml)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;

            emailTemplate = HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_HandShakeRequest.html");

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var subject = string.Format("New Handshake Request from " + fromUserRequest);
                emailContent = emailContent.Replace("[from-user-request]", fromUserRequest);
                emailContent = emailContent.Replace("[to-user-request]", toUserRequest);
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/about/business", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                IMailService mailService = new MailService();
                if (toListEmail.Length > 0)
                {
                    var notiList = new NotificationContent
                    {
                        Title = subject,
                        Body = string.Format(emailContent, ""),
                        SendTo = toListEmail
                    };
                    mailService.SendMailAttachmentAsync(notiList, filePath, contentHtml);
                }

                if (toEmail != "")
                {
                    var noti = new NotificationContent
                    {
                        Title = subject,
                        Body = string.Format(emailContent, ""),
                        SendTo = new[] {toEmail}
                    };
                    mailService.SendMailAttachmentAsync(noti, filePath, contentHtml);
                }
            }
        }

        public void SyncListMailHandShakeRequestUcb(string toEmailAdmin = null, string toEmail = null,
            string fromUser = null, string toUser = null)
        {
            var _accountService = new AccountService();
            var emailTemplate = string.Empty;

            emailTemplate = HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_HandShakeRequest.html");

            string emailContent = string.Empty;
            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                var subject = string.Format("New Handshake Request For " + toUser);
                emailContent = emailContent.Replace("[from-user-request]", fromUser);
                emailContent = emailContent.Replace("[to-user-request]", toUser);

                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/about/business", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);

                IMailService mailService = new MailService();
                var lstEmail = new List<string>();
                var lstEmailBcc = new List<string>();
                if (!string.IsNullOrEmpty(toEmailAdmin))
                    lstEmailBcc.Add(toEmailAdmin);
                if (!string.IsNullOrEmpty(toEmail))
                    lstEmail.Add(toEmail);

                var notificationContent = new NotificationContent()
                {
                    Title = subject,
                    Body = string.Format(emailContent, ""),
                    SendTo = lstEmail.ToArray()
                };

                mailService.SendMailBCCAsync(notificationContent, lstEmailBcc.ToArray());
            }
        }
    }
}