using GH.Core.BlueCode.Entity.ManualHandshake;
using System.Collections.Generic;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IManualHandshakeBusinessLogic
    {
        ManualHandshake GetById(string id);
        List<ManualHandshake> GetListByAccountId(string accountId);
        List<ManualHandshake> GetActiveListByAccountId(string accountId);
        List<ManualHandshake> GetActiveListPagingByAccountId(string accountId, int start = 0, int take = 10);
        List<ManualHandshake> GetActiveListPagingByToAccountId(string toAccountId, int start = 0, int take = 10);
        List<ManualHandshake> GetListPagingByAccountId(string accountId, int start = 0, int take = 10);
        List<ManualHandshake> GetListPagingByToAccountId(string toAccountId, int start = 0, int take = 10);
        List<ManualHandshake> GetActiveByEmail(string email, int start = 0, int take = 10);
        List<ManualHandshake> GetActiveByToEmail(string toEmail, int start = 0, int take = 10);
  
        List<ManualHandshake> GetListByToAccountId(string toAccountId);
        List<ManualHandshake> GetListByToEmail(string email);

        string Insert(ManualHandshake manualHandshake);
        void DeleteById(string id);
        string Update(ManualHandshake manualHandshake);
    }
}