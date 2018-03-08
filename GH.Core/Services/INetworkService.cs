using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GH.Core.Services
{
    public interface INetworkService
    {
        FuncResult Invite(ObjectId from, ObjectId to, string inviteId = null);

        FuncResult IsFriend(string userId, string userId2);
        FuncResult IsFriendByAccountId(string accountId, string accountId2);

        Task<FuncResult> GetFriendsAsync(Account account);
        
        Task<FuncResult> MoveFriendAsync(Account account, Account friend, bool toTrust = false);
        Task<FuncResult> RemoveFriendAsync(Account account, Account friend);
        
        #region TrustNetwork

        void InviteTrustEmergency(ObjectId from, ObjectId to, string relationship, bool isEmergency, int rate);
        void AcceptTrustEmergency(ObjectId invitationId, ObjectId accepter, string relationship, bool isEmergency, int rate);
        void UpdateTrustEmergency(ObjectId userId, string userAccountId, string displayName, ObjectId networkId, ObjectId friendId, string friendAccountId, string friendDisplayName, string relationship, bool isEmergency, int rate);
        void UpdateNetworkRelationship(ObjectId userId, string userAccountId, string fromName, ObjectId networkId, ObjectId friendId, string friendAccountId, string relationship);

        List<Account> SearchUsersForTrustEmergency(ObjectId userId, string keyword, int? start = null, int? length = null);
        #endregion TrustNetwork
        FuncResult AcceptInvitation(ObjectId invitationId, ObjectId accepter);
        void DelegateeJoinsTrustedNetwork(string delegateeAccountId, string delegatorAccountId);

        void RemoveInvitation(ObjectId invitationId, ObjectId remover);
        FuncResult DenyInvitation(ObjectId invitationId, ObjectId denier);
        List<FriendInvitation> GetReceivedInvitations(ObjectId receiver, ObjectId? fromId);
        //GetSendInvitations
        List<FriendInvitation> GetSendInvitations(ObjectId receiver);
        void MoveNetwork(ObjectId userId, ObjectId friendId, ObjectId fromNetworkId, ObjectId toNetworkId);
        void RemoveFromNetwork(ObjectId userId, ObjectId friendId, ObjectId networkId);


        Network GetNetworkById(ObjectId id);
        Network InsertNetwork(Network network);
        List<Network> GetNetworksOfuser(ObjectId userId);

        List<Account> SearchUsersForInvitation(ObjectId userId, string keyword, int? start = null, int? length = null);
        List<Network> GetNetworksUserBelongTo(ObjectId userId);

        IEnumerable<MyFriend> GetFriendsForDelegation(string userId);

        IEnumerable<Account> GetFriends(string userId);
        void InviteEmailOutsideForEmergency(string fromAccountId, string toEmail, string inviteId);
        void InviteListEmailOutsideForEmergency(string fromAccountId, string[] toEmail, string inviteId);
        void UpdateNetworkTrustData();
        FriendInvitation GetInvitationById(ObjectId invitationId);




    }
}