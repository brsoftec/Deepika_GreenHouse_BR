using System.Collections.Generic;

namespace GH.Core.ViewModels
{
    public class DisableUserResult
    {
        public long Total { get; set; }
        public List<DisableUserViewModel> DisableUsers { get; set; }
    }
}