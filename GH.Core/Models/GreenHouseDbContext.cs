using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class GreenHouseDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<CmsImage> CmsImages { get; set; }
        public DbSet<CmsLayout> CmsLayouts { get; set; }
        public DbSet<CmsPage> CmsPages { get; set; }
        public DbSet<CmsZone> CmsZones { get; set; }
        public DbSet<CmsZoneContent> CmsZoneContents { get; set; }

        public DbSet<GalleryFolder> GalleryFolders { get; set; }
        public DbSet<GalleryFile> GalleryFiles { get; set; }


        public GreenHouseDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static GreenHouseDbContext Create()
        {
            return new GreenHouseDbContext();
        }
    }
}