using System.Collections.Generic;
using GH.Core.BlueCode.Entity.Message;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IMessageBusinessLogic
    {
        void SendMessage(PersonalMessage personalMessage);
        IEnumerable<PersonalMessage> ViewPersonalMessage(string accountId);
        IEnumerable<PersonalMessage> GetPersonalMessageList(string userId, int pageIndex = 1, int pageSize = 10);
    }
}
