using System.Collections.Generic;
using GH.Core.Models;
using GH.Core.ViewModels;

namespace GH.Core.IServices
{
    public interface ITransactionService
    {
        Transaction InsertTransaction(FollowTransactionParamter paramter);
        TransactionResult GetFollowTransactions(TransactionParamter paramter);
    }
}
