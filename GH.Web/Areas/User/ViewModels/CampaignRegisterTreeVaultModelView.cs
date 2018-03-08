using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{

    public class CampaignRegisterTreeVaultModelView : TransactionalInformation
    {
        public CampaignRegisterTreeVaultModelView()
        {

        }

        public object TreeVault { set; get; }
    }
}