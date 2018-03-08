using GH.Core.Models;

namespace GH.Core.ViewModels
{
    public class FolloweeParameter
    {
        public Account Account { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
    }
}