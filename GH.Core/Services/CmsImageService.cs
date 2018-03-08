using GH.Core.Helpers;
using GH.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Web;

namespace GH.Core.Services
{
    public class CmsImageService : ICmsImageService
    {
        public const string UPLOAD_DIRECTORY = "~/Content/CmsUpload/Images";

        private GreenHouseDbContext _context;

        public CmsImageService(GreenHouseDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CmsImage> Search(int? start = null, int? length = null)
        {
            var query = _context.CmsImages.OrderByDescending(i => i.Id);
            if (start.HasValue && length.HasValue && start.Value >= 0 && length.Value > 0)
            {
                return query.Skip(start.Value).Take(length.Value);
            }
            else
            {
                return query.ToList();
            }
        }

        public CmsImage Upload(MultipartFileData file, string uploaderId)
        {
            string currentPath = file.LocalFileName;

            try
            {

                var localFileName = file.Headers.ContentDisposition.FileName;
                if (localFileName.StartsWith("\"") && localFileName.EndsWith("\""))
                {
                    localFileName = localFileName.Trim('"');
                }

                if (localFileName.Contains(@"/") || localFileName.Contains(@"\"))
                {
                    localFileName = Path.GetFileName(localFileName);
                }

                string name = Path.GetFileNameWithoutExtension(localFileName);
                string extension = Path.GetExtension(localFileName);

                string fullname = DateTime.Now.Ticks + "_" + name + extension;

                string virtualPath = UPLOAD_DIRECTORY + "/" + fullname;

                string absolutePath = CommonFunctions.MapPath(virtualPath);

                string directoryAbsolutePath = CommonFunctions.MapPath(UPLOAD_DIRECTORY);
                if (!Directory.Exists(directoryAbsolutePath))
                {
                    Directory.CreateDirectory(directoryAbsolutePath);
                }

                File.Move(file.LocalFileName, absolutePath);
                currentPath = absolutePath;

                var model = new CmsImage
                {
                    Path = virtualPath.Trim('~'),
                    Uploader = _context.Users.Find(uploaderId)
                };

                _context.CmsImages.Add(model);

                _context.SaveChanges();

                return model;
            }
            catch (Exception)
            {
                File.Delete(currentPath);
                throw;
            }
        }

        public void Delete(int imageId)
        {
            var image = _context.CmsImages.Find(imageId);

            string absolutePath = CommonFunctions.MapPath("~" + image.Path);
            if(File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }

            _context.CmsImages.Remove(image);
            _context.SaveChanges();
        }
    }
}