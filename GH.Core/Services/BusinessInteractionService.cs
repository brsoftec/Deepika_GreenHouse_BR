using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.WebPages;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using NLog;
using GH.Core.ViewModels;
using GH.Core.Adapters;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.BlueCode.Entity.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RegitSocial.Business.Notification;

namespace GH.Core.Services
{
    public class BusinessInteractionService : IBusinessInteractionService
    {
        private static readonly IAccountService AccountService = new AccountService();

        private readonly IMongoCollection<BsonDocument> _interactionCollection =
            MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

        private static readonly IMongoCollection<InteractionCampaign> InteractionCollection =
            MongoDBConnection.Database.GetCollection<InteractionCampaign>("Campaign");

        private static readonly IMongoCollection<Post> PostCollection =
            MongoDBConnection.Database.GetCollection<Post>("Post");

        private static readonly IMongoCollection<PostHandshake> HandshakeCollection =
            MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");

        private readonly MongoRepository<BusinessMember> _businessMemberRepos = new MongoRepository<BusinessMember>();

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public BusinessInteractionService()
        {
        }

        public BsonDocument GetInteraction(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            BsonDocument campaign = _interactionCollection.Find(filter).FirstOrDefault();

            if (campaign == null)
            {
                filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                campaign = _interactionCollection.Find(filter).FirstOrDefault();
            }

            return campaign;
        }

        public async Task<FuncResult> GetInteractions(Account account, string type = "")
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("UserId", account.AccountId) & builder.Ne("Interaction.Status", "Remove");
            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower() == "broadcast") type = "Advertising";
                filter &= builder.Regex("Interaction.Type",
                    BsonRegularExpression.Create(new Regex(type, RegexOptions.IgnoreCase)));
            }

            var found = await InteractionCollection.Find(filter).ToListAsync();
            if (found.Count == 0) return new FuncResult(false, "notFound");
            var interactions = new List<InteractionCore>();
            foreach (var c in found)
            {
                var interaction = c.Interaction;
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

        public async Task<FuncResult> ListInteractionsAsync(Account account)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("UserId", account.AccountId)
                         & builder.Ne("Interaction.Type", "Advertising") & builder.Ne("Interaction.Status", "Remove");

            var campaigns = await InteractionCollection.Find(filter).ToListAsync();
            if (campaigns.Count == 0) return new ErrResult("notFound");
            var interactions = new List<InteractionCore>();
            campaigns.Reverse();

            //var builderPost = Builders<Post>.Filter;
            foreach (var c in campaigns)
            {
                var interaction = c.Interaction;
                var i = new InteractionCore
                {
                    Id = c.Id,
                    Name = interaction.Name,
                    Type = interaction.Type.ToLower(),
                    Description = interaction.Description
                };
                //var filterPost = builderPost.Eq("CampaignId", c.Id);
                if (i.Type == "handshake")
                {
                    i.Participants = HandshakeCollection.AsQueryable()
                        .Count(p => p.CampaignId == c.Id && p.Status != "Terminate");
                }
                else
                {
                    i.Participants = PostCollection.AsQueryable().Where(p => p.CampaignId == c.Id)
                        .Sum(p => p.Followers.Count(f => f.Status != "Terminate"));
                }

                interactions.Add(i);
            }

            return new OkResult("interactions.list", interactions);
        }

        public async Task<FuncResult> GetInteractionAsync(string interactionId, Account account)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId) & builder.Ne("Interaction.Status", "Remove");

            var campaign = await InteractionCollection.Find(filter).FirstOrDefaultAsync();
            if (campaign == null) return new ErrResult("notFound");
            if (campaign.UserId != account.AccountId) return new ErrResult("interaction.notAuthorized");
            var interaction = campaign.Interaction;
            interaction.Id = campaign.Id;
            return new OkResult("interaction.found", interaction);
        }

        public async Task<FuncResult> PushInteractionAsync(Interaction interaction, string message, Account toAccount,
            Account account)
        {
            bool isHandshake = interaction.Type.ToLower() == "handshake";
            var interactionId = interaction.Id;

            if (isHandshake)
            {
                var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.Eq("UserId", account.AccountId);
                var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post != null)
                {
                    if (post.Status == "Terminate")
                        return new ErrResult("handshake.terminated");
                    if (post.IsAccept == "1")
                        return new ErrResult("handshake.not.pending");
                }
                else
                {
                    var lPost = new PostHandshake
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        UserId = toAccount.AccountId,
                        CampaignId = interactionId,
                        BusId = account.AccountId,
                        Status = "Not acknowledged",
                        Timestamp = DateTime.UtcNow,
                        PushDates = new[] {DateTime.UtcNow},
                        Comment = message,
                        IsAccept = "0",
                        IsJoin = "0",
                        IsChange = "0",
                        DateJson = string.Empty,
                        //Fields = new List<FieldinformationVault>(),
                        FieldsJson = "[]"
                    };
                    await handshakeCollection.InsertOneAsync(lPost);
                }
            }
            else
            {
                var postCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var builderFollower = Builders<Follower>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.ElemMatch("Followers",
                                     builderFollower.Eq("UserId", toAccount.AccountId));
                var post = await postCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post != null)
                {
                    var follower = post.Followers.FirstOrDefault(f => f.UserId == toAccount.AccountId);
                    if (follower?.Status != "Invite")
                    {
                        return new ErrResult("interaction.not.pending");
                    }
                }
                else
                {
                    var follower = new Follower
                    {
                        UserId = toAccount.AccountId,
                        FollowedDate = DateTime.UtcNow.ToString("o"),
                        Status = "Invite",
                        Comment = message,
                        PushDates = new[] {DateTime.UtcNow}
                    };

                    var filterPostCampaign = builderPost.Eq("CampaignId", interactionId);

                    var lPost = await postCollection.Find(filterPostCampaign).FirstOrDefaultAsync();
                    if (lPost == null)
                    {
                        var followers = new List<Follower> {follower};
                        lPost = new Post
                        {
                            PostedUserId = account.AccountId,
                            PostedUserName = "",
                            CampaignId = interactionId,
                            PostType = interaction.Type,
                            Followers = followers,
                            Created = DateTime.UtcNow
                        };
                        await postCollection.InsertOneAsync(lPost);
                    }
                    else
                    {
                        var update = Builders<Post>.Update.AddToSet("Followers", follower);
                        var result = await postCollection.UpdateOneAsync(filterPostCampaign, update);
                    }
                }
            }

            var notificationMessage = new NotificationMessage
            {
                Type =
                    isHandshake ? EnumNotificationType.NotifyInvitedHandshake : EnumNotificationType.NotifyInviteSRFI,
                FromAccountId = account.AccountId,
                FromUserDisplayName = account.Profile.DisplayName,
                ToAccountId = toAccount.AccountId,
                ToUserDisplayName = toAccount.Profile.DisplayName,
                PreserveBag = interaction.Id,
                Content = interaction.Type.ToLower()
            };
            new NotificationBusinessLogic().SendNotification(notificationMessage);
            return new OkResult("interaction.push.ok");
        }

        public async Task<FuncResult> UnregisterInteractionAsync(Interaction interaction, Account fromAccount,
            Account account)
        {
            bool isHandshake = interaction.Type.ToLower() == "handshake";
            var interactionId = interaction.Id;
            bool pending;
            if (isHandshake)
            {
                var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId) &
                                 builderPost.Eq("UserId", fromAccount.AccountId);

                var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null)
                    return new ErrResult("interaction.notParticipated");
                pending = post.IsAccept == "0";
                var result = await handshakeCollection.DeleteOneAsync(filterPost);
                if (!result.IsAcknowledged || result.DeletedCount == 0)
                    return new ErrResult("unregister.not");
            }
            else
            {
                var postCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var builderFollower = Builders<Follower>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interactionId)
                                 & builderPost.ElemMatch("Followers",
                                     builderFollower.Eq("UserId", fromAccount.AccountId));
                var post = await postCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null)
                    return new ErrResult("interaction.notParticipated");
                var follower = post.Followers.FirstOrDefault(f => f.UserId == fromAccount.AccountId);
                pending = follower?.Status == "Invite";

                var result = await postCollection.UpdateOneAsync(filterPost,
                    Builders<Post>.Update.PullFilter("Followers", builderFollower.Eq("UserId", fromAccount.AccountId)));
                if (!result.IsAcknowledged || result.ModifiedCount == 0)
                    return new ErrResult("unregister.not");
            }

            if (!pending)
            {
                var notificationMessage = new NotificationMessage
                {
                    Type = EnumNotificationType.NotifyBusinessUnregister,
                    FromAccountId = account.AccountId,
                    FromUserDisplayName = account.Profile.DisplayName,
                    ToAccountId = fromAccount.AccountId,
                    ToUserDisplayName = fromAccount.Profile.DisplayName,
                    PreserveBag = interaction.Id,
                    Content = interaction.Type.ToLower()
                };
                new NotificationBusinessLogic().SendNotification(notificationMessage);
            }

            return new OkResult("unregister.ok");
        }

        public async Task<FuncResult> GetBusinessInteractionAsync(string interactionId, Account account)
        {
            var builder = Builders<InteractionCampaign>.Filter;
            var filter = builder.Eq("Id", interactionId) & builder.Ne("Interaction.Status", "Remove");

            var campaign = await InteractionCollection.Find(filter).FirstOrDefaultAsync();
            if (campaign == null) return new ErrResult("notFound");
            if (campaign.UserId != account.AccountId) return new ErrResult("interaction.notAuthorized");
            var i = campaign.Interaction;
            var type = i.Type.ToLower();
            if (type == "advertising")
                type = "broadcast";
            var interaction = new BusinessInteraction
            {
                Id = campaign.Id,
                Type = type,
                Name = i.Name,
                Description = i.Description,
                Image = i.Image,
                Since = i.Criteria.Spend.EndDate,
                Indefinite = i.Indefinite
            };
            if (!i.Indefinite)
                interaction.Until = i.Criteria.Spend.EndDate;

            var fields = new List<UserFormField>();

            foreach (var f in i.Fields)
            {
                var path = f.JsPath;
                if (path == "undefined") path = f.Path; //"Custom";
                var fieldType = f.Type;
                object options = f.Options;
                if (fieldType == "qa")
                {
                    if (f.Choices)
                    {
                        if (f.Model is string)
                        {
                            f.Choices = false;
                        }
                        else
                        {
                            var choices = JArray.FromObject(f.Model);
                            options = choices?.Select(q => q["value"]).ToList();
                        }
                    }
                }
                else if (fieldType == "range")
                {
                    options = f.Model;
                }

/*
                if (options != null && options is string && fieldType != "range")
                    options
*/
                var field = new UserFormField
                {
                    Path = path,
                    DisplayName = f.DisplayName,
                    Type = fieldType,
                    Options = options,
                    Optional = f.Optional,
                };

                if (path.StartsWith("Custom") && string.IsNullOrEmpty(field.Group))
                    field.Group = "userInformation";
                fields.Add(field);
            }


            var groups = i.Groups?.ToList();
            if (groups == null)
            {
                groups = new List<InteractionFieldGroup>();
                foreach (var field in fields)
                {
                    string groupName;
                    if (field.Path == null || field.Path.StartsWith("Custom"))
                        groupName = "userInformation";
                    else
                    {
                        var paths = field.Path.Trim('.').Split('.');
                        groupName = paths[0];
                    }

                    var group = groups.FirstOrDefault(g => g.Name == groupName);
                    if (group == null)
                    {
                        groups.Add(new InteractionFieldGroup
                        {
                            Name = groupName,
                            DisplayName = groupName.First().ToString().ToUpper()
                                          + Regex.Replace(groupName, "(.)([A-Z])", "$1 $2").Substring(1)
                        });
                    }

                    field.Group = groupName;
                }
            }
            else
            {
                foreach (var group in groups)
                {
                    var name = group.Name;
                    foreach (var field in fields)
                    {
                        if (string.IsNullOrEmpty(field.Group) && field.Path.Contains(name))
                            field.Group = name;
                    }
                }
            }

            interaction.Fields = fields;
            interaction.Groups = groups;


            return new OkResult("interaction.ok", interaction);
        }

        public async Task<FuncResult> ListCustomersAsync(Account account, int start = 0, int take = 20)
        {
            var accountId = account.AccountId;
            DateTime timestamp;

            var customers = new List<Customer>();

            try
            {
                var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var filterPost = builderPost.Eq("PostedUserId", accountId);
                var posts = await legacyPostCollection.Find(filterPost).ToListAsync();
                try
                {
                    foreach (var post in posts)
                    {
                        foreach (var follower in post.Followers)
                        {
                            try
                            {
                                if (follower.Status == "Terminate") continue;
                                bool pending = follower.Status == "Invite";
                                DateTime.TryParse(follower.FollowedDate, out timestamp);
                                var participation = new InteractionParticipation
                                {
                                    InteractionId = post.CampaignId,
                                    ParticipatedAt = timestamp,
                                    Comment = follower.Comment,
                                    Status = pending ? "pending" : "active"
                                };
                                if (!pending)
                                {
                                    participation.Fields = follower.fields?.ToList();
                                }

                                var customer = customers.FirstOrDefault(c => c.AccountId == follower.UserId);
                                if (customer == null)
                                {
                                    customer = new Customer
                                    {
                                        AccountId = follower.UserId,
                                        Participations = new List<InteractionParticipation>
                                        {
                                            participation
                                        }
                                    };
                                    customers.Add(customer);
                                }
                                else
                                {
                                    customer.Participations.Add(participation);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug(post.Id);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }

                try
                {
                    var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                    var filterHandshakes = Builders<PostHandshake>.Filter.Eq("BusId", accountId)
                                           & Builders<PostHandshake>.Filter.Ne("Status", "Terminate");
                    var handshakes = await handshakeCollection.Find(filterHandshakes).ToListAsync();
                    foreach (var post in handshakes)
                    {
                        try
                        {
                            bool pending = post.IsJoin != "1";
                            timestamp = post.Timestamp ?? DateTime.MinValue;
                            var participation = new InteractionParticipation
                            {
                                InteractionId = post.CampaignId,
                                ParticipatedAt = timestamp,
                                Comment = post.Comment,
                                Status = pending ? "pending" : "active"
                            };
                            if (!pending)
                            {
                                var json = post.FieldsJson;
                                try
                                {
                                    participation.Fields =
                                        JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                                }
                                catch
                                {
                                    participation.Fields = new List<FieldinformationVault>(); //post.Fields?.ToList();
                                }
                            }

                            var customer = customers.FirstOrDefault(c => c.AccountId == post.UserId);
                            if (customer == null)
                            {
                                customer = new Customer
                                {
                                    AccountId = post.UserId,
                                    Participations = new List<InteractionParticipation>
                                    {
                                        participation
                                    }
                                };
                                customers.Add(customer);
                            }
                            else
                            {
                                customer.Participations.Add(participation);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug(post.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }
            }
            catch (Exception e)
            {
                Log.Debug($" {e.Message}: {e.StackTrace}");
            }

            var validCustomers = new List<Customer>();
            try
            {
                foreach (var customer in customers)
                {
                    var customerAccount = AccountService.GetByAccountId(customer.AccountId);
                    if (customerAccount == null || customerAccount.AccountType == AccountType.Business) continue;
                    customer.profile = new EmbeddedProfile
                    {
                        Id = customerAccount.Id.ToString(),
                        DisplayName = customerAccount.Profile.DisplayName,
                        Avatar = customerAccount.Profile.PhotoUrl
                    };
                    foreach (var participation in customer.Participations)
                    {
                        var fields = participation.Fields;
                        if (fields != null)
                            foreach (var field in fields)
                            {
                                var type = field.type;
                                string value = field.model;
                                object newValue = value;
                                try
                                {
                                    switch (type)
                                    {
                                        case "date":
                                        case "datecombo":
                                            if (DateTime.TryParse(value, out var date))
                                                newValue = date.ToString("yyyy-MM-dd");
                                            else
                                                newValue = null;
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
                                            var docs = new ArrayList();
                                            try
                                            {
                                                var fnames =
                                                    JsonConvert.DeserializeObject<List<string>>(field.modelarraysstr);
                                                foreach (var fname in fnames)
                                                {
                                                    docs.Add(new
                                                    {
                                                        fileName = fname,
                                                        filePath = field.pathfile
                                                    });
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Log.Debug(
                                                    $"#{participation.InteractionId} {e.Message} : {e.StackTrace}");
                                            }

                                            newValue = docs;
                                            break;

                                        case "range":
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug(e.Message);
                                }

                                var pField = new UserFieldData
                                {
                                    Path = field.jsPath,
                                    Value = newValue
                                };
                                participation.UserData.Add(pField);
                            }
                    }

                    customer.Participations = customer.Participations.OrderByDescending(p => p.ParticipatedAt).ToList();
                    validCustomers.Add(customer);
                }
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
            }


            return new OkResult("customers.list", validCustomers);
        }

        public async Task<FuncResult> ListCustomersByInteractionAsync(Interaction interaction, Account account,
            int start = 0,
            int take = 20)
        {
            var accountId = account.AccountId;

            DateTime timestamp;
            bool pending = false;
            string comment = null;

            var customers = new List<Customer>();

            if (interaction.Type.ToLower() == "handshake")
            {
                var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interaction.Id);
                var posts = await handshakeCollection.Find(filterPost).ToListAsync();
                if (posts.Count == 0) return new ErrResult("notFound");
                foreach (var post in posts)
                {
                    try
                    {
                        pending = post.IsJoin != "1";
                        timestamp = post.Timestamp ?? DateTime.MinValue;
                        var participation = new InteractionParticipation
                        {
                            InteractionId = interaction.Id,
                            ParticipatedAt = timestamp,
                            Comment = post.Comment,
                            Status = pending ? "pending" : "active"
                        };
                        if (!pending)
                        {
                            var json = post.FieldsJson;
                            try
                            {
                                participation.Fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                            }
                            catch
                            {
                                participation.Fields = new List<FieldinformationVault>(); //post.Fields?.ToList();
                            }
                        }

                        var customer = new Customer()
                        {
                            AccountId = post.UserId,
                            Participations = new List<InteractionParticipation>
                            {
                                participation
                            }
                        };
                        customers.Add(customer);
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"#{post.Id} {e.Message} : {e.StackTrace}");
                    }
                }
            }
            else
            {
                var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interaction.Id);
                var posts = await legacyPostCollection.Find(filterPost).ToListAsync();
                if (posts.Count == 0) return new ErrResult("notFound");
                foreach (var post in posts)
                {
                    foreach (var follower in post.Followers)
                    {
                        pending = follower.Status == "Invite";
                        DateTime.TryParse(follower.FollowedDate, out timestamp);
                        var participation = new InteractionParticipation
                        {
                            InteractionId = interaction.Id,
                            ParticipatedAt = timestamp,
                            Comment = follower.Comment,
                            Status = pending ? "pending" : "active"
                        };
                        if (!pending)
                        {
                            participation.Fields = follower.fields?.ToList();
                        }

                        var customer = new Customer
                        {
                            AccountId = follower.UserId,
                            Participations = new List<InteractionParticipation>
                            {
                                participation
                            }
                        };
                        customers.Add(customer);
                    }
                }
            }

            foreach (var customer in customers)
            {
                var customerAccount = AccountService.GetByAccountId(customer.AccountId);
                if (customerAccount != null)
                    customer.profile = new EmbeddedProfile
                    {
                        Id = customerAccount.Id.ToString(),
                        DisplayName = customerAccount.Profile.DisplayName,
                        Avatar = customerAccount.Profile.PhotoUrl
                    };
                foreach (var participation in customer.Participations)
                {
                    var fields = participation.Fields;
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
                                    var docs = new ArrayList();
                                    try
                                    {
                                        var fnames =
                                            JsonConvert.DeserializeObject<List<string>>(field.modelarraysstr);
                                        foreach (var fname in fnames)
                                        {
                                            docs.Add(new
                                            {
                                                fileName = fname,
                                                filePath = field.pathfile
                                            });
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Debug($"#{participation.InteractionId} {e.Message} : {e.StackTrace}");
                                    }

                                    newValue = docs;
                                    break;

                                case "range":
                                    break;
                            }

                            var pField = new UserFieldData
                            {
                                Path = field.jsPath,
                                Value = newValue
                            };
                            participation.UserData.Add(pField);
                        }
                }

                customer.Participations = customer.Participations.OrderByDescending(p => p.ParticipatedAt).ToList();
            }

            return new OkResult("customers.list", customers);
        }

        public async Task<FuncResult> GetParticipantAsync(Interaction interaction, string participantId,
            Account account)

        {
            var accountId = account.AccountId;

            DateTime timestamp;
            bool pending = false;
            string comment = null;

            var filterFollower = Builders<Follower>.Filter;

            InteractionParticipant participant;

            if (interaction.Type.ToLower() == "handshake")
            {
                var handshakeCollection = MongoDBConnection.Database.GetCollection<PostHandshake>("PostHandShake");
                var builderPost = Builders<PostHandshake>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interaction.Id)
                                 & builderPost.Eq("BusId", account.AccountId)
                                 & builderPost.Eq("UserId", participantId);
                var post = await handshakeCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null) return new ErrResult("notFound");
                pending = post.IsJoin != "1";
                timestamp = post.Timestamp ?? DateTime.UtcNow;
                participant = new InteractionParticipant
                {
                    AccountId = post.UserId,
                    ParticipatedAt = timestamp,
                    Comment = post.Comment,
                    Status = pending ? "pending" : "active"
                };
                if (!pending)
                {
                    var json = post.FieldsJson;
                    try
                    {
                        participant.Fields = JsonConvert.DeserializeObject<List<FieldinformationVault>>(json);
                    }
                    catch
                    {
                        participant.Fields = new List<FieldinformationVault>(); //post.Fields?.ToList();
                    }
                }
            }
            else
            {
                var legacyPostCollection = MongoDBConnection.Database.GetCollection<Post>("Post");
                var builderPost = Builders<Post>.Filter;
                var filterPost = builderPost.Eq("CampaignId", interaction.Id)
                                 & builderPost.ElemMatch("Followers", filterFollower.Eq("UserId", participantId));

                var post = await legacyPostCollection.Find(filterPost).FirstOrDefaultAsync();
                if (post == null) return new ErrResult("notFound");

                var follower = post.Followers.FirstOrDefault(f => f.UserId == participantId);
                if (follower == null) return new ErrResult("notFound");

                pending = follower.Status == "Invite";
                DateTime.TryParse(follower.FollowedDate, out timestamp);
                participant = new InteractionParticipant
                {
                    AccountId = follower.UserId,
                    ParticipatedAt = timestamp,
                    Comment = follower.Comment,
                    Status = pending ? "pending" : "active"
                };
                if (!pending)
                {
                    participant.Fields = follower.fields?.ToList();
                }
            }

            participant.InteractionId = interaction.Id;
            var participantAccount = AccountService.GetByAccountId(participant.AccountId);
            if (participantAccount != null)
                participant.profile = new EmbeddedProfile
                {
                    Id = participantAccount.Id.ToString(),
                    DisplayName = participantAccount.Profile.DisplayName,
                    Avatar = participantAccount.Profile.PhotoUrl
                };
            var fields = participant.Fields;
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
                            var docs = new ArrayList();
                            try
                            {
                                var fnames =
                                    JsonConvert.DeserializeObject<List<string>>(field.modelarraysstr);
                                foreach (var fname in fnames)
                                {
                                    docs.Add(new
                                    {
                                        fileName = fname,
                                        filePath = field.pathfile
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug($"#{participant.InteractionId} {e.Message} : {e.StackTrace}");
                            }

                            newValue = docs;
                            break;

                        case "range":
                            break;
                    }

                    var pField = new UserFieldData
                    {
                        Path = field.jsPath,
                        Value = newValue
                    };
                    participant.UserData.Add(pField);
                }

            return new OkResult("participant.ok", participant);
        }

        public List<BsonDocument> GetListInteraction(string type = null, string userId = null)
        {
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(userId))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.type", type);

                return _interactionCollection.Find(filter).ToList();
            }

            else
                return null;
        }

        public List<BsonDocument> GetListInteractionActive(string type = null, string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            if (!string.IsNullOrEmpty(type))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.type", type);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.status", "Active");
                return _interactionCollection.Find(filter).ToList();
            }
            else
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.status", "Active");
                return _interactionCollection.Find(filter).ToList();
            }
        }

        public List<CampaignDto> GetListInteractionActiveWithPagesize(string type = null, string userId = null,
            int start = 0, int take = 10)
        {
            var result = new List<CampaignDto>();
            if (string.IsNullOrEmpty(userId))
                return null;
            var listDocument = new List<BsonDocument>();
            if (!string.IsNullOrEmpty(type))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.type", type);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.status", "Active");
                listDocument = _interactionCollection.Find(filter).ToList();
            }
            else
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                filter = filter & Builders<BsonDocument>.Filter.Eq("campaign.status", "Active");
                listDocument = _interactionCollection.Find(filter).ToList();
            }

            if (listDocument.Count() <= 0)
                return null;
            foreach (var item in listDocument)
            {
                if (item == null) continue;
                var bsIn = new CampaignDto();
                bsIn = CampaignAdapter.BsonToCampaignDto(item);

                if (bsIn != null)
                    result.Add(bsIn);
            }

            return result.Skip(start).Take(take).ToList();
        }

        public string CreateInteraction(string json)
        {
//            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            BsonDocument campaign = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            //ObjectId.GenerateNewId();
            var id = ObjectId.GenerateNewId();
            campaign["_id"] = id.ToString();
            _interactionCollection.InsertOne(campaign);
            return id.ToString();
        }

        public void UpdateInteraction(string id, string json)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

            BsonDocument campaign = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            _interactionCollection.ReplaceOne(filter, campaign);
        }
    }
}