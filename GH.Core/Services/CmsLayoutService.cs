using GH.Core.Helpers;
using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public class CmsLayoutService : ICmsLayoutService
    {
        private GreenHouseDbContext _context;

        public CmsLayoutService(GreenHouseDbContext context)
        {
            _context = context;
        }

        public CmsLayout Get(int id)
        {
            return _context.CmsLayouts.Find(id);
        }

        public ICollection<CmsLayout> Search(string keyword, int? start, int? length, out int total, string sortName = null, bool isDesc = false)
        {
            var query = _context.CmsLayouts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(q => q.Name.Contains(keyword));
            }

            total = query.Count();

            if (string.IsNullOrEmpty(sortName))
            {
                query = query.OrderByDescending(q => q.Id);
            }
            else
            {
                string orderMethod = isDesc ? "OrderByDescending" : "OrderBy";
                var expression = LinQHelper.GenerateMethodCall<CmsLayout>(query, orderMethod, sortName);
                query = query.Provider.CreateQuery<CmsLayout>(expression);
            }

            if (start.HasValue && length.HasValue && start.Value >= 0 && length.Value >= 0)
            {
                query = query.Skip(start.Value).Take(length.Value);
            }

            return query.ToList();
        }

        public CmsLayout Add(CmsLayout layout, string creatorId)
        {

            if (layout.Zones.Any(z => layout.Zones.Any(z2 => z2 != z && z.Name == z2.Name)))
            {
                throw new Exception("Zone name cannot duplicate");
            }

            var now = DateTime.Now;

            var creator = _context.Users.Find(creatorId);

            layout.CreatedDate = now;
            layout.ModifiedDate = now;

            layout.Creator = creator;
            layout.Modifier = creator;

            _context.CmsLayouts.Add(layout);
            _context.SaveChanges();

            return layout;
        }

        public CmsLayout Update(CmsLayout layout, string modifierId)
        {
            var existLayout = Get(layout.Id);

            if (existLayout.Default)
            {
                throw new Exception("Cannot modified default layout");
            }

            if (layout.Zones.Any(z => layout.Zones.Any(z2 => z2 != z && z.Name == z2.Name)))
            {
                throw new Exception("Zone name cannot duplicate");
            }

            var now = DateTime.Now;

            existLayout.Description = layout.Description;
            existLayout.HtmlPreview = layout.HtmlPreview;
            existLayout.Name = layout.Name;
            existLayout.Path = layout.Path;

            var modifier = _context.Users.Find(modifierId);

            existLayout.ModifiedDate = now;
            existLayout.Modifier = modifier;

            var updateZoneIds = layout.Zones.Where(z => z.Id > 0).Select(z => z.Id);

            foreach (var zone in layout.Zones.Where(z => z.Id > 0))
            {
                var existZone = existLayout.Zones.FirstOrDefault(z => z.Id == zone.Id);
                existZone.Name = zone.Name;
                existZone.Description = zone.Description;
            }

            var newZones = layout.Zones.Where(z => z.Id <= 0);
            foreach (var zone in newZones)
            {
                zone.Layout = existLayout;
            }

            var deleteZones = existLayout.Zones.Where(z => !updateZoneIds.Contains(z.Id));

            _context.CmsZoneContents.RemoveRange(deleteZones.SelectMany(z => z.Contents != null ? z.Contents : new List<CmsZoneContent>()));
            _context.CmsZones.RemoveRange(deleteZones);

            _context.CmsZones.AddRange(newZones);

            _context.SaveChanges();

            return existLayout;
        }

        public void Delete(int layoutId)
        {
            var existLayout = Get(layoutId);

            if (existLayout.Default)
            {
                throw new Exception("Cannot modified default layout");
            }

            var defaultLayout = _context.CmsLayouts.FirstOrDefault(l => l.Default);

            _context.CmsPages.Where(p => p.DraftLayout.Id == layoutId).ToList().ForEach((page) =>
            {
                page.DraftLayout = defaultLayout;
            });

            _context.CmsPages.Where(p => p.Layout.Id == layoutId).ToList().ForEach((page) =>
            {
                page.Layout = defaultLayout;
            });

            _context.CmsZoneContents.RemoveRange(existLayout.Zones.SelectMany(z => z.Contents != null ? z.Contents : new List<CmsZoneContent>()));

            _context.CmsZones.RemoveRange(existLayout.Zones);

            _context.CmsLayouts.Remove(existLayout);

            _context.SaveChanges();
        }
    }
}