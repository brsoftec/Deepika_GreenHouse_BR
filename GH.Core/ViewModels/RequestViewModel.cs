using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class RequestViewModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string ToDisplayName { get; set; }
        public string ToEmail { get; set; }
        public string ToAvatarUrl { get; set; }
        public string ToObjectUserId { get; set; }
        public string Message { get; set; }
        public string InteractionId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}