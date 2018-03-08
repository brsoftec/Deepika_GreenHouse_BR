using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Invite;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace GH.Core.Services
{
    public class UserService : IUserService
    {
        IMongoCollection<Account> _accountCollection;
        IMongoCollection<JoiningBusinessInvitation> _invitationCollection;
        IMongoCollection<Role> _roleCollection;

        private MongoRepository<Invite> _inviteRepository;

        private Logger log = LogManager.GetCurrentClassLogger();

        public UserService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _invitationCollection = db.JoiningBusinessInvitations;
            _roleCollection = db.Roles;
            _inviteRepository = new MongoRepository<Invite>();
        }

        public List<Account> SearchUsers(string keyword, string by = "name", int? start = 0, int? length = null)
        {
            if (length == null)
            {
                length = ConfigurationManager.AppSettings["LIMIT_SEARCH_RECORDS"].ParseInt();
            }

            keyword = keyword.ToLower();
            return by == "name"
                ? _accountCollection.Find(
                        a =>
                            a.AccountType == AccountType.Personal &&
                            a.Profile.DisplayName.ToLower().Contains(keyword))
                    .SortBy(a => a.Profile.DisplayName)
                    .Skip(start)
                    .Limit(length)
                    .ToList()
                : _accountCollection.Find(
                        a =>
                            a.AccountType == AccountType.Personal &&
                            a.Profile.Email.ToLower() == keyword)
                    .SortBy(a => a.Profile.Email)
                    .Skip(start)
                    .Limit(length)
                    .ToList();
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SearchUser
        {
            public string Id { get; set; }
            public string AccountId { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
            public string Email { get; set; }
        }

        public async Task<List<SearchUser>> SearchUsersAsync(string keyword, string by = "name", int? start = 0,
            int? length = null)
        {
            if (length == null)
            {
                length = 20;
            }

            keyword = keyword.ToLower();
            var accounts = by == "email"
                    ? await _accountCollection.Find(
                            a =>
                                a.AccountType == AccountType.Personal &&
                                a.Profile.Email.ToLower() == keyword)
                        .SortBy(a => a.Profile.Email)
                        .Skip(start)
                        .Limit(length)
                        .ToListAsync()
                    : await _accountCollection.Find(
                            a =>
                                a.AccountType == AccountType.Personal &&
                                a.Profile.DisplayName.ToLower().Contains(keyword))
                        .SortBy(a => a.Profile.DisplayName)
                        .Skip(start)
                        .Limit(length)
                        .ToListAsync();

            return accounts.Select(a => new SearchUser
            {
                Id = a.Id.ToString(),
                AccountId = a.AccountId,
                DisplayName = a.Profile.DisplayName,
                Avatar = a.Profile.PhotoUrl,
                Email = a.Profile.Email
            }).ToList();
        }

        public string CreateInvite(Invite invite)
        {
            invite.Id = ObjectId.GenerateNewId();
//            if (outsite.Payload == null)
//            {
//                outsite.Payload = new object();
//            }
            try
            {
                _inviteRepository.Add(invite);
                log.Debug("Inserted invite " + invite.Id.ToString());
            }
            catch (Exception ex)
            {
                log.Debug("Error inserting invite Id = " + invite.Id.ToString() + " exception " + ex.ToString());
                return "Error inserting invite";
            }

            return invite.Id.ToString();
        }

        public Invite GetInviteById(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                var result = _inviteRepository.Single(Id);
                return result;
            }
            catch (Exception ex)
            {
                return null;
//                throw new CustomException(ex.Message);
            }
        }

        public List<Invite> GetInvites(string fromUserId, string cat = null)
        {
            var invites = new List<Invite>();
            try
            {
                if (cat != null)
                {
                    invites = _inviteRepository.Many(l => l.FromUserId.Equals(fromUserId) && l.Category.Equals(cat))
                        .OrderByDescending(s => s.Created).ToList();
                }
                else
                {
                    invites = _inviteRepository.Many(l => l.FromUserId.Equals(fromUserId)).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }

            return invites;
        }

        public void DeleteInvite(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                Invite invite = _inviteRepository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                _inviteRepository.Delete(invite);
            }
            catch
            {
            }
        }
    }
}