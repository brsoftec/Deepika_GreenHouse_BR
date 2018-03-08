using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class VerifyOTPViewModel
    {
        [Required]
        public string RequestId { get; set; }

        [Required]
        public string Code { get; set; }
        public bool Verify { get; set; }
        public string Status { get; set; }
    }
}