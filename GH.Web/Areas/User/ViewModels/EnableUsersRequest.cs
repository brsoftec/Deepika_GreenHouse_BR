using GH.Core.ViewModels;
using System;
using System.Collections.Generic;


namespace GH.Web.Areas.User.ViewModels
{
    public class EnableUsersRequest
    {
        public List<DisableUserViewModel> DisableUsers { get; set; }
    }
}