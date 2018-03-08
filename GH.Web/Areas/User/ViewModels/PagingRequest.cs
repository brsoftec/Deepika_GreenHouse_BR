using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class PagingRequest
    {
        public int? Start { get; set; }
        public int? Length { get; set; }
    }
}