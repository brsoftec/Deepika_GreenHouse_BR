using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class FullFeedBackModel
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string UserIP { get; set; }
        //UserLocal
        public string UserLocal { get; set; }
        public string Device { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public string Component { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string FeedBackURL { get; set; }

        //
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
       
        public string PhotoUrl { get; set; }
    
    }
}