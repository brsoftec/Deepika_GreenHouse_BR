using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GH.Core.BlueCode.Entity.Invite;

namespace GH.Core.Services
{
    public interface IUserService
    {
        List<Account> SearchUsers(string keyword, string by, int? start = null, int? length = null);
        Task<List<UserService.SearchUser>> SearchUsersAsync(string keyword, string by = "name", int? start = 0, int? length = null);
    }
}