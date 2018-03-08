
using GH.Core.ViewModels;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using GH.Core.Models;

namespace GH.Core.Services
{
    public interface IBusinessInteractionService
    {
        Task<FuncResult> ListInteractionsAsync(Account account);
        
        BsonDocument GetInteraction(string id);
        Task<FuncResult> GetInteractionAsync(string interactionId, Account account);
        Task<FuncResult> GetBusinessInteractionAsync(string interactionId, Account account);

        Task<FuncResult> PushInteractionAsync(Interaction interaction, string message, Account toAccount,
            Account account);
        Task<FuncResult> UnregisterInteractionAsync(Interaction interaction, Account fromAccount,
            Account account);

        Task<FuncResult> ListCustomersAsync(Account account, int start = 0, int take = 20);
        Task<FuncResult> ListCustomersByInteractionAsync(Interaction interaction, Account account, int start = 0, int take = 20);

        Task<FuncResult> GetParticipantAsync(Interaction interaction, string participantId,
            Account account);

        List<BsonDocument> GetListInteraction(string type = null, string userId = null);
        string CreateInteraction(string json);
        void UpdateInteraction(string id, string json);
        List<BsonDocument> GetListInteractionActive(string type = null, string userId = null);
        List<CampaignDto> GetListInteractionActiveWithPagesize(string type = null, string userId = null, int start = 0, int take = 10);
    }
}