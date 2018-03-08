using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class DocumentVaultViewModel
    {
        public string UserId { set; get; }
        public string FileName { set; get; }
       
        public string SaveName { set; get; }
        public string Category { set; get; }
        public string Path { set; get; }
        public string UploadDate { set; get; }
        public string ExpiredDate { set; get; }
        public string Message { get; set; }
        public bool NoSearch { get; set; }
        public string Status { get; set; }
    }
}