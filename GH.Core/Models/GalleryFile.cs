using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class GalleryFile
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        public string CompressedPath { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ApplicationUser Creator { get; set; }
        public virtual ApplicationUser Modifier { get; set; }

        public virtual GalleryFolder Folder { get; set; }
    }
}