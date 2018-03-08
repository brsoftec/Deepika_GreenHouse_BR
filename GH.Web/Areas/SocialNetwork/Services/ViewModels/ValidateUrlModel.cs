using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.Services.ViewModels
{
    public class ValidateUrlModel
    {
        [Required, Url]
        public string Url { get; set; }
    }
}