using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface ICmsLayoutService
    {
        /// <summary>
        /// Get layout by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CmsLayout Get(int id);
        /// <summary>
        /// Search layout by keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="total"></param>
        /// <param name="sortName"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        ICollection<CmsLayout> Search(string keyword, int? start, int? length, out int total, string sortName = null, bool isDesc = false);

        CmsLayout Add(CmsLayout layout, string creatorId);
        CmsLayout Update(CmsLayout layout, string modifierId);
        void Delete(int layoutId);
    }
}