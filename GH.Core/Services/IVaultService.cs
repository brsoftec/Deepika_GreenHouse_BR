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
    public interface IVaultService
    {
        Task<List<UserVaultField>> ListFieldsAsync();
        UserVaultField GetField(string path);
        Task<UserVaultField> GetFieldAsync(string path);
    }
}