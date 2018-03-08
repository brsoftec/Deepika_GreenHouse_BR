using System.Collections.Generic;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using GH.Core.Models;

namespace GH.Core.Services
{
    public interface IUserCreatedBusinessService
    {
        UserCreatedBusiness CreateUcb(UserCreatedBusiness ucb);
        UserCreatedBusiness UpdateUcb(UserCreatedBusiness ucb);
        UserCreatedBusiness GetUcbById(string id);
        List<UserCreatedBusiness> SearchUcb(string keyword="", string status = "", int start = 0, int length = 5);
        FuncResult ClaimUcb(UserCreatedBusinessService.UcbClaim claim);
        void ChangeStatus(string id, string status);
        void Delete(string id);
    }
}