using GH.Core.IRepositories;
using GH.Core.Models;
using MongoDB.Driver;

namespace GH.Core.Repositories
{
    public class TransactionRepository : EntityRepository<Transaction>, ITransactionRepository
    {
        public override UpdateResult Update(Transaction entity)
        {
            throw new System.NotImplementedException();
        }

        public IFindFluent<Transaction, Transaction> GetTransactions(FilterDefinition<Transaction> filter, int? skip,
            int? length)
        {
            return MongoConnectionHandler.MongoCollection.Find(filter).Skip(skip).Limit(length);
        }

        public IFindFluent<Transaction, Transaction> GetTotalTransactions(FilterDefinition<Transaction> filter)
        {
            return MongoConnectionHandler.MongoCollection.Find(filter);
        }
    }
}