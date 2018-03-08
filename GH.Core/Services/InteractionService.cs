using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.WebPages;
using System.Xml.Linq;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.BlueCode.Entity.ManualHandshake;
using GH.Core.BlueCode.Entity.Notification;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.PostHandShake;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.BlueCode.Entity.Request;
using GH.Core.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.ViewModels;
using GH.Util;
using JavaPort;
using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Excel;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Util;
using Quartz.Xml.JobSchedulingData20;
using RegitSocial.Business.Notification;
using HeaderFooter = NPOI.HSSF.UserModel.HeaderFooter;

namespace GH.Core.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly IAccountService _accountService = new AccountService();
        private readonly IBusinessMemberLogic _businessMemberService = new BusinessMemberLogic();
        private readonly IDelegationBusinessLogic _delegationService = new DelegationBusinessLogic();
        private readonly IMongoCollection<Account> _accountCollection;

        private readonly MongoRepository<Post> _postRepos = new MongoRepository<Post>();
//        private readonly MongoRepository<PostHandShake> _postHsRepos = new MongoRepository<PostHandShake>();

        private readonly IMongoCollection<BsonDocument> _interactionCollection =
            MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

        private static readonly IMongoCollection<InteractionCampaign> InteractionCollection =
            MongoDBConnection.Database.GetCollection<InteractionCampaign>("Campaign");

        private readonly MongoRepository<BusinessMember> _businessMemberRepos = new MongoRepository<BusinessMember>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InteractionService()
        {
        }

        public BsonDocument GetInteractionById(string id)
        {
            //var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var filter = Builders<BsonDocument>.Filter;

            var criteria = filter.Eq("_id", id);
            criteria = criteria & !filter.Eq("campaign.status", "Remove");
            BsonDocument campaign = _interactionCollection.Find(criteria).FirstOrDefault();
            if (campaign == null)
            {
                criteria = filter.Eq("_id", new ObjectId(id));
                criteria = criteria & !filter.Eq("campaign.status", "Remove");

                campaign = _interactionCollection.Find(criteria).FirstOrDefault();
            }

            return campaign;
        }

        public async Task<FuncResult> GetUserInteractionAsync(string id, Account account,
            UserDelegation delegation = null)
        {
            //var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var filter = Builders<BsonDocument>.Filter;

            var criteria = filter.Eq("_id", id);
            criteria = criteria | filter.Eq("_id", ObjectId.Parse(id));
            criteria = criteria & filter.Ne("campaign.status", "Remove");
            BsonDocument campaign = _interactionCollection.Find(criteria).FirstOrDefault();

            if (campaign == null) return new FuncResult(false, "notFound");

            BsonDocument cd = campaign["campaign"].AsBsonDocument;
            var type = cd["type"].AsString.ToLower();
            if (type == "advertising")
            {
                type = "broadcast";
            }

            string businessId = campaign["userId"].AsString;
            Account businessAccount = _accountService.GetByAccountId(businessId);
            if (businessAccount == null)
                return new FuncResult(false, "business.notFound");

            DateTime until = DateTime.MinValue;
            bool indefinite = cd["criteria"]["spend"]["type"].AsString == "Daily";
            if (!indefinite)
                DateTime.TryParse(cd["criteria"]["spend"]["endDate"]?.AsString, out until);

            var interaction = new UserInteractionModel
            {
                id = id,
                type = type,
                name = cd["name"].AsString,
                description = cd["description"].AsString,
                business = new FeedBusinessViewModel
                {
                    id = businessAccount.Id.ToString(),
                    accountId = businessId,
                    avatar = businessAccount.Profile.PhotoUrl,
                    name = businessAccount.Profile.DisplayName,
                },
                image = cd.GetValue("image", null)?.AsString,
            };

            interaction.targetUrl = cd.GetValue("targetLink", null)?.AsString;
            interaction.termsType = cd.GetValue("termsType", null)?.AsString;
            interaction.termsUrl = cd.GetValue("termsUrl", null)?.AsString;
            interaction.verb = cd.GetValue("verb", null)?.AsString;
            interaction.socialShare = cd.GetValue("socialShare", null)?.AsString;
            interaction.paid = cd.GetValue("paid", false).AsBoolean;
            interaction.price = cd.GetValue("price", null)?.AsString;
            interaction.priceCurrency = cd.GetValue("priceCurrency", null)?.AsString;
            interaction.until = until == DateTime.MinValue ? (DateTime?) null : until;

            if (type == "event")
            {
                BsonDocument evt = cd.GetValue("event", null)?.AsBsonDocument;
                if (evt != null)
                {
                    DateTimeOffset.TryParse(evt.GetValue("startdate", null)?.AsString, out var fromDate);
                    DateTimeOffset.TryParse(evt.GetValue("starttime", null)?.AsString, out var fromTime);
                    DateTimeOffset.TryParse(evt.GetValue("enddate", null)?.ToString(), out var toDate);
                    DateTimeOffset.TryParse(evt.GetValue("endtime", null)?.ToString(), out var toTime);

                    var location = evt.GetValue("location", null);
                    var theme = evt.GetValue("theme", null);

                    interaction.eventInfo = new InteractionEventViewModel
                    {
                        fromDate = fromDate == DateTimeOffset.MinValue ? null : fromDate.ToString("yyyy-MM-dd"),
                        fromTime = fromTime == DateTimeOffset.MinValue ? null : fromTime.ToString("HH:mm"),
                        toDate = toDate == DateTimeOffset.MinValue ? null : toDate.ToString("yyyy-MM-dd"),
                        toTime = toTime == DateTimeOffset.MinValue ? null : toTime.ToString("HH:mm"),
                        location = location.IsString ? location.AsString : null,
                        theme = theme.IsString ? theme.AsString : null,
                    };
                }
            }

            var role = delegation?.DelegationRole.ToLower();
            var permissions = delegation?.Permissions;

            List<UserFormField> fields = new List<UserFormField>();

            var baFields = cd["fields"].AsBsonArray.ToList();
            foreach (var baField in baFields)
            {
                var fd = baField.AsBsonDocument;
                var path = fd.GetValue("jsPath", string.Empty).AsString;
                var fieldType = fd.GetValue("type", null)?.AsString;
                var optionsValue = fd.GetValue("options", null);
                object options = optionsValue;
                if (fieldType == "qa")
                {
                    if (fd.GetValue("choices", false).AsBoolean)
                    {
                        var choices = fd.GetValue("model", null)?.AsBsonArray.ToList();
                        options = choices?.Select(q => q["value"]).ToList();
                    }
                }
                else if (fieldType == "range")
                {
                    options = fd.GetValue("model", null);
                }

                var permission = role == null ? "w" : "-";
                if (role != null && (fieldType == "qa" || fieldType == "range"))
                    permission = "w";
                else
                {
                    if (role == "normal")
                        permission = "r";
                    else if (role == "super")
                        permission = "w";
                    else if (role == "custom")
                    {
                        var paths = path.Split('.');
                        if (paths?.Length > 2)
                        {
                            var bucket = paths[1];
                            var delegationPermission =
                                permissions.FirstOrDefault(p => p.jsonpath.Trim('.').StartsWith(bucket));
                            if (delegationPermission != null)
                            {
                                if (delegationPermission.write) permission = "w";
                                else if (delegationPermission.read)
                                    permission = "r";
                            }
                        }
                    }
                }

                var field = new UserFormField
                {
                    Path = path,
                    DisplayName = fd.GetValue("displayName", null)?.AsString,
                    Type = fieldType,
                    Options = optionsValue != null && optionsValue.IsString && fieldType != "range"
                        ? optionsValue.ToString()
                        : options,
                    Optional = fd.GetValue("optional", false).AsBoolean,
                    Permission = permission
                };


                if (path.StartsWith("Custom"))
                    field.Group = "userInformation";
                fields.Add(field);
            }

            List<FormFieldGroup> groups = new List<FormFieldGroup>();
            var baGroups = cd.GetValue("groups", null)?.AsBsonArray.ToList();
            if (baGroups != null)
            {
                foreach (var group in baGroups)
                {
                    var gd = group.AsBsonDocument;
                    var name = gd.GetValue("name", string.Empty).AsString;
                    groups.Add(new FormFieldGroup
                    {
                        Name = name,
                        DisplayName = gd.GetValue("displayName", null)?.AsString
                    });
                    foreach (var field in fields)
                    {
                        if (field.Path.Contains(name))
                            field.Group = name;
                    }
                }
            }

            List<UserFieldData> dFields;

            var partResult =
                await GetParticipation(interaction.id, interaction.type, account); //, delegation?.DelegationId);
            if (partResult.Success)
            {
                var part = (UserParticipation) partResult.Data;
                if (part.Status == "active")
                {
                    interaction.participated = true;
                    interaction.participation = part;
                    dFields = part.UserData;
                }
                else
                {
                    interaction.request = new ParticipationRequest
                    {
                        Comment = part.Comment,
                        Status = part.Status,
                        Created = part.ParticipatedAt
                    };
                    interaction.participated = false;
                    var result = GetUserDataForFields(fields, account);
                    dFields = (List<UserFieldData>) result.Data;
                }
            }
            else
            {
                interaction.participated = false;
                var result = GetUserDataForFields(fields, account);
                dFields = (List<UserFieldData>) result.Data;
            }

            interaction.form = new UserInteractionFormModel
            {
                fields = fields,
                groups = groups,
                userData = dFields
            };

            var dGroups = new List<UserFormGroupWithData>();
            foreach (var group in groups)
            {
                var dGroup = new UserFormGroupWithData()
                {
                    Heading = group.DisplayName,
                    Fields = new List<UserFieldWithData>()
                };
                if (dFields != null)
                {
                    var gFields = fields.Where(f => f.Group == group.Name).ToList();
                    foreach (var gField in gFields)
                    {
                        var fType = fields.FirstOrDefault(f => f.Path == gField.Path)?.Type;
                        var fOptions = fields.FirstOrDefault(f => f.Path == gField.Path)?.Options;
                        var value = dFields.FirstOrDefault(f => f.Path == gField.Path)?.Value;
                        object[] optionArray;
                        var options = gField.Options;
                        switch (gField.Options)
                        {
                            case null:
                                optionArray = new object[] { };
                                break;
                            case string optionStr:
                                if (string.IsNullOrEmpty(optionStr))
                                    optionArray = new object[] { };
                                else
                                    optionArray = new object[] {optionStr};
                                break;
                            case string[] optionArr:
                                optionArray = optionArr;
                                break;
                            case BsonArray bsonArray:
                                optionArray = bsonArray.ToArray<object>();
                                break;
                            case List<BsonValue> optionArr:
                                optionArray = optionArr.ToArray<object>();
                                break;
                            default:
                                optionArray = new object[] { };
                                break;
                        }

                        UserFieldValue valueObj = new UserFieldValue();

                        var dType = gField.Type;

                        if (dType == "range")
                        {
                            var rangeOptions = new List<string>();
                            foreach (var range in optionArray)
                            {
                                var rangeArray = JArray.FromObject(range);
                                rangeOptions.Add(rangeArray[0] + "-" + rangeArray[1] ?? string.Empty);
                            }

                            if (value != null && Int32.TryParse(value.ToString(), out var index))
                            {
                                if (index < rangeOptions.Count)
                                {
                                    valueObj.Text = rangeOptions[index];
                                }
                            }

                            dType = "select";
                            optionArray = rangeOptions.toArray<object>();
                        }

                        if (value != null)
                            switch (fType)
                            {
                                case "textbox" when fOptions?.ToString() == "phone" &&
                                                    !string.IsNullOrEmpty(value.ToString()):
                                    var result = PhoneNumberHelper.GetFormattedPhone(value.ToString());
                                    valueObj.Text = result.Success
                                        ? ((FormattedPhone) result.Data).CountryCode + " " +
                                          ((FormattedPhone) result.Data).PhoneNumber
                                        : value.ToString();
                                    break;
                                /*                          case "select":
                                                              optionArray = (string[]) fOptions;
                                                              break;*/
                                case "smartinput":
                                case "tagsinput":
                                case "doc":
                                    valueObj.List = JArray.FromObject(value).ToArray<object>();
                                    break;
                                case "location":
                                    valueObj.Json = JsonConvert.SerializeObject(value);
                                    break;
                                case "range":
                                    break;

                                default:
                                    valueObj.Text = value.ToString();
                                    break;
                            }


                        var dField = new UserFieldWithData
                        {
                            Path = gField.Path,
                            Title = gField.DisplayName,
                            Type = dType,
                            Options = optionArray,
                            Optional = gField.Optional,
                            Value = valueObj
                        };
                        dGroup.Fields.Add(dField);
                    }
                }

                dGroups.Add(dGroup);
            }

            interaction.formData = dGroups;


            if (type != "broadcast" && type != "handshake")
            {
                var accountId = account.AccountId;
                var delegators = account.Delegations?.Where(d => d.Direction.Equals("DelegationIn")).ToList();
                var delegatees = account.Delegations?.Where(d => d.Direction.Equals("DelegationOut")).ToList();

                interaction.business.following = IsFollowing(accountId, businessId);
                var participations = GetParticipations(account, id, type);
                if (participations != null)
                {
                    interaction.participations = new List<FeedParticipationViewModel>();
                    foreach (var participation in participations)
                    {
                        var actor = "self";
                        var name = "";
                        if (participation.UserId == accountId)
                        {
                            var delegateeId = participation.DelegateeId;
                            if (delegateeId != null && delegateeId != accountId)
                            {
                                actor = "by";
                                var delegatee = delegatees?.FirstOrDefault(d => d.ToAccountId.Equals(delegateeId));
                                name = delegatee?.ToUserDisplayName;
                            }
                        }
                        else
                        {
                            actor = "for";
                            var delegator =
                                delegators?.FirstOrDefault(d => d.FromAccountId.Equals(participation.UserId));
                            name = delegator?.FromUserDisplayName;
                        }

                        var part = new FeedParticipationViewModel
                        {
                            actor = actor,
                            actorName = name
                        };
                        if (participation.DelegationId != null)
                        {
                            part.delegationId = participation.DelegationId;
                        }

                        if (participation.FollowedDate != null)
                        {
                            part.participated = participation.FollowedDate;
                        }

                        interaction.participations.Add(part);
                    }
                }
            }

            return new FuncResult(true, "interaction.ok", interaction);
        }

        public FuncResult GetUserDataForFields(List<UserFormField> formFields, Account account)
        {
            var accountId = account.AccountId;

            var vaultBus = new InfomationVaultBusinessLogic();
            BsonDocument vaultDoc = vaultBus.GetInformationVaultByUserId(accountId);
            if (vaultDoc == null)
                return new FuncResult(false, "vault.error");


            vaultDoc.Remove("_id");
            JObject vault = JObject.Parse(vaultDoc.ToJson());
            //JObject vault = JObject.FromObject(vaultDoc.ToDictionary());
            //JArray vaultObj = (JArray)JToken.FromObject(vaultDoc);

            var list = new List<UserFieldData>();
            var fields = formFields.Where(f => !f.Path.StartsWith("Custom")
                                               && f.Permission != "-" && f.Type != "static")
                .Select(f => new UserDataField
                {
                    Path = f.Path,
                    Type = f.Type,
                    Value = null
                });
            foreach (var field in fields)
            {
                try
                {
                    var path = field.Path;
                    var type = field.Type;

                    var pathTokens = path.Trim('.').Split('.');

                    var fieldData = new UserFieldData
                    {
                        Source = "vaultCurrentValue",
                        Path = path,
                    };

                    var bucket = pathTokens[0];

                    switch (bucket)
                    {
                        case "contact":
                            var contact = vault.SelectToken($"contact.value.{pathTokens[1]}");
                            fieldData.Value = (string) contact?.SelectToken("default");
                            break;

                        case "education":
                        case "employment":
                        case "membership":
                        {
                            var prop = pathTokens[1];
                            var node = vault.SelectToken($"{bucket}");
                            var defaultDesc = (string) node.SelectToken("default");
                            var current = node.SelectToken("value")?.Children().FirstOrDefault(
                                a => a.SelectToken("description")?.ToString() == defaultDesc);
                            switch (prop)
                            {
                                default:
                                    fieldData.Value = RenderPropValue(current, prop, type, true, vault, pathTokens);
                                    break;
                            }

                            break;
                        }
                        case "address":
                        case "financial":
                        case "governmentID":
                        {
                            bucket = bucket == "address" ? "groupAddress" :
                                bucket == "financial" ? "groupFinancial" : "groupGovernmentID";
                            var prop = pathTokens[2];
                            var node = vault.SelectToken($"{bucket}.value.{pathTokens[1]}");
                            var defaultDesc = (string) node.SelectToken("default");
                            var current = node.SelectToken("value")?.Children().FirstOrDefault(
                                a => a.SelectToken("description")?.ToString() == defaultDesc);
                            switch (prop)
                            {
                                case "address":
                                    prop = "addressLine";
                                    break;
                            }

                            fieldData.Value = RenderPropValue(current, prop, type, true, vault, pathTokens);
                            break;
                        }
                        default:
                        {
                            var prop = pathTokens[pathTokens.Length - 1];
                            var tokens = pathTokens.Take(pathTokens.Length - 1);
                            var token = string.Join(".value.", tokens);
                            var node = vault.SelectToken($"{token}");
                            fieldData.Value = RenderPropValue(node, prop, type, false, vault, pathTokens);

                            break;
                        }
                    }

                    //var newField = GetVaultValue(vaultDoc, field);
                    //fieldData.Value = newField.Value;

                    list.Add(fieldData);
                }
                catch (Exception e)
                {
                    Log.Debug($"Error field: {field.Path}");
                }
            }

            return new FuncResult(true, "userFieldData.ok", list);
        }

        private object RenderPropValue(JToken node, string prop, string type, bool isForm,
            JObject vault, string[] pathTokens)
        {
            switch (prop)
            {
                case "location":
                    JToken country, city;
                    if (isForm)
                    {
                        country = node?.SelectToken("country");
                        city = node?.SelectToken("city");
                    }
                    else
                    {
                        country = node?.SelectToken($"value.country.value");
                        city = node?.SelectToken($"value.city.value");
                    }

                    if (country == null && city == null) return null;
                    return new
                    {
                        country,
                        city
                    };
            }

            switch (type)
            {
                case "numinput":
                    var amount = node?.SelectToken($"value.{prop}.value");
                    var unit = node?.SelectToken($"value.{prop}.unit");
                    if (amount == null && unit == null) return null;
                    return new
                    {
                        amount,
                        unit
                    };
                case "doc":
                    var form = pathTokens[pathTokens.Length - 2];
                    switch (form)
                    {
                        case "passportID":
                            form = "Passport ID";
                            break;
                        case "birthCertificate":
                            form = "Birthday Certificate";
                            break;
                        case "nationalID":
                            form = "National ID";
                            break;
                    }

                    var index = node?.BeforeSelf()?.Count();
                    if (index != null) form = $"{form}/{++index}";
                    var vaultDocs = vault.SelectToken("document.value")?.Children();
                    var docs = vaultDocs?
                        .Where(d => ((string) d.SelectToken("jsPath")).ToLower().Contains(form.ToLower()))?
                        .Select(d => new
                        {
                            id = d["_id"],
                            docPath = $".{string.Join(".", pathTokens)}.{index}",
                            fileName = d["name"],
                            filePath = $"/Content/vault/documents/{vault.SelectToken("userId")}"
                        });
                    return docs;
            }

            var value = isForm ? node?.SelectToken($"{prop}") : node?.SelectToken($"value.{prop}.value");
            if (value == null) return null;

            switch (value.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.Date:
                    return string.IsNullOrEmpty(value.ToString()) ? null : ((DateTime) value).ToString("yyyy-MM-dd");

                case JTokenType.Array:
                    if (((JArray) value).Count == 0) return null;
                    return value;

                default:
                    switch (prop)
                    {
                        case "dob":
                        case "issuedDate":
                        case "expiryDate":
                            return string.IsNullOrEmpty(value.ToString())
                                ? null
                                : ((DateTime) value).ToString("yyyy-MM-dd");
                    }

                    switch (type)
                    {
                        case "date":
                        case "datecombo":
                            return string.IsNullOrEmpty(value.ToString())
                                ? null
                                : ((DateTime) value).ToString("yyyy-MM-dd");
                    }

                    return value;
            }
        }

        public async Task<FuncResult> GetParticipation(string interactionId, Account account)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId);
            var campaign = await InteractionCollection.Find(filter).FirstOrDefaultAsync();
            if (campaign == null) return new ErrResult("interaction.notFound");
            return await GetParticipation(interactionId, campaign.Interaction.Type.ToLower(), account);
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class UserFollowedBusiness
        {
            public string Id { get; set; }
            public string AccountId { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
            public bool Followed { get; set; }
            public string FollowedAt { get; set; }
        }

        public async Task<FuncResult> ListFollowedBusinessesAsync(Account account)
        {
            var accountId = account.AccountId;

            var memberCollection = MongoDBConnection.Database.GetCollection<BusinessMember>("BusinessMember");
            var builder = Builders<BusinessMember>.Filter;
            var builderMember = Builders<Follower>.Filter;
            var filter = builder.ElemMatch("Members", builderMember.Eq("UserId", accountId));
            var businessList = await memberCollection.Find(filter).ToListAsync();
            var businesses = new List<UserFollowedBusiness>();
            foreach (var b in businessList)
            {
                var businessAccount = _accountService.GetByAccountId(b.BusinessUserId);
                if (businessAccount == null || businessAccount.AccountType != AccountType.Business) continue;
                var follower = b.Members.FirstOrDefault(f => f.UserId == accountId);
                if (follower == null) continue;
                var business = new UserFollowedBusiness
                {
                    Id = businessAccount.Id.ToString(),
                    AccountId = businessAccount.AccountId,
                    DisplayName = businessAccount.Profile.DisplayName,
                    Avatar = businessAccount.Profile.PhotoUrl,
                    Followed = true,
                };
                if (DateTimeOffset.TryParse(follower.FollowedDate, out var followedAt))
                    business.FollowedAt = followedAt.ToString("o");
                else
                    business.FollowedAt = follower.FollowedDate;

                businesses.Add(business);
            }

            return new OkResult("followed.businesses.list", businesses);
        }

        public async Task<FuncResult> GetParticipation(string interactionId, string interactionType, Account account,
            string delegationId = null)
        {
            var accountId = account.AccountId;

            List<FieldinformationVault> fields = null;

            DateTime participatedAt;
            bool pending = false;
            string comment = null;
            string delegateeId = null;

            if (interactionType == "handshake")
            {
                var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.Eq("UserId", account.AccountId);
                var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null) return new ErrResult("notFound");
                pending = post.IsJoin != "1";
                participatedAt = post.ParticipatedAt ?? post.Timestamp ?? DateTime.UtcNow;
                if (!pending)
                {
                    var json = post.FieldsJson;
                    try
                    {
                        fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                    }
                    catch
                    {
                        fields = new List<FieldinformationVault>(); //post.Fields?.ToList();
                    }
                }
            }
            else
            {
                var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var builderFollower = Builders<Follower>.Filter;
                var filterFollower = builderFollower.Eq("UserId", accountId);
                if (!string.IsNullOrEmpty(delegationId))
                    filterFollower &= builderFollower.Eq("DelegationId", delegationId);
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.ElemMatch("Followers", filterFollower);
                var post = await legacyPostCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null) return new ErrResult("notFound");
                var follower = post.Followers.FirstOrDefault(f => f.UserId == accountId);
                if (follower == null) return new ErrResult("notFound");
                pending = follower.Status == "Invite";
                comment = follower.Comment;
                delegationId = follower.DelegationId;
                delegateeId = follower.DelegateeId;
                if (follower.ParticipatedAt != null)
                    participatedAt = follower.ParticipatedAt ?? DateTime.MinValue;
                else
                    DateTime.TryParse(follower.FollowedDate, out participatedAt);
                if (!pending)
                {
                    fields = follower.fields?.ToList();
                }
            }

            var participation = new UserParticipation
            {
                InteractionId = interactionId,
                AccountId = accountId,
                ParticipatedAt = participatedAt,
                Comment = comment,
                Status = pending ? "pending" : "active"
            };
            if (!string.IsNullOrEmpty(delegationId))
            {
                participation.DelegationId = delegationId;
                participation.DelegateeId = delegateeId;
            }

            if (fields != null)
                foreach (var field in fields)
                {
                    var type = field.type;
                    string value = field.model;
                    object newValue = value;

                    switch (type)
                    {
                        case "date":
                        case "datecombo":
                            if (DateTime.TryParse(value, out var date))
                                newValue = date.ToString("yyyy-MM-dd");
                            else
                                newValue = null;
                            //newValue = value.Split(' ')[0];
                            break;
                        case "smartinput":
                        case "tagsinput":
                            newValue = value.Split(',');
                            break;

                        case "location":
                            newValue = new
                            {
                                country = field.model,
                                city = field.unitModel
                            };
                            break;
                        case "numinput":
                            newValue = new
                            {
                                amount = field.model,
                                unit = field.unitModel
                            };
                            break;

                        case "doc":
                            var fnames = JsonConvert.DeserializeObject<List<string>>(field.modelarraysstr);
                            var docs = new ArrayList();
                            foreach (var fname in fnames)
                            {
                                docs.Add(new
                                {
                                    fileName = fname,
                                    filePath = field.pathfile
                                });
                            }

                            newValue = docs;
                            break;

                        case "range":
                            break;
                    }

                    var pField = new UserFieldData
                    {
                        Source = "submittedValue",
                        Path = field.jsPath,
                        Value = newValue
                    };
                    participation.UserData.Add(pField);
                }

            return new OkResult("participation.ok", participation);
        }

        public async Task<bool> IsParticipated(string interactionId, Account account)
        {
            var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
            var builderPost = Builders<Post>.Filter;
            var builderFollower = Builders<Follower>.Filter;
            var filterPost = builderPost.Eq("CampaignId", interactionId)
                             & builderPost.ElemMatch("Followers", builderFollower.Eq("UserId", account.AccountId));
            var existingPost = await legacyPostCollection.Find(filterPost).FirstOrDefaultAsync();
            return existingPost != null;
        }

        public async Task<FuncResult> ListHandshakesAsync(Account account, string include = "")
        {
            bool includeTerminated = include.ToLower() == "terminated";
            var postHandshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
            var builderPost = Builders<PostHandshake>.Filter;
            var filterPost = builderPost.Eq("UserId", account.AccountId);
            var posts = await postHandshakeCollection.Find(filterPost).ToListAsync();
            if (!posts.Any())
                return new OkResult("handshakes.notFound");
            var handshakes = new List<UserHandshake>();
            foreach (var post in posts)
            {
                if (!includeTerminated && post.Status == "Terminate") continue;
                var hs = new UserHandshake
                {
                    Id = post.Id,
                    InteractionId = post.CampaignId,
                    AccountId = post.UserId,
                    BusinessId = post.BusId,
                };
                if (post.Status == "Terminate")
                {
                    hs.UserStatus = "terminated";
                }
                else
                {
                    var status = "active";
                    if (post.IsAccept == "0")
                    {
                        status = "pending";
                    }
                    else if (post.IsJoin == "0")
                    {
                        status = "paused";
                    }

                    hs.UserStatus = status;
                    if (status == "active")
                    {
                        hs.Status = "standby";
                        if (post.IsChange == "1" && post.Status == "Not acknowledged")
                            hs.Status = "sent";
                        hs.LastUpdated = post.DateJson;
                    }
                }

                var result = await GetUserInteractionAsync(post.CampaignId, account);
                if (result.Success)
                {
                    hs.Interaction = (UserInteractionModel) result.Data;
                }

                handshakes.Add(hs);
            }

            return new OkResult("handshakes.list", handshakes);
        }

        public async Task<FuncResult> AddPersonalHandshakeAsync(PersonalHandshakePostModel model, Account account)
        {
            var accountId = account.AccountId;
            var handshakeCollection = MongoDBConnection.Database.GetCollection<ManualHandshake>("ManualHandshake");
            var builderPost = Builders<ManualHandshake>.Filter;
            var filterPost = builderPost.Eq("accountId", accountId);
            bool isPublic = string.IsNullOrEmpty(model.ToAccountId);
            if (!isPublic)
                filterPost &= builderPost.Eq("toAccountId", model.ToAccountId);
            else
                filterPost &= builderPost.Eq("toEmail", model.ToEmail);
            var ePost = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (ePost != null)
                return new ErrResult("handshake.exists", ePost);

            DateTime expires = DateTime.MinValue;
            bool indefinite = string.IsNullOrEmpty(model.Expires);
            if (!indefinite)
            {
                if (!DateTime.TryParse(model.Expires, out expires))
                    return new ErrResult("handshake.expiry.invalid");
            }

            var nf = model.NotifyFormat;
            if (string.IsNullOrEmpty(nf)) nf = "notif";
            else nf = nf == "values" ? nf : "notif";
            var hs = new ManualHandshake
            {
                Id = ObjectId.GenerateNewId(),
                accountId = accountId,
                toAccountId = model.ToAccountId,
                description = model.Description,
                notifyFormat = nf,
                name = account.Profile.DisplayName,
                email = account.Profile.Email,
                expiry = new Expiry
                {
                    indefinite = indefinite,
                    date = expires
                },
                status = "active",
                CreatedDate = DateTime.UtcNow,
            };
            Account toAccount = null;
            if (isPublic)
            {
                hs.toName = model.ToName;
                hs.toEmail = model.ToEmail;
            }
            else
            {
                toAccount = _accountService.GetByAccountId(model.ToAccountId);
                if (toAccount == null) return new ErrResult("account.notFound");
                if (string.IsNullOrEmpty(model.ToName))
                    hs.toName = toAccount.Profile.DisplayName;
                else
                    hs.toName = model.ToName;
                if (string.IsNullOrEmpty(model.ToEmail))
                    hs.toEmail = toAccount.Profile.Email;
                else
                    hs.toEmail = model.ToEmail;
            }

            hs.fields = new List<FieldManualHandshake>();
            foreach (var path in model.FieldPaths)
            {
                string label;
                var jsPath = path;
                switch (path)
                {
                    case "address.currentAddress":
                        label = "Current Address";
                        jsPath = "." + path;
                        break;
                    case "address.mailingAddress":
                        label = "Current Address";
                        jsPath = "." + path;
                        break;
                    case "contact.mobile":
                        label = "Mobile Number";
                        break;
                    case "contact.office":
                        label = "Office Phone Number";
                        break;
                    case "contact.email":
                        label = "Personal Email";
                        break;
                    case "contact.officeEmail":
                        label = "Work Email";
                        break;
                    default: continue;
                }

                hs.fields.Add(new FieldManualHandshake
                {
                    jsPath = jsPath,
                    label = label,
                    selected = true
                });
            }

            if (hs.fields.Count == 0)
                return new ErrResult("handshake.fields.invalid");

            await handshakeCollection.InsertOneAsync(hs);

            return new OkResult("handshake.invidual.created", hs.Id.ToString());
        }


        public async Task<FuncResult> ListPersonalHandshakesAsync(Account account, string filter = "all",
            string include = "")
        {
            filter = filter.ToLower();
            bool includeTerminated = include.ToLower() == "terminated";
            var accountId = account.AccountId;
            var postHandshakeCollection = MongoDBConnection.Database.GetCollection<ManualHandshake>("ManualHandshake");
            var builderPost = Builders<ManualHandshake>.Filter;
            var filterPost = filter == "in"
                ? builderPost.Eq("toAccountId", accountId)
                : builderPost.Eq("accountId", accountId);
            if (filter == "all")
                filterPost |= builderPost.Eq("toAccountId", accountId);
            var posts = await postHandshakeCollection.Find(filterPost).ToListAsync();
            if (!posts.Any())
                return new OkResult("handshakes.notFound");
            var handshakes = new List<UserPersonalHandshake>();
            foreach (var post in posts)
            {
                if (!includeTerminated && post.status == "terminate") continue;
                var toId = post.toAccountId;
                var hs = new UserPersonalHandshake
                {
                    Id = post.Id.ToString(),
                    AccountId = post.accountId,
                    ToAccountId = post.toAccountId,
                    Direction = post.accountId == accountId ? "out" : "in"
                };
                if (toId == null)
                {
                    hs.ToName = post.toName;
                    hs.ToEmail = post.toEmail;
                }

                if (post.status == "terminate")
                {
                    hs.UserStatus = "terminated";
                }
                else
                {
                    hs.UserStatus = post.status;
                    hs.Expires = post.expiry.indefinite ? "Indefinite" : post.expiry.date.ToString("YYYY-MM-dd");
                    if (post.synced != DateTime.MinValue)
                        hs.LastUpdated = post.synced.ToString("o");
                }

                var otherId = post.accountId;
                if (otherId == account.AccountId)
                    otherId = post.toAccountId;
                if (otherId != null)
                {
                    var other = _accountService.GetByAccountId(otherId);
                    if (other == null) continue;
                    hs.WithProfile = new EmbeddedProfile
                    {
                        Id = other.Id.ToString(),
                        DisplayName = other.Profile.DisplayName,
                        Avatar = other.Profile.PhotoUrl
                    };
                }

                handshakes.Add(hs);
            }

            return new OkResult("handshakes.invidual.list", handshakes);
        }

        public async Task<FuncResult> ListHandshakeRequestsAsync(Account account)
        {
            var accountId = account.AccountId;
            var hsrCollection = MongoDBConnection.Database.GetCollection<Request>("Request");
            var builder = Builders<Request>.Filter;
            var filter = builder.Eq("FromUserId", accountId) & builder.Ne("Status", "Delete");
            var hsRequests = new List<UserHandshakeRequest>();

            var reqs = await hsrCollection.Find(filter).ToListAsync();
            if (reqs.Count == 0)
                return new OkResult("hsRequests.notFound", hsRequests);
            var ucbService = new UserCreatedBusinessService();
            foreach (var req in reqs)
            {
                try
                {
                    var toId = req.ToUserId;
                    var type = req.Type;
                    var hsr = new UserHandshakeRequest
                    {
                        Id = req.Id.ToString(),
                        FromAccountId = req.FromUserId,
                        FirstName = req.FirstName,
                        LastName = req.LastName,
                        PhoneNumber = req.Phone,
                        Email = req.Email,
                        CreatedAt = req.CreatedDate.ToString("o"),
                        Message = req.Message,
                        Status = req.Status == "Complete" ? "completed" : "sent"
                    };
                    if (type == "ucb")
                    {
                        hsr.ToUcbId = toId;
                        var ucb = ucbService.GetUcbById(toId);
                        if (ucb == null) continue;
                        hsr.ToProfile = new EmbeddedProfile
                        {
                            Id = toId,
                            DisplayName = ucb.Name,
                            Avatar = ucb.Avatar
                        };
                    }
                    else
                    {
                        hsr.ToAccountId = toId;
                        var other = _accountService.GetByAccountId(toId);
                        if (other == null) continue;
                        hsr.ToProfile = new EmbeddedProfile
                        {
                            Id = other.Id.ToString(),
                            DisplayName = other.Profile.DisplayName,
                            Avatar = other.Profile.PhotoUrl
                        };
                    }

                    hsRequests.Add(hsr);
                }
                catch (Exception e)
                {
                    Log.Debug($"Handshake request #{req.Id}: {e.Message} : {e.Source}");
                }
            }

            return new OkResult("hsrequests.list", hsRequests);
        }

        public async Task<FuncResult> ListHandshakeRequestsByBusinessAsync(Account account)
        {
            var accountId = account.AccountId;
            var hsrCollection = MongoDBConnection.Database.GetCollection<Request>("Request");
            var builder = Builders<Request>.Filter;
            var filter = builder.Eq("ToUserId", accountId) & builder.Ne("Status", "Delete");
            var reqs = await hsrCollection.Find(filter).ToListAsync();
            if (reqs.Count == 0)
                return new OkResult("hsRequests.notFound", reqs);
            var hsRequests = new List<UserHandshakeRequest>();
            foreach (var req in reqs)
            {
                try
                {
                    var fromId = req.FromUserId;
                    var hsr = new UserHandshakeRequest
                    {
                        Id = req.Id.ToString(),
                        FromAccountId = fromId,
                        ToAccountId = req.ToUserId,
                        FirstName = req.FirstName,
                        LastName = req.LastName,
                        PhoneNumber = req.Phone,
                        Email = req.Email,
                        CreatedAt = req.CreatedDate.ToString("o"),
                        Message = req.Message,
                        Status = req.Status == "Complete" ? "completed" : "sent"
                    };
                    var other = _accountService.GetByAccountId(fromId);
                    if (other == null) continue;
                    hsr.ToProfile = new EmbeddedProfile
                    {
                        Id = other.Id.ToString(),
                        DisplayName = other.Profile.DisplayName,
                        Avatar = other.Profile.PhotoUrl
                    };

                    hsRequests.Add(hsr);
                }
                catch (Exception e)
                {
                    Log.Debug($"Handshake request #{req.Id}: {e.Message} : {e.StackTrace}");
                }
            }

            return new OkResult("hsrequests.list", hsRequests);
        }

        public async Task<FuncResult> RemoveHandshakeRequestAsync(string hsrId, string accountId)
        {
            if (!ObjectId.TryParse(hsrId, out var objId))
                return new ErrResult("handshake.request.invalid");
            var hsrCollection = MongoDBConnection.Database.GetCollection<Request>("Request");
            var builder = Builders<Request>.Filter;
            var filter = builder.Eq("Id", objId);
            var req = await hsrCollection.Find(filter).FirstOrDefaultAsync();
            if (req == null)
                return new ErrResult("handshake.request.notFound");
            if (req.FromUserId != accountId)
                return new ErrResult("handshake.request.notAuthorized");

            var update = Builders<Request>.Update.Set("Status", "Delete");
            var result = await hsrCollection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged)
                return new ErrResult("handshake.request.remove.error");

            return new OkResult("handshake.request.remove.ok", req.ToUserId);
        }

        public async Task<FuncResult> AddHandshakeRequestAsync(HandshakeRequestPostModel model, Account account)
        {
            var accountId = account.AccountId;
            var hsrCollection = MongoDBConnection.Database.GetCollection<Request>("Request");
            var builder = Builders<Request>.Filter;
            bool isUcb = string.IsNullOrEmpty(model.ToAccountId);
            var filter = builder.Eq("ToUserId", isUcb ? model.ToUcbId : model.ToAccountId);
            filter &= builder.Ne("Status", "Delete");
            var existingHsr = await hsrCollection.Find(filter).FirstOrDefaultAsync();
            if (existingHsr != null && existingHsr.Status == null)
                return new ErrResult("handshake.request.exists", existingHsr);

            var req = new Request
            {
                Id = ObjectId.GenerateNewId(),
                FromUserId = accountId,
                ToUserId = isUcb ? model.ToUcbId : model.ToAccountId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.PhoneNumber,
                Email = model.Email,
                Message = model.Message,
                CreatedDate = DateTime.UtcNow
            };
            if (isUcb)
            {
                req.Type = "ucb";
            }

            await hsrCollection.InsertOneAsync(req);

            if (!isUcb)
            {
                var toAccount = _accountService.GetByAccountId(model.ToAccountId);
                if (toAccount != null)
                {
                    var notificationMessage = new NotificationMessage
                    {
                        Type = EnumNotificationType.NotifyRequestdHandshake,
                        FromAccountId = accountId,
                        FromUserDisplayName = account.Profile.DisplayName,
                        ToAccountId = model.ToAccountId,
                        ToUserDisplayName = toAccount.Profile.DisplayName,
                        PreserveBag = req.Id.ToString()
                    };
                    new NotificationBusinessLogic().SendNotification(notificationMessage);
                }
            }
            else
            {
                var sendEmailAdmin = ConfigurationManager.AppSettings["IsSendEmailTemporaryBusiness"];
                if (sendEmailAdmin == "true")
                {
                    var emailAdmin = ConfigurationManager.AppSettings["EmailAdmin"];
                    if (!string.IsNullOrEmpty(emailAdmin))
                        new PostHandShakeBusinessLogic().SyncListMailHandShakeRequestUcb(emailAdmin, "",
                            account.Profile.DisplayName, "UCB");
                }
            }

            return new OkResult("handshake.request.created", req.Id.ToString());
        }

        public async Task<FuncResult> UpdateHandshakeStatusAsync(string interactionId, string status, Account account)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
            var builderPost = Builders<PostHandshake>.Filter;
            var filterPost = builderPost.Eq("CampaignId", interactionId) & builderPost.Eq("UserId", account.AccountId);
            ;
            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null || post.Status == "Terminate")
                return new ErrResult("handshake.notFound");
            if (post.IsAccept == "0")
                return new ErrResult("handshake.pending");
            var isJoin = status == "paused" ? "0" : "1";
            var update = Builders<PostHandshake>.Update.Set("IsJoin", isJoin);
            var result = await handshakeCollection.UpdateOneAsync(filterPost, update);

            if (result.IsAcknowledged && result.ModifiedCount == 0)
                return new ErrResult("handshake.status.update.not");
            if (!result.IsAcknowledged)
                return new ErrResult("handshake.status.update.error");
            var notificationMessage = new NotificationMessage
            {
                Type = status == "paused"
                    ? EnumNotificationType.NotifyPauseHandshake
                    : EnumNotificationType.NotifyResumeHandshake,
                FromAccountId = account.AccountId,
                FromUserDisplayName = account.Profile.DisplayName,
                ToAccountId = post.BusId,
                ToUserDisplayName = "",
                PreserveBag = interactionId
            };
            new NotificationBusinessLogic().SendNotification(notificationMessage);

            return new OkResult("handshake.status.update.ok", post.Id);
        }

        public async Task<FuncResult> TerminateHandshakeAsync(string interactionId, Account account)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
            var builderPost = Builders<PostHandshake>.Filter;
            var filterPost = builderPost.Eq("CampaignId", interactionId) & builderPost.Eq("UserId", account.AccountId);
            ;
            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null)
                return new ErrResult("handshake.notFound");
            var update = Builders<PostHandshake>.Update.Set("Status", "Terminate");
            var result = await handshakeCollection.UpdateOneAsync(filterPost, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("handshake.terminate.error");
            update = Builders<PostHandshake>.Update.Set("IsJoin", "0");
            await handshakeCollection.UpdateOneAsync(filterPost, update);
            update = Builders<PostHandshake>.Update.Set("DateTerminate", DateTime.UtcNow);
            await handshakeCollection.UpdateOneAsync(filterPost, update);

            var notificationMessage = new NotificationMessage
            {
                Type = EnumNotificationType.NotifyTerminateHandshake,
                FromAccountId = account.AccountId,
                FromUserDisplayName = account.Profile.DisplayName,
                ToAccountId = post.BusId,
                ToUserDisplayName = "",
                PreserveBag = interactionId
            };
            new NotificationBusinessLogic().SendNotification(notificationMessage);

            return new OkResult("handshake.terminate.ok", post.Id);
        }

        public async Task<FuncResult> TerminatePersonalHandshakeAsync(string handshakeId, Account account)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<ManualHandshake>("ManualHandshake");
            var builderPost = Builders<ManualHandshake>.Filter;
            if (!ObjectId.TryParse(handshakeId, out var objId))
                return new ErrResult("handshake.invalid");

            var filterPost = builderPost.Eq("Id", objId);

            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null)
                return new ErrResult("handshake.notFound");
            var update = Builders<ManualHandshake>.Update.Set("status", "terminate");
            var result = await handshakeCollection.UpdateOneAsync(filterPost, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("handshake.terminate.error");

            var otherId = post.accountId;
            if (otherId == account.AccountId)
                otherId = post.toAccountId;

            var notificationMessage = new NotificationMessage
            {
                Type = EnumNotificationType.NotifyTerminateIndividualHandshake,
                FromAccountId = account.AccountId,
                FromUserDisplayName = account.Profile.DisplayName,
                ToAccountId = otherId,
                ToUserDisplayName = "",
                PreserveBag = handshakeId
            };
            new NotificationBusinessLogic().SendNotification(notificationMessage);

            return new OkResult("handshake.terminate.ok", otherId);
        }


        public async Task<FuncResult> UpdateStatusPersonalHandshakeAsync(string handshakeId, string status,
            Account account)
        {
            if (!ObjectId.TryParse(handshakeId, out var objId))
                return new ErrResult("handshake.invalid");

            var handshakeCollection = MongoDBConnection.Database.GetCollection<ManualHandshake>("ManualHandshake");
            var builderPost = Builders<ManualHandshake>.Filter;
            var filterPost = builderPost.Eq("Id", objId);
            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null)
                return new ErrResult("handshake.notFound");
            if (post.status == "terminate")
                return new ErrResult("handshake.terminated");
            if (status == "blocked" && post.toAccountId != account.AccountId)
                return new ErrResult("handshake.recipient.invalid");
            if (status == "active" && post.status == "blocked" && post.toAccountId != account.AccountId)
                return new ErrResult("handshake.recipient.invalid");

            var update = Builders<ManualHandshake>.Update.Set("status", status);
            var result = await handshakeCollection.UpdateOneAsync(filterPost, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("handshake.status.update.not");

            var otherId = post.accountId;
            if (otherId == account.AccountId)
                otherId = post.toAccountId;

            return new OkResult("handshake.update.ok", otherId);
        }

        public async Task<FuncResult> RemoveHandshakeAsync(string interactionId, Account account)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
            var builderPost = Builders<PostHandshake>.Filter;
            var filterPost = builderPost.Eq("CampaignId", interactionId) & builderPost.Eq("UserId", account.AccountId);

            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null)
                return new ErrResult("handshake.notFound");
            if (post.Status.ToLower() != "terminate")
                return new ErrResult("handshake.notTerminated");
            var result = await handshakeCollection.DeleteOneAsync(filterPost);
            if (!result.IsAcknowledged || result.DeletedCount == 0)
                return new ErrResult("handshake.remove.error");

            return new OkResult("handshake.remove.ok", post.Id);
        }

        public async Task<FuncResult> RemovePersonalHandshakeAsync(string handshakeId, Account account)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<ManualHandshake>("ManualHandshake");
            var builderPost = Builders<ManualHandshake>.Filter;
            if (!ObjectId.TryParse(handshakeId, out var objId))
                return new ErrResult("handshake.invalid");

            var filterPost = builderPost.Eq("Id", objId);
            var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
            if (post == null)
                return new ErrResult("handshake.notFound");

            if (post.status.ToLower() != "terminate")
                return new ErrResult("handshake.notTerminated");
            var result = await handshakeCollection.DeleteOneAsync(filterPost);
            if (!result.IsAcknowledged || result.DeletedCount == 0)
                return new ErrResult("handshake.remove.error");

            var otherId = post.accountId;
            if (otherId == account.AccountId)
                otherId = post.toAccountId;
            return new OkResult("handshake.remove.ok", otherId);
        }

        public async Task<FuncResult> Unregister(string interactionId, Account account,
            UserDelegation delegation = null)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId) & builder.Ne("Interaction.Status", "Remove");

            var campaign = await InteractionCollection.Find(filter).FirstOrDefaultAsync();

            if (campaign == null) return new ErrResult("interaction.notFound");

            Interaction interaction = campaign.Interaction;
            var iType = interaction.Type;
            if (iType == "Handshake")
            {
                return new ErrResult("unregister.handshake.na");
            }

            var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
            var builderPost = Builders<Post>.Filter;
            var builderFollower = Builders<Follower>.Filter;
            var filterPost = builderPost.Eq("CampaignId", interactionId)
                             & builderPost.ElemMatch("Followers", builderFollower.Eq("UserId", account.AccountId));
            if (!await legacyPostCollection.Find(filterPost).AnyAsync())
                return new ErrResult("interaction.notParticipated");

            var result = await legacyPostCollection.UpdateOneAsync(filterPost,
                Builders<Post>.Update.PullFilter("Followers", builderFollower.Eq("UserId", account.AccountId)));
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return new ErrResult("unregister.update.error");

            return new OkResult("unregister.ok");
        }

        public async Task<FuncResult> Register(string interactionId, Account account,
            IList<UserFieldData> fields, UserDelegation delegation = null)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId); // | builder.Eq("_id", ObjectId.Parse(interactionId));
            filter &= builder.Ne("Interaction.Status", "Remove");

            var campaign = await InteractionCollection.Find(filter).FirstOrDefaultAsync();

            if (campaign == null) return new FuncResult(false, "interaction.notFound");

            Interaction interaction = campaign.Interaction;
            var iType = interaction.Type;

            string businessAccountId = campaign.UserId;
            var businessAccount = _accountService.GetByAccountId(businessAccountId);

            IMongoCollection<Post> legacyPostCollection = null;
            IMongoCollection<PostHandshake> postHandshakeCollection = null;

            Post pendingPost = null;
            Follower pendingFollower = null;
            PostHandshake pendingHandshake = null;

            if (iType == "Handshake")
            {
                postHandshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.Eq("UserId", account.AccountId);
                var existingPost = await postHandshakeCollection.Find(filterPost).FirstOrDefaultAsync();
                if (existingPost != null)
                {
                    if (existingPost.Status == "Terminate")
                        return new ErrResult("handshake.terminated");
                    if (existingPost.IsAccept != "1")
                        pendingHandshake = existingPost;
                    else
                        return new ErrResult("interaction.participated");
                }
            }
            else
            {
                legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var builderFollower = Builders<Follower>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.ElemMatch("Followers", builderFollower.Eq("UserId", account.AccountId));
                var existingPost = await legacyPostCollection.Find(filterPost).FirstOrDefaultAsync();
                if (existingPost != null)
                {
                    var follower = existingPost.Followers.FirstOrDefault(f => f.UserId == account.AccountId);
                    if (follower?.Status == "Invite")
                    {
                        pendingPost = existingPost;
                        pendingFollower = follower;
                    }
                    else
                        return new ErrResult("interaction.participated");
                }
            }

            var updateFields = new List<UserField>();
            var postFields = new List<ParticipationField>();
            var legacyFields = new List<FieldinformationVault>();

            var role = delegation?.DelegationRole.ToLower();
            var permissions = delegation?.Permissions;

            foreach (var iField in interaction.Fields)
            {
                var type = iField.Type;
                if (type == "static") continue;
                var field = fields.FirstOrDefault(f => f.Path == iField.JsPath);

                var fieldFullName = $"{iField.DisplayName} ({iField.JsPath})";

                if (field == null)
                {
                    if (iField.Optional) continue;
                    return new FuncResult(false, "field.missing", fieldFullName);
                }

                if (field.Value == null)
                    return new FuncResult(false, "field.null", fieldFullName);

                var value = field.Value;

                // Validate delegated permissions

                if (role != null && role != "super" && type != "qa" && type != "range")
                {
                    if (role == "normal")
                    {
                        if (field.Source == "newValue")
                            return new ErrResult("field.readonly", fieldFullName);
                    }
                    else if (role == "custom")
                    {
                        bool readable = false, writable = false;
                        var paths = iField.JsPath.Split('.');
                        if (paths?.Length > 1)
                        {
                            var bucket = paths[1];
                            var delegationPermission =
                                permissions.FirstOrDefault(p => p.jsonpath.Trim('.').StartsWith(bucket));
                            if (delegationPermission != null)
                            {
                                if (delegationPermission.write) writable = readable = true;
                                else if (delegationPermission.read)
                                    readable = true;
                            }
                        }

                        if (!readable)
                            return new ErrResult("field.forbidden", fieldFullName);
                        if (field.Source == "newValue" && !writable)
                            return new ErrResult("field.readonly", fieldFullName);
                    }
                }

                //    Validate field values
                switch (type)
                {
                    case "textbox":
                    case "radio":
                    case "checkbox":
                    case "select":
                    case "qa":
                    case "address":
                        if (!(value is string stringValue))
                            return new ErrResult("field.invalid", $"{fieldFullName} <{type}> expects string");
                        if (string.IsNullOrEmpty(stringValue))
                            return new ErrResult("field.null", $"{fieldFullName} empty string");
                        if (type == "textbox" && iField.Options.Equals("phone"))
                        {
                            value = "+" + stringValue.Trim('+');
                        }

                        break;
                    case "range":
                        if (!(value is string stringVal))
                            return new ErrResult("field.invalid", $"{fieldFullName} <select> expects string");
                        if (string.IsNullOrEmpty(stringVal))
                            return new ErrResult("field.null", $"{fieldFullName} empty string");

                        if (iField.Model is List<object> optionArray)
                        {
                            if (!Int32.TryParse(stringVal, out var index))
                            {
                                string rangeValue = null;
                                for (var i = 0; i < optionArray.Count; i++)
                                {
                                    var rangeArray = JArray.FromObject(optionArray[i]);
                                    if (value.ToString() == rangeArray[0] + "-" + rangeArray[1])
                                    {
                                        rangeValue = i.ToString();
                                    }
                                }

                                if (rangeValue == null)
                                    return new ErrResult("field.invalid.value",
                                        $"{fieldFullName} <select> value not in pre-defined list");
                                value = index.ToString();
                            }
                            else if (index >= optionArray.Count)
                                return new ErrResult("field.invalid.value",
                                    $"{fieldFullName} <range> value out of bound");
                        }


                        break;
                    case "smartinput":
                    case "tagsinput":
                        if (!(value is JArray arrayValue))
                        {
                            if (!(value is string listStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <{type}> expects array or JSON");
                            try
                            {
                                arrayValue = JArray.Parse(listStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <{type}> expects valid JSON array");
                            }
                        }

                        if (arrayValue.Count == 0)
                            return new ErrResult("field.null", $"{fieldFullName} empty list");

                        value = arrayValue;
                        break;
                    case "date":
                    case "datecombo":
                        if (!(value is string dateStr))
                            return new ErrResult("field.invalid",
                                $"{fieldFullName} <{type}> expects 'YYYY-MM-DD' string");
                        if (string.IsNullOrEmpty(dateStr) && iField.Options.ToString() != "indef")
                            return new ErrResult("field.null", $"{fieldFullName} empty date");
                        if (!string.IsNullOrEmpty(dateStr) && !Regex.IsMatch(dateStr, @"\d\d\d\d-\d\d-\d\d"))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <{type}> expects 'YYYY-MM-DD' string");
                        var dateParts = dateStr.Split('-');
                        if (Convert.ToInt32(dateParts[0]) < 1900 || Convert.ToInt32(dateParts[1]) > 12 ||
                            Convert.ToInt32(dateParts[2]) > 31)
                            return new ErrResult("field.invalid.value", $"{fieldFullName} invalid date");
                        break;
                    case "location":
                        if (!(value is JObject locationObj))
                        {
                            if (!(value is string locationStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <location> expects object or JSON");
                            try
                            {
                                locationObj = JObject.Parse(locationStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <location> expects valid JSON object");
                            }
                        }

                        if (string.IsNullOrEmpty(locationObj.SelectToken("country")?.ToString()))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <location> requires {{ country }}");
                        value = locationObj;
                        break;
                    case "numinput":
                        if (!(value is JObject numObject))
                        {
                            if (!(value is string numStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <numinput> expects object or JSON");
                            try
                            {
                                numObject = JObject.Parse(numStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <numinput> expects valid JSON object");
                            }
                        }

                        if (string.IsNullOrEmpty(numObject.SelectToken("amount")?.ToString()))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <numinput> requires {{ amount }}");
                        value = numObject;
                        break;
                    case "doc":
                        if (!(value is JArray docArray))
                        {
                            if (!(value is string listStr))
                                return new ErrResult("field.invalid", $"{fieldFullName} <doc> expects array or JSON");
                            try
                            {
                                docArray = JArray.Parse(listStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <doc> expects valid JSON array");
                            }
                        }

                        if (docArray.Count == 0)
                            return new ErrResult("field.null", $"{fieldFullName} empty document list");
                        var docs = new ArrayList();
                        foreach (var doc in docArray)
                        {
//                            var selected = doc.SelectToken("selected");
//                            if (selected != null && (bool) selected)
//                            {
                            docs.Add(doc);
//                            }
                        }

                        if (docs.Count == 0) docs.Add(docArray[0]);
                        value = docs;
                        break;
                }


                var source = FieldSource.VaultCurrentValue;

                var json = JsonConvert.SerializeObject(value);
                var bsDoc = BsonSerializer.Deserialize<object>(json);
                value = bsDoc;

                if (field.Source == "newValue" && iField.Type != "qa" && iField.Type != "range")
                {
                    source = FieldSource.NewValue;


                    updateFields.Add(new UserField
                    {
                        Field = new UserFormField
                        {
                            Path = field.Path
                        },
                        Data = new UserFieldData
                        {
                            Value = value
                        }
                    });
                }

                postFields.Add(new ParticipationField
                {
                    Source = source,
                    Path = field.Path,
                    Value = value
                });


                // Construct Legacy Post fields

                var optionsJson = JsonConvert.SerializeObject(iField.Options);
//                var model = field.Value.ToString();
                var model = value.ToString();

                string modelarraystr = "";

                switch (type)
                {
                    case "doc":
                        var modelarrays = (JArray.FromObject(value)).Select(d =>
                            d.SelectToken("fileName")).ToList();
                        modelarraystr = JsonConvert.SerializeObject(modelarrays);
                        model = "";
                        break;

                    case "smartinput":
                    case "tagsinput":
                        if (value is List<object> list)
                        {
                            model = string.Join(",", list);
                        }

                        break;
                }

                var lField = new FieldinformationVault
                {
                    jsPath = field.Path,
                    displayName = iField.DisplayName,
                    type = iField.Type,
                    optional = iField.Optional,
                    options = iField.Options,
                    model = model,
                    modelarraysstr = modelarraystr
                };

                switch (type)
                {
                    case "doc":
                        lField.pathfile = (JArray.FromObject(value)).FirstOrDefault()?.SelectToken("filePath")
                            ?.ToString();
                        break;

                    case "location":
                    {
                        JObject location = JObject.FromObject(value);
                        lField.model = location.SelectToken("country")?.ToString();
                        lField.unitModel = location.SelectToken("city")?.ToString();
                        break;
                    }
                    case "numinput":
                    {
                        JObject num = JObject.FromObject(value);
                        lField.model = num.SelectToken("amount")?.ToString();
                        lField.unitModel = num.SelectToken("unit")?.ToString();
                        break;
                    }
                }

                legacyFields.Add(lField);
            }

            var post = new Participation
            {
                InteractionId = interactionId,
                AccountId = account.AccountId,
                BusinessId = businessAccountId,
                Fields = postFields,
                Timestamp = DateTime.UtcNow
            };
            var postCollection = MongoDBConnection.Database.GetCollection<Participation>("Participation");
            await postCollection.InsertOneAsync(post);

            //    Legacy Post

            //    Handshake
            if (legacyPostCollection == null)
            {
                string json = JsonConvert.SerializeObject(legacyFields);

                var lPost = new PostHandshake
                {
                    UserId = account.AccountId,
                    CampaignId = interactionId,
                    BusId = campaign.UserId,
                    Status = "Not acknowledged",
                    ParticipatedAt = DateTime.UtcNow,
                    Comment = string.Empty,
                    IsAccept = "1",
                    IsJoin = "1",
                    IsChange = "0",
                    DateJson = string.Empty,
                    FieldsJson = json,
                    FieldsJsonOld = json,
                    //Fields = legacyFields
                };
                if (pendingHandshake == null)
                {
                    lPost.Id = ObjectId.GenerateNewId().ToString();
                    lPost.Timestamp = DateTime.UtcNow;
                    await postHandshakeCollection.InsertOneAsync(lPost);
                }
                else
                {
                    lPost.Id = pendingHandshake.Id;
                    lPost.Timestamp = pendingHandshake.Timestamp;
                    lPost.Comment = pendingHandshake.Comment;
                    lPost.PushDates = pendingHandshake.PushDates;
                    var builderPost = Builders<PostHandshake>.Filter;
                    var filterPost = builderPost.Eq("Id", pendingHandshake.Id);
                    var result = await postHandshakeCollection.ReplaceOneAsync(filterPost, lPost);
                }
            }
            //    Non handshake
            else
            {
                var follower = new Follower
                {
                    UserId = account.AccountId,
                    Name = account.Profile.DisplayName,
                    Age = (account.Profile.Birthdate.HasValue)
                        ? (DateTime.Now.Year - account.Profile.Birthdate.Value.Year)
                        : 0,
                    Gender = account.Profile.Gender,
                    CountryName = account.Profile.Country,
                    CityName = account.Profile.City,
                    ParticipatedAt = DateTime.UtcNow,
                    DelegationId = delegation?.DelegationId,
                    DelegateeId = delegation?.ToAccountId,
                    fields = legacyFields
                };

                var builderPost = Builders<Post>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId);
                var filterFollower = Builders<Follower>.Filter.Eq("UserId", account.AccountId);

                var lPost = await legacyPostCollection.Find(filterPost).FirstOrDefaultAsync();
                if (lPost == null)
                {
                    var followers = new List<Follower> {follower};
                    lPost = new Post
                    {
                        PostedUserId = businessAccountId,
                        PostedUserName = "",
                        CampaignId = interactionId,
                        PostType = interaction.Type,
                        Followers = followers,
                        Created = DateTime.UtcNow
                    };
                    await legacyPostCollection.InsertOneAsync(lPost);
                }
                else
                {
                    if (pendingPost != null)
                    {
                        follower.FollowedDate = pendingFollower.FollowedDate;
                        follower.Comment = pendingFollower.Comment;
                        follower.PushDates = pendingFollower.PushDates;
                        filterPost = builderPost.Eq("Id", pendingPost.Id)
                                     & builderPost.ElemMatch("Followers", filterFollower);
                        var update = Builders<Post>.Update.Set("Followers.$", follower);
                        var result = await legacyPostCollection.UpdateOneAsync(filterPost, update);
                    }
                    else
                    {
                        var update = Builders<Post>.Update.AddToSet("Followers", follower);
                        var result = await legacyPostCollection.UpdateOneAsync(filterPost, update);
                    }
                }
            }

            _businessMemberService.AddBusinessMember(account, businessAccount);


            //    Save current vault
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filterVault = Builders<BsonDocument>.Filter.Eq("userId", account.AccountId);
            BsonDocument vaultDoc = vaultCollection.Find(filterVault).SingleOrDefault();
            vaultDoc?.Remove("_id");

            var updateResult = await UpdateVaultFields(updateFields, account.AccountId);
            List<FieldUpdate> fieldUpdates = (List<FieldUpdate>) updateResult.Data;
            if (!updateResult.Success || fieldUpdates.Count == 0)
            {
            }
            else
            {
                // new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(account.AccountId, true);
                await SyncHandshakeFields(fieldUpdates, legacyFields, account.AccountId);

                foreach (var field in fieldUpdates)
                {
                    var path = field.Path;
                    if (path.Contains(".currentAddress") || path.Contains(".currentAddress")
                                                         || path.Contains(".contact.mobile") ||
                                                         path.Contains(".contact.office")
                                                         || path.Contains(".contact.email") ||
                                                         path.Contains(".contact.officeEmail"))
                    {
                        var vaultService = new InfomationVaultBusinessLogic();
                        vaultService.CheckManualHandshakes(account.AccountId, vaultDoc);
                        break;
                    }
                }
            }

            var notificationMessage = new NotificationMessage
            {
                Type = iType == "Handshake"
                    ? EnumNotificationType.NotifyJoinHandshake
                    : EnumNotificationType.NotifyRegister,
                FromAccountId = account.AccountId,
                FromUserDisplayName = account.Profile.DisplayName,
                ToAccountId = campaign.UserId,
                ToUserDisplayName = "",
                PreserveBag = interaction.Id,
                Content = iType.ToLower()
            };
            new NotificationBusinessLogic().SendNotification(notificationMessage);

            return new FuncResult(true, "interaction.registered", fieldUpdates);
        }

        public class VaultUpdateResult
        {
            public bool Success { get; set; }
            public string Status { get; set; }
            public List<FieldUpdate> Fields { get; set; }
        }

        public async Task<FuncResult> UpdateVaultFields(List<UserField> fields, string accountId)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter.Eq("userId", accountId);
            BsonDocument vaultDoc = vaultCollection.Find(filter).SingleOrDefault();
            vaultDoc.Remove("_id");

//            JObject vault = JObject.Parse(vaultDoc.ToJson());
            JObject vault = JObject.FromObject(vaultDoc.ToDictionary());

            var fieldUpdates = new List<FieldUpdate>();

            foreach (var field in fields)
            {
                var path = field.Field.Path;
                var type = field.Field.Type;
                var value = field.Data.Value;

                var pathTokens = path.Trim('.').Split('.');

                if (pathTokens.Length < 2)
                    return new FuncResult(false, "fieldpath.invalid");

                string token = null, token2 = null, addFormToken = null, addFormDefaultToken = null;
                object value2 = value;
                object addFormItem = null;

                var bucket = pathTokens[0];


                switch (bucket)
                {
                    case "contact":
                    {
                        var contact = vault.SelectToken($"contact.value.{pathTokens[1]}");
                        var defaultValue = (string) contact.SelectToken("default");
                        var index = contact.SelectToken("value").Children().FirstOrDefault(
                                            c => (string) c.SelectToken("value") == defaultValue)?
                                        .BeforeSelf().Count() ?? 0;
                        token = $"contact.value.{pathTokens[1]}.value.{index}.value";
                        token2 = $"contact.value.{pathTokens[1]}.default";
                        break;
                    }

                    case "education":
                    case "employment":
                    case "membership":
                    {
                        var prop = pathTokens[1];
                        var node = vault.SelectToken($"{bucket}");
                        var defaultDesc = (string) node.SelectToken("default");
                        var index = 0;

                        var items = (JArray) node.SelectToken("value");

                        if (items.Count == 0)
                        {
                            var item = new
                            {
                                _id = 1,
                                privacy = string.Empty,
                                description = string.Empty,
                                note = string.Empty,
                                _default = true
                            };

                            addFormToken = $"{bucket}.value";
                            addFormItem = item;

                            addFormDefaultToken = $"{bucket}.default";
                        }
                        else
                        {
                            index = items.FirstOrDefault(a => a.SelectToken("description").ToString() == defaultDesc)?
                                        .BeforeSelf().Count() ?? 0;
                        }

                        switch (prop)
                        {
                            default:
                                token = $"{bucket}.value.{index}.{prop}";
                                break;
                        }

                        break;
                    }

                    case "address":
                    case "financial":
                    case "governmentID":
                    {
                        bucket = bucket == "address" ? "groupAddress" :
                            bucket == "financial" ? "groupFinancial" : "groupGovernmentID";
                        var prop = pathTokens[2];
                        var node = vault.SelectToken($"{bucket}.value.{pathTokens[1]}");
                        if (node == null) return new ErrResult("vault.error");
                        var defaultDesc = node.SelectToken("default")?.ToString() ?? "";
                        var index = 0;
                        var items = (JArray) node.SelectToken("value");
                        if (items.Count == 0)
                        {
                            var item = new
                            {
                                _id = 1,
                                privacy = string.Empty,
                                description = string.Empty,
                                note = string.Empty,
                                _default = true
                            };

                            addFormToken = $"{bucket}.value.{pathTokens[1]}.value";
                            addFormItem = item;

                            addFormDefaultToken = $"{bucket}.value.{pathTokens[1]}.default";
                        }
                        else
                        {
                            index = items.FirstOrDefault(a => a.SelectToken("description").ToString() == defaultDesc)?
                                        .BeforeSelf().Count() ?? 0;
                        }

                        switch (prop)
                        {
                            case "address":
                                prop = "addressLine";
                                break;
                            case "location":
                                var location = JObject.FromObject(value);
                                var subPathTokens = pathTokens.Take(pathTokens.Length - 1).ToArray();
                                token = $"{bucket}.value.{pathTokens[1]}.value.{index}.country";
                                value = location.SelectToken("country")?.ToString();
                                token2 = $"{bucket}.value.{pathTokens[1]}.value.{index}.city";
                                value2 = location.SelectToken("city")?.ToString();
                                break;
                        }

                        if (token == null)
                            token = $"{bucket}.value.{pathTokens[1]}.value.{index}.{prop}";
                        break;
                    }

                    default:
                    {
                        var prop = pathTokens[pathTokens.Length - 1];
                        switch (prop)
                        {
                            case "location":
                                var location = JObject.FromObject(value);
                                var subPathTokens = pathTokens.Take(pathTokens.Length - 1).ToArray();
                                token = string.Join(".value.", subPathTokens) + ".value.country.value";
                                value = location.SelectToken("country")?.ToString();
                                token2 = string.Join(".value.", subPathTokens) + ".value.city.value";
                                value2 = location.SelectToken("city")?.ToString();
                                break;
                            case "height":
                            case "weight":
                            case "neck":
                            case "arm":
                            case "inner":
                            case "biceps":
                            case "shoes":
                            case "ring":
                                var num = JObject.FromObject(value);
                                token = string.Join(".value.", pathTokens) + ".value";
                                token2 = string.Join(".value.", pathTokens) + ".unit";
                                value = num.SelectToken("amount")?.ToString();
                                value2 = num.SelectToken("unit")?.ToString();
                                break;
                            default:
                                token = string.Join(".value.", pathTokens) + ".value";
                                break;
                        }

                        break;
                    }
                }

                if (!string.IsNullOrEmpty(addFormToken))
                {
                    var formUpdate = Builders<BsonDocument>.Update.AddToSet(addFormToken, addFormItem);
                    var formResult = await vaultCollection.UpdateOneAsync(filter, formUpdate);
                    formUpdate = Builders<BsonDocument>.Update.Set(addFormDefaultToken, string.Empty);
                    formResult = await vaultCollection.UpdateOneAsync(filter, formUpdate);
                }

                var update = Builders<BsonDocument>.Update.Set(token, value);
                var result = await vaultCollection.UpdateOneAsync(filter, update);
                var fieldUpdate = new FieldUpdate
                {
                    Path = path,
                    Timestamp = DateTimeOffset.UtcNow
                };
                if (!result.IsAcknowledged || result.ModifiedCount == 0)
                {
                    fieldUpdate.Success = false;
                    fieldUpdate.Status = "notUpdated";
                }
                else
                {
                    fieldUpdate.Success = true;
                    fieldUpdate.Status = "updated";
                }

                fieldUpdates.Add(fieldUpdate);
                if (!string.IsNullOrEmpty(token2))
                {
                    update = Builders<BsonDocument>.Update.Set(token2, value2);
                    await vaultCollection.UpdateOneAsync(filter, update);
                }
            }

            return new FuncResult(true, "vault.updated", fieldUpdates);
        }

        public async Task<FuncResult> SyncHandshakeFields(IList<FieldUpdate> updatedFields,
            IList<FieldinformationVault> postFields, string accountId)
        {
            var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
            var builder = Builders<PostHandshake>.Filter;
            var update = Builders<PostHandshake>.Update;
            //var builderField = Builders<FieldinformationVault>.Filter;

            var filter = builder.Eq("UserId", accountId) & builder.Eq("IsJoin", "1");
            //& builder.ElemMatch("Fields", builderField.Eq("jsPath", path));
            var posts = await handshakeCollection.Find(filter).ToListAsync();
            foreach (var post in posts)
            {
                if (post.Status == "Terminate") continue;

                List<FieldinformationVault> fields;
                try
                {
                    fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(post.FieldsJson);
                }
                catch
                {
                    fields = /*post.Fields ?? */new List<FieldinformationVault>();
                }

                if (!fields.Any(f => updatedFields.Any(u => u.Path == f.jsPath)))
                    continue;
                var filterPost = builder.Eq("Id", post.Id);
                handshakeCollection.UpdateOne(filterPost, update.Set("FieldsJsonOld", post.FieldsJson));
                foreach (var field in fields)
                {
                    var pField = postFields.FirstOrDefault(f => f.jsPath == field.jsPath);
                    if (pField == null) continue;
                    field.model = pField.model;
                    field.unitModel = pField.unitModel;
                }


                try
                {
                    var json = JsonConvert.SerializeObject(fields);
                    var result = handshakeCollection.UpdateOne(filterPost, update.Set("FieldsJson", json));
                    if (result.IsAcknowledged && result.ModifiedCount > 0)
                    {
                        Log.Debug($"!!!--HS#{post.Id} of {post.UserId} of {post.BusId} / {post.CampaignId}");

                        handshakeCollection.UpdateOne(filterPost, update.Set("FieldsJson", json));
                        handshakeCollection.UpdateOne(filterPost, update.Set("IsChange", "1"));
                        handshakeCollection.UpdateOne(filterPost, update.Set("Status", "Not acknowledged"));
                        handshakeCollection.UpdateOne(filterPost, update.Set("LastSync", DateTime.UtcNow));
                        handshakeCollection.UpdateOne(filterPost,
                            update.Set("DateJson", DateTime.UtcNow.ToString("yyyy/MM/dd")));

                        var notificationMessage = new NotificationMessage
                        {
                            Type = EnumNotificationType.NotifyHandShakeVaultChanged,
                            FromAccountId = accountId,
                            FromUserDisplayName =
                                new AccountService().GetByAccountId(accountId).Profile.DisplayName,
                            ToAccountId = post.BusId,
                            ToUserDisplayName = "",
                            PreserveBag = post.CampaignId
                        };
                        new NotificationBusinessLogic().SendNotification(notificationMessage);

                        await new PostHandShakeBusinessLogic().ExportToSendMailHandShake(accountId, post.BusId,
                            post.CampaignId);
                    }
                }
                catch
                {
                    //Log.Debug(post.FieldsJson);
                }
            }

            return new OkResult("handshakes.sync.ok");
        }


        public async Task<FuncResult> GetInteraction(string interactionId)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId) & builder.Eq("Interaction.Status", "Active");

            var c = await InteractionCollection.Find(filter).FirstOrDefaultAsync();
            if (c == null) return new FuncResult(false, "notFound");
            var interaction = c.Interaction;

            return new OkResult("interaction.found", interaction);
        }

        public async Task<FuncResult> GetActiveInteractionsFromBusiness(string businessAccountId, string type = "")
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("UserId", businessAccountId) & builder.Eq("Interaction.Status", "Active");
            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower() == "broadcast") type = "Advertising";
                else if (type.ToLower() == "feed")
                    filter &= builder.Eq("Interaction.Type", "Advertising")
                              | builder.Eq("Interaction.Type", "Registration")
                              | builder.Eq("Interaction.Type", "Event");
                else
                    filter &= builder.Regex("Interaction.Type",
                        BsonRegularExpression.Create(new Regex(type, RegexOptions.IgnoreCase)));
            }

            var found = await InteractionCollection.Find(filter).ToListAsync();
            if (found.Count == 0) return new FuncResult(false, "notFound");
            var interactions = new List<InteractionCore>();
            foreach (var c in found)
            {
                var interaction = c.Interaction;
                var from = interaction.Criteria.Spend.EffectiveDate;
                if (from > DateTime.Now) continue;
                if (interaction.Criteria.Spend.Type != "Daily")
                {
                    var until = interaction.Criteria.Spend.EndDate;
                    if (new DateTime(until.Year, until.Month, until.Day, 0, 0, 0) < DateTime.Today)
                        continue;
                }

                interactions.Add(new InteractionCore
                {
                    Id = c.Id,
                    BusinessAccountId = c.UserId,
                    Name = interaction.Name,
                    Description = interaction.Description
                });
            }

            if (interactions.Count == 0) return new FuncResult(false, "notFound");
            return new FuncResult(true, "interactions.found", interactions);
        }


        public BsonArray GetInteractionFields(string interactionId)
        {
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", interactionId);
            var interaction = _interactionCollection.Find(criteria).FirstOrDefault();
            if (interaction == null) return null;
            var fields = (interaction["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("fields")) ==
                          null)
                ? null
                : interaction["campaign"]["fields"].AsBsonArray;
            return fields;
        }

        private object InteractionFields(string interactionId)
        {
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", interactionId);
            var interaction = _interactionCollection.Find(criteria).FirstOrDefault();
            if (interaction == null) return null;
            var cd = interaction["campaign"].AsBsonDocument.ToDictionary();
            object fields;
            cd.TryGetValue("fields", out fields);
            return fields;
        }

        public BsonArray GetInteractionGroups(string interactionId)
        {
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", interactionId);
            var interaction = _interactionCollection.Find(criteria).FirstOrDefault();
            if (interaction == null) return null;
            var groups = (interaction["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("groups")) ==
                          null)
                ? null
                : interaction["campaign"]["groups"].AsBsonArray;
            return groups;
        }


        public bool IsFollowing(string userAccountId, string businessAccountId)
        {
            var member = _businessMemberRepos.Single(m =>
                m.BusinessUserId.Equals(businessAccountId) && m.Members != null
                                                           && m.Members.Any(f =>
                                                               f.UserId.Equals(userAccountId) &&
                                                               f.Status == EnumFollowType.Following));

            return member != null;
        }

        public int CountParticipants(string interactionId)
        {
            try
            {
                var post = _postRepos.Many(p => p.CampaignId.Equals(interactionId) && p.Followers != null)
                    .FirstOrDefault();
                if (post == null) return 0;

                return post.Followers.Count(f => f.Status == null || f.Status != "Invite");
            }
            catch (Exception e)
            {
                Log.Debug("Error counting participants: " + e.Message + e.StackTrace);
                return 0;
            }
        }

        public Follower GetParticipation(string userAccountId, string interactionId)
        {
            var post = _postRepos.Single(p => p.CampaignId.Equals(interactionId) && p.Followers != null
                                                                                 && p.Followers.Any(f =>
                                                                                     f.UserId.Equals(userAccountId) &&
                                                                                     f.Status != "Invite"));
            if (post != null)
            {
                var participation = post.Followers.FirstOrDefault(f => f.UserId.Equals(userAccountId));
                return participation;
            }

            return null;
        }

        public Follower GetHandshakeParticipation(string userAccountId, string interactionId)
        {
            var post = _postRepos.Single(p => p.CampaignId.Equals(interactionId) && p.Followers != null
                                                                                 && p.Followers.Any(f =>
                                                                                     f.UserId.Equals(userAccountId) &&
                                                                                     f.Status != "Invite"));
            if (post != null)
            {
                var participation = post.Followers.FirstOrDefault(f => f.UserId.Equals(userAccountId));
                return participation;
            }

            return null;
        }

        public List<Follower> GetParticipations(Account account, string interactionId, string interactionType = "")
        {
//            Account account = _accountService.GetByAccountId(accountId);
//            var delegators = account.Delegations?.Where(d => d.Direction.Equals("DelegationIn")).ToList();
//            var delegatees = account.Delegations?.Where(d => d.Direction.Equals("DelegationOut")).ToList();
            List<Follower> participations = new List<Follower>();
            var post = _postRepos.Many(p => p.CampaignId.Equals(interactionId) && p.Followers != null).FirstOrDefault();

            if (post != null)
            {
                var participation = interactionType == "srfi"
                    ? post.Followers.FirstOrDefault(f => f.UserId == account.AccountId && f.Status != "Invite")
                    : post.Followers.FirstOrDefault(f => f.UserId == account.AccountId);
                if (participation != null)
                {
                    participations.Add(participation);
                }

                var actors = post.Followers.Where(f => f.DelegateeId == account.AccountId && f.Status != "Invite");

                foreach (var actor in actors)
                {
                    participations.Add(actor);
                }

                //                if (delegatees != null)
//                foreach (var delegatee in delegatees)
//                {
//                    participation = post.Followers.FirstOrDefault(f => f.UserId.Equals(delegatee.ToAccountId));
//                    if (participation != null)
//                    {
//                        participations.Add(participation);
//                    }
//                }
            }

            return participations;
        }


        public bool Follow(Account follower, Account followee)
        {
            return _businessMemberService.AddBusinessMember(follower, followee);
        }

        public async Task<FuncResult> UnfollowAsync(string followerId, string followeeId)
        {
            var memberCollection = MongoDBConnection.Database.GetCollection<BusinessMember>("BusinessMember");
            var builder = Builders<BusinessMember>.Filter;
            var builderMember = Builders<Follower>.Filter;
            var filter = builder.Eq("BusinessUserId", followeeId.ToString());
            var filterMember = builderMember.Eq("UserId", followerId.ToString());
            var result = await memberCollection.UpdateOneAsync(filter,
                Builders<BusinessMember>.Update.PullFilter("Members", filterMember));
            if (!result.IsAcknowledged)
                return new ErrResult("unfollow.not");
            return new OkResult("unfollow.ok");
        }

        public bool Unfollow(Account follower, Account followee)
        {
            return _businessMemberService.RemoveBusinessMember(follower, followee);
        }


        private FilterDefinition<BsonDocument> GetFilterVault(BsonValue bsonvaultfilter,
            FilterDefinitionBuilder<BsonDocument> filter)
        {
            FilterDefinition<BsonDocument> criteria = null;
            if (bsonvaultfilter.IsBsonArray)
                criteria = filter.AnyIn("campaign.criteria.keywords",
                    bsonvaultfilter.AsBsonArray.Select(x => x.AsString).ToArray());
            else
                criteria = filter.AnyIn("campaign.criteria.keywords", new string[] { });
            return criteria;
        }
    }
}