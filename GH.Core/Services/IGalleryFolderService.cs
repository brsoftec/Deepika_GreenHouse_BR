using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GH.Core.Services
{
    public interface IGalleryFolderService
    {
        GalleryFolder Get(int id);
        ICollection<GalleryFolder> Search(SearchGalleryFolderCriteria criteria, out int total);
        GalleryFolder Add(GalleryFolder folder, MultipartFileData file, string userId);
        GalleryFolder Update(GalleryFolder folder, MultipartFileData file, string userId);
        void Delete(int folderId);
    }
}