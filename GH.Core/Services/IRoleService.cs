using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface IRoleService
    {
        Role GetRoleByName(string name);
        Role GetRoleById(ObjectId id);
        List<Role> GetAllRoles();
        List<Role> GetRolesOfAccount(Account acc, ObjectId BAId);
    }
}