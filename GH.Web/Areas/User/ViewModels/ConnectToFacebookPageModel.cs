using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ConnectToFacebookPageModel
    {
        [Required(ErrorMessage="Please choose a page")]
        public string Id { get; set; }
    }
}