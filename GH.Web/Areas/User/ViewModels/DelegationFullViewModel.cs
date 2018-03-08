using GH.Core.BlueCode.Entity.Delegation;
using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class DelegationFullViewModel : TransactionalInformation
    {
        public DelegationFullViewModel()
        {
            Listitems = new List<DelegationItemViewModel>();
        }

        public IList<DelegationItemViewModel> Listitems { set; get; }

        public IEnumerable<MyFriend> ListFrienditems { set; get; }
        // Vu
        public List<MyFriend> ListFriend { set; get; }
       

        // End Vu

        public string Direction { set; get; }

        public string DelegationRole { set; get; }

        public string AccountId { set; get; }

        public object DelegationItemTemplate { set; get; }

        public string DelegationId { set; get; }

        public DelegationItemTemplate DelegationItemTemplateInsert { set; get; }


    }
}