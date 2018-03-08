using GH.Core.BlueCode.Entity.Outsite;
using System.Collections.Generic;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IOutsiteBusinessLogic
    {
        string InsertOutsite(Outsite outsite);
        List<Outsite> LoadOutsiteByEmail(string email);
        List<Outsite> LoadOutsiteByFromUserId(string fromUserId);
        Outsite GetOutsiteById(string id);
        void DeleteOutsiteById(string id);
        Outsite GetOutsiteByUserId(string userId, string type = null);
        List<Outsite> GetListOutsiteByUserId(string userId, string type = null, string compnentId = null);
        string UpdateOutsite(Outsite outsite);
        Outsite GetOutsiteByCompnentId(string compnentId = null, string type = null);
    }
}