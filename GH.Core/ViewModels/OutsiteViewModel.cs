using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class OutsiteViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Option { get; set; }
        public string[] ListEmail { get; set; }
        public bool SendMe { get; set; }
        public string FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string CompnentId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
    }
}