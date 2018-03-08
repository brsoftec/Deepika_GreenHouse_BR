using System.Collections.Generic;
using GH.Core.Models;

namespace GH.Core.ViewModels
{
    public class TransactionResult
    {
        public long Total { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}