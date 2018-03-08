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
    public class GalleryFileService : IGalleryFileService
    {
        private GreenHouseDbContext _context;

        public const string GALLERY_UPLOAD_IMAGE = "~/Content/GalleryUpload/Pictures";
        public const string GALLERY_UPLOAD_VIDEO = "~/Content/GalleryUpload/Videos";

        public GalleryFileService(GreenHouseDbContext context)
        {
            _context = context;
        }

        public ICollection<GalleryFile> Search(SearchGalleryFileCriteria criteria, out int total)
        {
            var query = _context.GalleryFiles.Where(f => f.Folder.Id == criteria.FolderId);

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

        public GalleryFile Get(int id)
        {
            return _context.GalleryFiles.SingleOrDefault(f => f.Id == id);
        }

        public GalleryFile Add(GalleryFile file, MultipartFileData uploadFile, string userId)
        {
            var folder = _context.GalleryFolders.SingleOrDefault(f => f.Id == file.Folder.Id);

            if (folder == null)
            {
                throw new CustomException("Folder not found");
            }

            file.Folder = folder;

            //process file name to get file extension
            var localFileName = uploadFile.Headers.ContentDisposition.FileName;
            if (localFileName.StartsWith("\"") && localFileName.EndsWith("\""))
            {
                localFileName = localFileName.Trim('"');
            }

            string fileExtension = Path.GetExtension(localFileName);
            
            //generate an unique file name
            var now = DateTime.Now;
            string name = now.ToString("ddMMyyyyhhmmss") + "_" + now.Ticks + Guid.NewGuid().ToString().Replace("-","");
            string thumbnail = name + "_thumbnail" + fileExtension;
            name = name + fileExtension;

            string directory = file.Folder.GalleryType == GalleryType.Image ? GALLERY_UPLOAD_IMAGE : GALLERY_UPLOAD_VIDEO;
            //move file to gallery upload folder
            var fileInfo = FileAccessHelper.SaveMultipartFileData(uploadFile, directory, name);
                      
            try
            {
                if (file.Folder.GalleryType == GalleryType.Image && fileExtension.ToLower() != ".svg")
                {
                    FileAccessHelper.ResizeImage(fileInfo, directory, thumbnail);
                    file.CompressedPath = FileAccessHelper.CombineHttpPathWithFileName(directory, thumbnail);
                }

                file.CreatedDate = now;
                file.ModifiedDate = now;

                var user = _context.Users.Find(userId);
                file.Creator = user;
                file.Modifier = user;

                file.Path = FileAccessHelper.CombineHttpPathWithFileName(directory, name);

                _context.GalleryFiles.Add(file);
                _context.SaveChanges();

                return file;
            }
            catch (Exception)
            {
                FileAccessHelper.DeleteFileWithAbsoluteFilePath(fileInfo.FullName);
                if (!string.IsNullOrEmpty(file.CompressedPath))
                {
                    FileAccessHelper.DeleteFile(file.CompressedPath);
                }
                throw;
            }
        }

        public GalleryFile Update(GalleryFile file, string userId)
        {
            throw new NotImplementedException();
        }

        public void Delete(int fileId)
        {
            throw new NotImplementedException();
        }
    }
}