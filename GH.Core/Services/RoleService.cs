using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public class RoleService : IRoleService
    {
        IMongoCollection<Role> _roleCollection;

        public RoleService()
        {
            var db = MongoContext.Db;
            _roleCollection = db.Roles;
        }

        public Role GetRoleByName(string name)
        {
            return _roleCollection.Find(r => r.Name == name).FirstOrDefault();
        }

        public Role GetRoleById(MongoDB.Bson.ObjectId id)
        {
            return _roleCollection.Find(r => r.Id == id).FirstOrDefault();
        }

        public List<Role> GetAllRoles()
        {
            return _roleCollection.Find(new BsonDocument()).ToList();
        }

        public List<Role> GetRolesOfAccount(Account acc, ObjectId BAId)
        {
            var RoleIdsOfBA = acc.BusinessAccountRoles.Where(s => s.AccountId == BAId && s.RoleId != null).Select(s => s.RoleId).ToList();
            var roles = _roleCollection.Find(new BsonDocument()).ToList();
            roles = roles.Where(s => RoleIdsOfBA.Contains(s.Id)).ToList();
            return roles;
        }
    }
}