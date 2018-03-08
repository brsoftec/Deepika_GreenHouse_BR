using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class SearchGalleryFileCriteria
    {
        public string Keyword { get; set; }
        public int FolderId { get; set; }
        public int Start { get; set; }
        public int? Length { get; set; }
    }
}