using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class CmsZone
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public virtual CmsLayout Layout { get; set; }

        public virtual ICollection<CmsZoneContent> Contents { get; set; }
    }
}