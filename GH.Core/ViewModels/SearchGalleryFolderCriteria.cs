using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SearchGalleryFolderCriteria
    {
        public string Keyword { get; set; }
        public GalleryType GalleryType { get; set; }

        public int Start { get; set; }
        public int? Length { get; set; }
    }
}