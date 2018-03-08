using GH.Core.BlueCode.Entity.Request;
using GH.Core.ViewModels;
using MongoDB.Bson;

namespace GH.Core.Adapters
{
    public static class RequestAdapter
    {
        public static Request RequestViewModelToRequest(RequestViewModel req)
        {
            var rs = new Request();
            if(string.IsNullOrEmpty(req.Id))
               rs.Id = ObjectId.GenerateNewId();
            else
                rs.Id = new MongoDB.Bson.ObjectId(req.Id);
            rs.CreatedDate = req.CreatedDate;
            rs.FromUserId = req.FromUserId;
            rs.ToUserId = req.ToUserId;
            rs.Email = req.Email;
            rs.FirstName = req.FirstName;
            rs.Phone = req.Phone;
            rs.LastName = req.LastName;
            rs.Message = req.Message;
            rs.InteractionId = req.InteractionId;
            rs.Status = req.Status;
            rs.Type = req.Type;

            return rs;
        }
        public static RequestViewModel RequestToRequestViewModel(Request req)
        {
            var rs = new RequestViewModel();
            rs.Id = req.Id.ToString();
            rs.CreatedDate = req.CreatedDate;
            rs.FromUserId = req.FromUserId;
            rs.Email = req.Email;
            rs.FirstName = req.FirstName;
            rs.LastName = req.LastName;
            rs.Phone = req.Phone;
            rs.ToUserId = req.ToUserId;
            rs.Message = req.Message;
            rs.InteractionId = req.InteractionId;
            rs.Status = req.Status;
            rs.Type = req.Type;
            return rs;
        }
    }
}