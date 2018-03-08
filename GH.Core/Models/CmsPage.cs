using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class CmsPage
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        public string Content { get; set; }

        public string DraftContent { get; set; }

        [StringLength(255)]
        public string MetaTitle { get; set; }

        [StringLength(255)]
        public string DraftMetaTitle { get; set; }

        [StringLength(255)]
        public string MetaDescription { get; set; }

        [StringLength(255)]
        public string DraftMetaDescription { get; set; }

        public DateTime? PublishedDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public virtual ApplicationUser Modifier { get; set; }

        [Required, MaxLength(255)]
        public string PermanentLink { get; set; }

        public bool HasDraft { get; set; }

        public virtual ICollection<CmsZoneContent> Zones { get; set; }

        public virtual ICollection<CmsZoneContent> DraftZones { get; set; }

        public virtual CmsLayout Layout { get; set; }

        public virtual CmsLayout DraftLayout { get; set; }
    }
}