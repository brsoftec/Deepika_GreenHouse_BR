using GH.Core.BlueCode.Entity.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IRequestBusinessLogic
    {
        Request GetById(string id);
        List<Request> GetList(string type = null, string status = null);
        List<Request> GetListByEmail(string email);
        List<Request> GetListByFromUserId(string fromUserId, string status = null);
        List<Request> GetListByToUserId(string toUserId);
        List<Request> GetListByToUserIdStatus(string toUserId, string status = null);
        string Insert(Request request);
        void DeleteById(string id);
        string Update(Request request);
    }
}