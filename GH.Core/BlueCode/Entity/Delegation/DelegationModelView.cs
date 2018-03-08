using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.Models;

namespace GH.Core.BlueCode.Entity.Delegation
{
    public class DelegationModelView : TransactionalInformation
    {
        public DelegationModelView()
        {
            Listitems = new List<DelegationItemTemplate>();
        }
       
        public IList<DelegationItemTemplate> Listitems { set; get; }
        public IEnumerable<MyFriend> ListFrienditems { set; get; }
        public List<MyFriend> ListFriend { set; get; }
       
        public string Direction { set; get; }

        public string DelegationRole { set; get; }

        public string AccountId { set; get; }

        public object DelegationItemTemplate { set; get; }

        public string DelegationId { set; get; }

        public DelegationItemTemplate DelegationItemTemplateInsert { set; get; }


    }   

}