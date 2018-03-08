﻿using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.ManageTokenDevice;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
 using System.Runtime.CompilerServices;
 using System.Threading.Tasks;
using System.Web;
using GH.Core.BlueCode.Entity.AuthToken;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.ManualHandshake;
using GH.Core.BlueCode.Entity.Message;
 using GH.Core.BlueCode.Entity.Notification;
 using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class AuthTokensLogic : IAuthTokensLogic
    {
        private IAccountService _accountService;
        private MongoRepository<AuthToken> _repository;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public AuthTokensLogic()
        {
            _accountService = new AccountService();
            _repository = new MongoRepository<AuthToken>();
        }

        public AuthToken GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            AuthToken rs = null;
            try
            {
                var Id = new ObjectId(id);
                rs = _repository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }

            return rs;
        }

        public AuthToken GetByToken(string tokenStr)
        {
            if (string.IsNullOrEmpty(tokenStr)) return null;
            AuthToken token = null;
            try
            {
                token = _repository.Many(l => l.AccessToken.Equals(tokenStr)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }

            return token;
        }


        public string Insert(AuthToken authToken)
        {
            try
            {
                var token = GetById(authToken.Id.ToString());
                if (token != null)
                {
//                    token.Status = tokenDevice.Status;
                    //                  Update(token);
                }
                else
                {
                    authToken.Id = ObjectId.GenerateNewId();
//                    authToken.Issued = authToken
                    _repository.Add(authToken);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Insert token Id = " + authToken.Id.ToString() + " exception " + ex.ToString());
                return "Error inserting auth token";
            }

            return authToken.Id.ToString();
        }

        public FuncResult InsertToken(AuthToken token)
        {
            var tokenCollection = MongoDBConnection.Database.GetCollection<AuthToken>("AuthToken");
            try
            {
                tokenCollection.InsertOne(token);
            }
            catch
            {
                return new ErrResult("auth.token.add.error");
            }

            return new OkResult("auth.token.add.ok");
        }         
        
        public async Task<FuncResult> InsertTokenAsync(AuthToken token)
        {
            var tokenCollection = MongoDBConnection.Database.GetCollection<AuthToken>("AuthToken");
            try
            {
                await tokenCollection.InsertOneAsync(token);
            }
            catch
            {
                return new ErrResult("auth.token.add.error");
            }

            return new OkResult("auth.token.add.ok");
        }    
        public async Task<FuncResult> CloseTokenAsync(string token)
        {
            var tokenCollection = MongoDBConnection.Database.GetCollection<AuthToken>("AuthToken");
            var builder = Builders<AuthToken>.Filter;
            var filter = builder.Eq("AccessToken", token);
            var result = await tokenCollection.UpdateOneAsync(filter, 
                    Builders<AuthToken>.Update.Set("Status", "closed"));
            return new OkResult("auth.token.close.ok");
        }      
        public async Task<FuncResult> GetFcmTokensByAccountIdAsync(string accountId)
        {
            try
            {
                var tokenCollection = MongoDBConnection.Database.GetCollection<AuthToken>("AuthToken");
                var builder = Builders<AuthToken>.Filter;
                var filter = builder.Eq("AccountId", accountId) & builder.Ne<string>("FcmToken", null)
                            & builder.Eq("Status", "active") & builder.Gt("Expires", DateTime.UtcNow);
                var tokens = tokenCollection.Find(filter).ToList();
                if (tokens.Count == 0)
                    return new ErrResult("auth.tokens.notFound");
                return new OkResult("auth.tokens.list.ok", tokens.Select(t => t.FcmToken).Distinct().ToList());
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                return new ErrResult("auth.tokens.error");
            }
        }
        
        public async Task<FuncResult> PostToFcmAsync(Account account, Account fromAccount, string payloadType, string title, string body,
            object payload)
        {
            const string END_POINT = "https://fcm.googleapis.com/fcm/send";

            var accountId = account.AccountId;
            
            var result = await GetFcmTokensByAccountIdAsync(accountId);
            if (!result.Success)
            {
                return new ErrResult("fcm.token.notFound");
            }
            List<string> tokens = (List<string>) result.Data;
            Log.Debug($"FCM to send to {string.Join(", ", tokens)}");

            
            string payloadInfo = "";

            EmbeddedProfile fromProfile = null;
            
            if (fromAccount != null)
            {
                fromProfile =  new EmbeddedProfile {
                    Id = fromAccount.Id.ToString(),
                    DisplayName= fromAccount.Profile.DisplayName,
                    Avatar = fromAccount.Profile.PhotoUrl
                };
            }
           // var fromProfileJson = JsonConvert.SerializeObject(fromProfile);

            if (payloadType == "message")
            {
                payloadInfo = "message";

                var personalMessage = (PersonalMessage) payload;

                var msg = new ConversationMessage
                {
                    conversationId = personalMessage.ConversationId,
                    messageid = personalMessage.Id.ToString(),
                    read = personalMessage.Read,
                    created = personalMessage.Created,
                    text = personalMessage.Text,
                    fromMe = true,
                    fromAccountId = personalMessage.FromAccountId,
                    isread = personalMessage.IsRead,
                    toReceiverId = personalMessage.ToReceiverId,
                    Datedeleted = personalMessage.DateDelete == DateTime.MinValue
                        ? null
                        : (DateTime?) personalMessage.DateDelete,
                    Isdeleted = personalMessage.IsDelete,
                    type = personalMessage.Type,
                    jsonFieldsdrfi = personalMessage.JsonFieldsdrfi
                };


                if (personalMessage.Type == "drfi")
                    msg.status = personalMessage.Status;
                else if (personalMessage.Type != null && personalMessage.Type.StartsWith("drfiresponse"))
                    msg.drfiRequestId = personalMessage.DrfiRequestId;

                List<FieldinformationVault> fields = null;
                var json = msg.jsonFieldsdrfi;
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                    }
                    catch
                    {
                    }
                }

                if (fields != null)
                {
                    var vaultFields = await VaultService.ListFieldsAsync();
                    var drfiFields = new List<UserDrfiField>();
                    foreach (var field in fields)
                    {
                        var type = field.type;
                        string value = field.model;
                        var valueObj = value == null
                            ? new UserFieldValue
                            {
                                Text = null,
                                List = null,
                                Json = null,
                            }
                            : new UserFieldValue();


                        var optionArray = new object[] { };
                        try
                        {
                            switch (field.options)
                            {
                                case null:
                                    break;
                                case JArray jarray:
                                    optionArray = jarray.ToArray<object>();
                                    break;
                                case string[] optionArr:
                                    optionArray = optionArr.ToArray<object>();
                                    break;
                                case BsonArray bsonArray:
                                    optionArray = bsonArray.ToArray<object>();
                                    break;
                                case List<BsonValue> optionArr:
                                    optionArray = optionArr.ToArray<object>();
                                    break;
                                case string optionStr:
                                    if (string.IsNullOrEmpty(optionStr))
                                        optionArray = new object[] { };
                                    else if (optionStr == "[]")
                                        optionArray = new object[] { };
                                    else
                                        optionArray = new object[] {optionStr};
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            //Log.Debug($"{field.jsPath}: {e.Message}");
                        }

                        if (optionArray.Length == 0)
                        {
                            var vField = vaultFields.FirstOrDefault(f => f.Path == field.jsPath);
                            if (vField != null)
                            {
                                optionArray = vField.Options;
                            }
                        }

                        if (value != null)
                        {
                            try
                            {
                                switch (type)
                                {
                                    case "textbox"
                                        when optionArray.Length > 0 && optionArray[0].ToString() == "phone":
                                        var res = PhoneNumberHelper.GetFormattedPhone(value);
                                        valueObj.Text = res.Success
                                            ? ((FormattedPhone) res.Data).CountryCode + " " +
                                              ((FormattedPhone) res.Data).PhoneNumber
                                            : value;
                                        break;
                                    case "date":
                                    case "datecombo":
                                        if (DateTime.TryParse(value, out var date))
                                            valueObj.Text = date.ToString("yyyy-MM-dd");
                                        break;
                                    case "smartinput":
                                    case "tagsinput":
                                        valueObj.List = value == "[]"
                                            ? new object[] { }
                                            : value.Split(',').ToArray<object>();
                                        break;

                                    case "location":
                                        valueObj.Json = JsonConvert.SerializeObject(new
                                        {
                                            country = field.model,
                                            city = field.unitModel
                                        });
                                        break;
                                    case "numinput":
                                        valueObj.Json = JsonConvert.SerializeObject(new
                                        {
                                            amount = field.model,
                                            unit = field.unitModel
                                        });
                                        break;
                                    default:
                                        if (value != null)
                                            valueObj.Text = value;
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                //Log.Debug($"{field.jsPath}: {e.Message}");
                            }
                        }

                        var pField = new UserDrfiField
                        {
                            Path = field.jsPath,
                            Title = field.displayName,
                            Type = type,
                            Options = optionArray,
                            Value = valueObj
                        };
                        drfiFields.Add(pField);
                    }

                    if (drfiFields.Count > 0)
                        msg.drfiFields = drfiFields;
                }

                payload = msg;

            }

            else
            {
                var notif = (NotificationMessage) payload;
                payloadInfo = notif.Category;
                // = _accountService.GetByAccountId(notif.FromAccountId);
                if (fromAccount != null)
                {
                    notif.FromProfile = fromProfile;
                }
                payload = notif;
            }
            
            string payloadJson = null;
            try
            {
                payloadJson = JsonConvert.SerializeObject(payload);
            }
            catch
            {
                payloadJson = null;
            }

            var serverKeys = new string[]
            {
                ConfigurationManager.AppSettings["fcmkey"],
                "key=AIzaSyD6J_r7GCTwgSIqPC9jAhODYbVcVApGe08"
            };
            var fcmResponses = new List<object>();
            var errors = 0;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var serverKey in serverKeys)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", serverKey);

                    var response = await client.PostAsJsonAsync(END_POINT, new
                    {
                        registration_ids = tokens,
                        notification = new
                        {
                            title,
                            body
                            //fromProfileJson
                            //icon = "http://localhost:9112" + fromAccount?.Profile.PhotoUrl
                        },
                        data = new
                        {
                            type = payloadType,
                            text = body,
                            fromAccountId = fromAccount?.AccountId,
                            fromDisplayName = fromAccount?.Profile.DisplayName,
                            fromAvatar = fromAccount?.Profile.PhotoUrl,
                            json = payloadJson
                        },
                        priority = "high"
                    });

                    var json = await response.Content.ReadAsStringAsync();
                    Log.Debug(json);
                    JObject resp;
                    try
                    {
                        resp = JObject.Parse(json);
                        Log.Debug($"FCM [{payloadInfo}] sent from {fromAccount?.Profile.DisplayName} to {string.Join(", ", tokens)}");
                    }
                    catch
                    {
                        resp = null;
                    }

                    fcmResponses.Add(resp);

                    if (!response.IsSuccessStatusCode)
                        errors++;
                    
                    client.DefaultRequestHeaders.Remove("Authorization");

                }
            }

            if (errors == serverKeys.Length)
                return new ErrResult("fcm.post.error", fcmResponses);
            return new OkResult("fcm.post.ok", fcmResponses);
        }
    }
}