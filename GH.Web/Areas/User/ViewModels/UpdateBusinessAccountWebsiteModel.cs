using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class UpdateBusinessAccountWebsiteModel
    {
        [Required, Url]
        public string Website { get; set; }
    }
}