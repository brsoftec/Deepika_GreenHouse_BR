using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface ICmsPageService
    {
        /// <summary>
        /// Get page by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CmsPage Get(int id);
        /// <summary>
        /// Get page by permanentLink
        /// </summary>
        /// <param name="permanentLink"></param>
        /// <returns></returns>
        CmsPage Get(string permanentLink);
        /// <summary>
        /// Search page by keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="total"></param>
        /// <param name="sortName"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        ICollection<CmsPage> Search(string keyword, int? start, int? length, out int total, string sortName = null, bool isDesc = false);

        CmsPage Add(CmsPage page, bool publish, string creatorId);
        CmsPage Update(CmsPage page, bool publish, string modifierId);
        CmsPage Unpublished(int pageId, string modifierId);
        void Delete(int pageId);
        bool IsUrlAvailable(string link, int? ignorePageId = null);
    }
}