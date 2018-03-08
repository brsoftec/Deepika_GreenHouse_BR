using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.ViewModels
{
    public class BusinessPostViewModel
    {

        public BusinessPostViewModel()
        {
            Photos = new List<Photo>();
        }

        public string Id { get; set; }
        public string Message { get; set; }
        public AccountViewModel Creator { get; set; }
        public List<Photo> Photos { get; set; }
        public PostPrivacy Privacy { get; set; }
        public List<string> SocialTypes { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public List<BusinessPostStateFLowViewModel> Workflows { get; set; }
    }

    public class BusinessPostStateFLowViewModel
    {
        public DateTime ExecuteTime { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
        public AccountViewModel Executor { get; set; }
    }
}