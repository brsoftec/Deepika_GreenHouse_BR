using GH.Core.BlueCode.Entity.Delegation;
using MongoDB.Bson;
using System.Collections.Generic;
using GH.Core.Models;
using GH.Core.ViewModels;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IDelegationBusinessLogic
    {
        BsonDocument GetDelegationItemTemplate();
        IEnumerable<DelegationItemTemplate> GetDelegationList(string userId, string delegateDirection = "All");
        void RequestDelegation(string userId, DelegationItemTemplate delegationMessage);
        FuncResult AcceptDelegation(string userId, string delegationId);
        void ActivatedDelegation(string userId, string delegationId);
        FuncResult DenyDelegation(string userId, string delegationId);
        void RemoveDelegation(string userId, string delegationId);
        DelegationItemTemplate GetDelegationById(string userId, string delegationId);
        DelegationItemTemplate GetDelegationById(string delegationId);
        bool checkIfInvitedEmailForDelegation(string delegatorAccountId, string delegateeEmail);

        FuncResult GetUserDelegations(Account account, string direction = "DelegationIn");
        FuncResult GetUserDelegation(string delegationId, Account account);
    }
}
