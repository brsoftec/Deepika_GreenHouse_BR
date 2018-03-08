using System.Collections.Generic;
using GH.Core.ViewModels;

namespace GH.Web.Areas.User.ViewModels
{
    public class FolloweeResponse
    {
        public long Total { get; set; }
        public List<FolloweeViewModel> Data { get; set; }
    }
}