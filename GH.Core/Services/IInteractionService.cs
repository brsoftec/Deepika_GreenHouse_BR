using System.Collections.Generic;
using System.Threading.Tasks;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.Models;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.ViewModels;

namespace GH.Core.Services
{
    public interface IInteractionService
    {
        BsonDocument GetInteractionById(string id);
        bool IsFollowing(string userAccountId, string businessAccountId);
        bool Follow(Account follower, Account followee);
        bool Unfollow(Account follower, Account followee);
        Task<FuncResult> UnfollowAsync(string followerId, string followeeId);

        Task<FuncResult> ListFollowedBusinessesAsync(Account account);

        BsonArray GetInteractionFields(string interactionId);
        BsonArray GetInteractionGroups(string interactionId);

        Task<bool> IsParticipated(string interactionId, Account account);
        Task<FuncResult> GetParticipation(string interactionId, Account account);
        Task<FuncResult> GetParticipation(string interactionId, string interactionType, Account account, string delegationId=null);
        Follower GetParticipation(string userAccountId, string interactionId);
        List<Follower> GetParticipations(Account account, string interactionId, string interactionType="");
        int CountParticipants(string interactionId);

        Task<FuncResult> GetUserInteractionAsync(string id, Account account, UserDelegation delegation=null);
        Task<FuncResult> GetActiveInteractionsFromBusiness(string businessAccountId, string type = "");
        Task<FuncResult> GetInteraction(string interactionId);
        FuncResult GetUserDataForFields(List<UserFormField> fields, Account account);
        
        Task<FuncResult> Register(string interactionId, Account account, IList<UserFieldData> fields, UserDelegation delegation=null);
        Task<FuncResult> UpdateVaultFields(List<UserField> fields, string accountId);
        Task<FuncResult> Unregister(string interactionId, Account account, UserDelegation delegation = null);

        Task<FuncResult> ListHandshakesAsync(Account account, string include="");
        Task<FuncResult> UpdateHandshakeStatusAsync(string interactionId, string status, Account account);
        Task<FuncResult> TerminateHandshakeAsync(string interactionId, Account account);
        Task<FuncResult> RemoveHandshakeAsync(string interactionId, Account account);

        Task<FuncResult> ListPersonalHandshakesAsync(Account account, string filter = "", string include = "");
        Task<FuncResult> UpdateStatusPersonalHandshakeAsync(string handshakeId, string status, Account account);
        Task<FuncResult> TerminatePersonalHandshakeAsync(string handshakeId, Account account);
        Task<FuncResult> RemovePersonalHandshakeAsync(string handshakeId, Account account);
        Task<FuncResult> AddPersonalHandshakeAsync(PersonalHandshakePostModel model, Account account);

        Task<FuncResult> ListHandshakeRequestsAsync(Account account);
        Task<FuncResult> ListHandshakeRequestsByBusinessAsync(Account account);
        Task<FuncResult> RemoveHandshakeRequestAsync(string hsrId, string accountId);
        Task<FuncResult> AddHandshakeRequestAsync(HandshakeRequestPostModel model, Account account);

        Task<FuncResult> SyncHandshakeFields(IList<FieldUpdate> updatedFields,
            IList<FieldinformationVault> postFields, string accountId);
    }
}