using System.Collections.Generic;
using System.Web.Security;
using GH.Core.Models;

namespace GH.Web.Areas.User.ViewModels
{
    public class AccountInfoViewModel
    {
        public string TierView { get; set; }
        public Account ActiveAccount { get; set; }
        public Account UserAccount { get; set; }
        public Account BusinessAccount { get; set; }
        public bool AsBusinessMember { get; set; }
        public string avatarUrlLiteral { get; set; }
        public string displayNameLiteral { get; set; }
        public List<Role> Roles { get; set; }
    }
    public class Amount
    {
        public int Start { get; set; }
        public int Take { get; set; }
    }
}