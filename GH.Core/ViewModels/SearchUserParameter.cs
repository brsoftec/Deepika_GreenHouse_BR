using GH.Core.Helpers;
using GH.Core.Models;

namespace GH.Core.ViewModels
{
    public class SearchUserParameter
    {
        public string SearchText { get; set; }
        public AccountType AccountType { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public FilterType FilterType { get; set; }
    }
}