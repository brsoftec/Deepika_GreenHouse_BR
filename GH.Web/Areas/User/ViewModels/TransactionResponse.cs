using System.Collections.Generic;
using GH.Core.ViewModels;

namespace GH.Web.Areas.User.ViewModels
{
    public class TransactionResponse
    {
        public long Total { get; set; }
        public IEnumerable<TransactionViewModel> Transactions { get; set; }
    }
}