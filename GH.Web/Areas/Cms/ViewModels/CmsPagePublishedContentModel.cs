using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.Cms.ViewModels
{
    public class CmsPagePublishedContentModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Content { get; set; }

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public DateTime? PublishedDate { get; set; }

        public IList<CmsZoneContentViewModel> Zones { get; set; }

        public string LayoutPath { get; set; }

    }
}