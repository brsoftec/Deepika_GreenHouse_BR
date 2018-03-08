using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;

namespace GH.Core.Services
{
    public interface ICmsImageService
    {
        IEnumerable<CmsImage> Search(int? start = null, int? length = null);

        CmsImage Upload(MultipartFileData file, string uploaderId);

        void Delete(int imageId);
    }
}