using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface IAdminService
    {
        void syncaccount(string accountId, string action);
    }
}