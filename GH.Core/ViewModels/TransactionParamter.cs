using GH.Core.Extensions;

namespace GH.Core.ViewModels
{
    public class TransactionParamter
    {
        public TransactionType? TransactionType { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public FollowTransactionParamter FollowTransactionParamter { get; set; }
    }
}