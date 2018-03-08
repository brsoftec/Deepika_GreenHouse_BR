using System.Collections.Generic;
using GH.Core.Models;

namespace GH.Core.ViewModels
{
    public class FolloweeResult
    {
        public long Total { get; set; }
        public List<Account> Followee { get; set; }
    }
}