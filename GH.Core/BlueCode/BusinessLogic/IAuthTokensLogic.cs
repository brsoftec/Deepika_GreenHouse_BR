using System.Threading.Tasks;
using GH.Core.BlueCode.Entity.AuthToken;
using GH.Core.Models;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IAuthTokensLogic
    {
        AuthToken GetById(string id);
        AuthToken GetByToken(string tokenStr);
        string Insert(AuthToken authToken);

        Task<FuncResult> PostToFcmAsync(Account account, Account fromAccount, string type, string title, string body,
            object payload);

        FuncResult InsertToken(AuthToken token);
        Task<FuncResult> InsertTokenAsync(AuthToken token);
        Task<FuncResult> GetFcmTokensByAccountIdAsync(string accountId);

        Task<FuncResult> CloseTokenAsync(string token);
    }
}