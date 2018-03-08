using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class AccountBusinessRolePage
    {
        public AccountBusinessRolePage()
        {
            PageAllows = new List<string>();
        }
        public string RoleName { set; get; }

        public List<string> PageAllows { set; get; }

    }


}