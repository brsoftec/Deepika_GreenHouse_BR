using GH.Core.Extensions;

namespace GH.Core.Models
{
    public class Transaction : MongoEntity
    {
        public TransactionType TransactionType { get; set; }
        public FollowTransaction FollowTransaction { get; set; }
    }
}