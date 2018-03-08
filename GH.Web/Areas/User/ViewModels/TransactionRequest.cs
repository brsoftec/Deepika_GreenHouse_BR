using GH.Core.Extensions;

namespace GH.Web.Areas.User.ViewModels
{
    public class TransactionRequest
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public TransactionType? TransactionType { get; set; }
        public int? Length { get; set; }
    }
}