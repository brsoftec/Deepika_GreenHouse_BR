using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class CmsImage
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(255)]
        public string Path { get; set; }
        public virtual ApplicationUser Uploader { get; set; }
    }
}