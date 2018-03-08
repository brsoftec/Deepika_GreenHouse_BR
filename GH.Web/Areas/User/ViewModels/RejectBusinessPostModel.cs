using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class RejectBusinessPostModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}