using GH.Core.Exceptions;
using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GH.Core.Services
{
    public class GalleryFolderService : IGalleryFolderService
    {
        private GreenHouseDbContext _context;
        public const string GALLERY_UPLOAD_PREVIEW = "~/Content/GalleryUpload/Pictures";

        public GalleryFolderService(GreenHouseDbContext db)
        {
            _context = db;
        }

        public GalleryFolder Get(int id)
        {
            return _context.GalleryFolders.SingleOrDefault(f => f.Id == id);
        }

        public ICollection<GalleryFolder> Search(SearchGalleryFolderCriteria criteria, out int total)
        {
            var query = _context.GalleryFolders.Where(f => f.GalleryType == criteria.GalleryType);
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                criteria.Keyword = criteria.Keyword.ToLower();
                query = query.Where(f => f.Name.ToLower().Contains(criteria.Keyword) || f.Description.ToLower().Contains(criteria.Keyword));
            }

            total = query.Count();

            if (criteria.Start < 0 || !criteria.Length.HasValue || criteria.Length <= 0)
            {
                return query.OrderBy(f => f.Name).ToList();
            }
            else
            {
                return query.OrderBy(f => f.Name).Skip(criteria.Start).Take(criteria.Length.Value).ToList();
            }            
        }

        public GalleryFolder Add(GalleryFolder folder, MultipartFileData file, string userId)
        {
            var now = DateTime.Now;

            FileInfo fileInfo = null;
            string fileName = null;
            if (file != null)
            {
                //process file name to get file extension
                var localFileName = file.Headers.ContentDisposition.FileName;
                if (localFileName.StartsWith("\"") && localFileName.EndsWith("\""))
                {
                    localFileName = localFileName.Trim('"');
                }

                string fileExtension = Path.GetExtension(localFileName);

                //generate an unique file name
                fileName = now.ToString("ddMMyyyyhhmmss") + "_" + now.Ticks + Guid.NewGuid().ToString().Replace("-", "") + fileExtension;

                //move file to gallery upload folder
                fileInfo = FileAccessHelper.SaveMultipartFileData(file, GALLERY_UPLOAD_PREVIEW, fileName);
            }

            //validate for folder name
            if (_context.GalleryFolders.Any(f => f.Name.ToLower() == folder.Name.ToLower() && f.GalleryType == folder.GalleryType))
            {
                if (fileInfo != null)
                {
                    FileAccessHelper.DeleteFileWithAbsoluteFilePath(fileInfo.FullName);
                }
                throw new CustomException("Folder name is already existed");
            }

            try
            {
                folder.CreatedDate = now;
                folder.ModifiedDate = now;

                var user = _context.Users.Find(userId);
                folder.Creator = user;
                folder.Modifier = user;

                if (fileInfo != null)
                {
                    folder.PreviewImage = FileAccessHelper.CombineHttpPathWithFileName(GALLERY_UPLOAD_PREVIEW, fileName);
                }

                _context.GalleryFolders.Add(folder);
                _context.SaveChanges();

                return folder;
            }
            catch (Exception)
            {
                if (fileInfo != null)
                {
                    FileAccessHelper.DeleteFileWithAbsoluteFilePath(fileInfo.FullName);
                }                
                throw;
            }
        }

        public GalleryFolder Update(GalleryFolder folder, MultipartFileData file, string userId)
        {
            throw new NotImplementedException();
        }

        public void Delete(int folderId)
        {
            throw new NotImplementedException();
        }

    }
}