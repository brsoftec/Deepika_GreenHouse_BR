using GH.Core.Interfaces;
using GH.Core.Models;
using MongoDB.Driver;

namespace GH.Core.IRepositories
{
    public interface ITransactionRepository : IEntityRepository<Transaction>
    {
        IFindFluent<Transaction, Transaction> GetTransactions(FilterDefinition<Transaction> filter, int? skip, int? length);
        IFindFluent<Transaction, Transaction> GetTotalTransactions(FilterDefinition<Transaction> filter);
    }
}