using GH.Core.BlueCode.Entity.ManageTokenDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IManageTokenDeviceBusinessLogic
    {
        ManageTokenDevice GetByTokenId(string id);
        List<ManageTokenDevice> GetListManageTokenDeviceByAccountId(string accountId);
        string Insert(ManageTokenDevice tokenDevice);
        void DeleteByTokenId(string id);
       
        void Update(ManageTokenDevice tokenDivice);

        void UpdateStatus(string accountId, string tokenDivice, string status);
        ManageTokenDevice GetByTokenDevice(string tokenDevice);
        List<ManageTokenDevice> GetListByTokenDevice(string tokenDevice);
    }
}