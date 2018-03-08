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
    public interface IInviteService
    {
        Invite CreateInvite(Invite invite);
        List<Invite> GetInvites(string fromUserId, string cat = null, string status = null);
        Invite GetInviteById(string id);
        void DeleteInviteById(string id);
        void ConvertInviteById(string id);
        void SendInvite(Invite invite);
        Task<FuncResult> SendInviteAsync(Invite invite);
    }
}