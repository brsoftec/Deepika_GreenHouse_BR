using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.PostHandShake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class PostHandShakeModel : TransactionalInformation
    {
        public string CampaignId { set; get; }

        public bool IsJoin { set; get; }

        public string Status { set; get; }

        public string Userid { set; get; }

        public List<PostHandShake> List { set; get; }

        public string userinvitedid { set; get; }
        public string userinvitedemail { set; get; }

        public string invitetype { set; get; }

        public string posthandshakecomment { set; get; }

        public string posthandshakeid { set; get; }

        public List<FieldinformationVault> listinformations { set; get; }

    }
}