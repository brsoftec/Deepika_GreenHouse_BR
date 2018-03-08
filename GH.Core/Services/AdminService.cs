using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using NLog;

namespace GH.Core.Services
{
    public class AdminService : IAdminService
    {
        private IAccountService _accountService;
                private Logger log = LogManager.GetCurrentClassLogger();
        public void syncaccount(string accountId, string action)
        {
            HttpClient client = new HttpClient();
           client.GetAsync("http://regit.sg:5610/api/syncaccount/" + accountId + "?action=" + action + "&environment=" + ConfigurationManager.AppSettings["Environment"]);
 //            client.GetAsync("http://localhost:5610/api/syncaccount/" + accountId + "?action=" + action + "&environment=" + ConfigurationManager.AppSettings["Environment"]);
            log.Debug("Synced data: " + action);
        }
     
    }
}