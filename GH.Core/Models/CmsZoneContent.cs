using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class CmsZoneContent
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public virtual CmsZone Zone { get; set; }
    }
}