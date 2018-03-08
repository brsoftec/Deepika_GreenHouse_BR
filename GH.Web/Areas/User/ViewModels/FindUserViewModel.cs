using MongoDB.Bson;

namespace GH.Web.Areas.User.ViewModels
{
    public class FindUserViewModel
    {
        public ObjectId Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
    }
}