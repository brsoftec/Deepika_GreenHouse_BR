using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GH.Core.Services
{
    public interface IGalleryFileService
    {
        ICollection<GalleryFile> Search(SearchGalleryFileCriteria criteria, out int total);
        GalleryFile Get(int id);
        GalleryFile Add(GalleryFile file, MultipartFileData uploadFile, string userId);
        GalleryFile Update(GalleryFile file, string userId);
        void Delete(int fileId);
    }
};