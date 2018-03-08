using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class GalleryFolder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public GalleryType GalleryType { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public string PreviewImage { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public virtual ApplicationUser Modifier { get; set; }

        public virtual ICollection<GalleryFile> Files { get; set; }
    }

    public enum GalleryType
    {
        Image, Video
    }
}