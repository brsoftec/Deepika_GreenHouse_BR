using GH.Core.Helpers;
using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace GH.Core.Services
{
    public class CmsPageService : ICmsPageService
    {
        private GreenHouseDbContext _context;

        public CmsPageService(GreenHouseDbContext context)
        {
            _context = context;
        }

        public CmsPage Get(int id)
        {
            return _context.CmsPages.SingleOrDefault(c => c.Id == id);
        }

        public CmsPage Get(string permanentLink)
        {
            return _context.CmsPages.SingleOrDefault(c => c.PermanentLink.ToLower() == permanentLink.ToLower());
        }

        public ICollection<CmsPage> Search(string keyword, int? start, int? length, out int total, string sortName = null, bool isDesc = false)
        {
            var query = _context.CmsPages.AsQueryable();

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
                var expression = LinQHelper.GenerateMethodCall<CmsPage>(query, orderMethod, sortName);
                query = query.Provider.CreateQuery<CmsPage>(expression);
            }

            if (start.HasValue && length.HasValue && start.Value >= 0 && length.Value >= 0)
            {
                query = query.Skip(start.Value).Take(length.Value);
            }

            return query.ToList();
        }

        public CmsPage Add(CmsPage page, bool publish, string creatorId)
        {
            var now = DateTime.Now;

            var layout = _context.CmsLayouts.Find(page.DraftLayout.Id);

            if (layout == null)
            {
                throw new Exception("Layout not found");
            }

            if (!this.IsUrlAvailable(page.PermanentLink))
            {
                throw new Exception("Permanent link is invalid or not available");
            }

            page.DraftLayout = layout;
            page.Layout = layout;

            if (page.DraftZones != null)
            {
                page.Zones = new List<CmsZoneContent>();
                foreach (var zone in page.DraftZones)
                {
                    zone.Zone = layout.Zones.FirstOrDefault(z => z.Id == zone.Zone.Id);
                    if (zone.Zone == null)
                    {
                        throw new Exception("Zone not found");
                    }

                    page.Zones.Add(new CmsZoneContent
                    {
                        Content = zone.Content,
                        Zone = zone.Zone
                    });
                }
            }

            page.Content = page.DraftContent;
            page.MetaDescription = page.DraftMetaDescription;
            page.MetaTitle = page.DraftMetaTitle;

            if (publish)
            {
                page.HasDraft = false;
                page.PublishedDate = now;
            }
            else
            {
                page.HasDraft = true;
                page.PublishedDate = null;
            }

            var creator = _context.Users.Find(creatorId);


            page.CreatedDate = now;
            page.ModifiedDate = now;

            page.Creator = creator;
            page.Modifier = creator;

            _context.CmsPages.Add(page);

            _context.SaveChanges();

            return page;
        }

        public CmsPage Update(CmsPage page, bool publish, string modifierId)
        {
            var existPage = Get(page.Id);

            var layout = _context.CmsLayouts.Find(page.DraftLayout.Id);

            if (layout == null)
            {
                throw new Exception("Layout not found");
            }
            
            if (!this.IsUrlAvailable(page.PermanentLink, page.Id))
            {
                throw new Exception("Permanent link is invalid or not available");
            }

            if (existPage.DraftLayout.Id != layout.Id)
            {
                _context.CmsZoneContents.RemoveRange(existPage.DraftZones);
                existPage.DraftLayout = layout;

                if (publish)
                {
                    _context.CmsZoneContents.RemoveRange(existPage.Zones);
                    existPage.Layout = layout;
                }

                if (page.DraftZones != null)
                {
                    foreach (var zone in page.DraftZones)
                    {
                        zone.Zone = layout.Zones.FirstOrDefault(z => z.Id == zone.Zone.Id);
                        if (zone.Zone == null)
                        {
                            throw new Exception("Zone not found");
                        }

                        if (publish)
                        {
                            existPage.Zones.Add(new CmsZoneContent
                            {
                                Content = zone.Content,
                                Zone = zone.Zone
                            });
                        }
                    }
                }
            }
            else
            {
                if (publish)
                {
                    _context.CmsZoneContents.RemoveRange(existPage.Zones);
                    existPage.Layout = layout;
                }

                if (page.DraftZones != null)
                {
                    foreach (var zone in page.DraftZones)
                    {
                        var existZone = existPage.DraftZones.FirstOrDefault(z => z.Zone.Id == zone.Zone.Id);
                        if (existZone == null)
                        {
                            existZone = zone;
                            existZone.Zone = layout.Zones.FirstOrDefault(z => z.Id == existZone.Zone.Id);
                            existPage.DraftZones.Add(existZone);
                        }

                        if (existZone.Zone == null)
                        {
                            throw new Exception("Zone not found");
                        }

                        existZone.Content = zone.Content;

                        if (publish)
                        {
                            var existPublishedZone = existPage.Zones.FirstOrDefault(z => z.Zone.Id == zone.Zone.Id);
                            if (existPublishedZone == null)
                            {
                                existPublishedZone = new CmsZoneContent
                                {
                                    Content = existZone.Content,
                                    Zone = existZone.Zone
                                };
                                existPage.Zones.Add(existPublishedZone);
                            }
                            else
                            {
                                existPublishedZone.Content = existZone.Content;
                            }
                        }
                    }
                }
            }

            var now = DateTime.Now;

            if (publish)
            {
                existPage.Content = page.DraftContent;
                existPage.MetaDescription = page.DraftMetaDescription;
                existPage.MetaTitle = page.DraftMetaTitle;
                existPage.HasDraft = false;
                existPage.PublishedDate = now;
            }

            existPage.DraftContent = page.DraftContent;
            existPage.DraftMetaDescription = page.DraftMetaDescription;
            existPage.DraftMetaTitle = page.DraftMetaTitle;
                        
            if (existPage.Content != existPage.DraftContent || existPage.MetaDescription != existPage.DraftMetaDescription || existPage.MetaTitle != existPage.DraftMetaTitle || existPage.Layout.Id != existPage.DraftLayout.Id
                || existPage.Zones.Any(z => !existPage.DraftZones.Any(d => d.Zone.Id == z.Zone.Id && d.Content == z.Content)))
            {
                existPage.HasDraft = true;
            }

            existPage.PermanentLink = page.PermanentLink;

            var modifier = _context.Users.Find(modifierId);

            existPage.Name = page.Name;

            existPage.ModifiedDate = now;
            existPage.Modifier = modifier;

            _context.SaveChanges();

            return existPage;
        }

        public CmsPage Unpublished(int pageId, string modifierId)
        {
            var existPage = Get(pageId);
            existPage.PublishedDate = null;

            var modifier = _context.Users.Find(modifierId);

            var now = DateTime.Now;

            existPage.ModifiedDate = now;
            existPage.Modifier = modifier;

            _context.SaveChanges();
            return existPage;
        }

        public void Delete(int pageId)
        {
            var exist = Get(pageId);
            if (exist == null)
            {
                throw new Exception("Page not found");
            }

            _context.CmsZoneContents.RemoveRange(exist.Zones);
            _context.CmsZoneContents.RemoveRange(exist.DraftZones);
            _context.CmsPages.Remove(exist);
            _context.SaveChanges();
        }

        public bool IsUrlAvailable(string link, int? ignorePageId = null)
        {
            if (!IsValidPermanentLink(link))
            {
                return false;
            }

            var filePath = CommonFunctions.MapPath("~/" + link);
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                return false;
            }

            link = link.ToLower();
            if (ignorePageId.HasValue)
            {
                return !_context.CmsPages.Any(p => p.Id != ignorePageId && p.PermanentLink.ToLower() == link);
            }
            else
            {
                return !_context.CmsPages.Any(p => p.PermanentLink.ToLower() == link);
            }
        }

        private bool IsValidPermanentLink(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                return false;
            }
            Regex regex = new Regex(@"^[a-zA-Z]{1}(\w*(\-\w)*)*(\/[a-zA-Z]{1}(\w*(\-\w)*)*)*$");
            return regex.IsMatch(link);
        }
    }
}