using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class RemoveFriendModel
    {
        [Required]
        public string FriendId { get; set; }
        [Required]
        public string NetworkId { get; set; }
    }
}