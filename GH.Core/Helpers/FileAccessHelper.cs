using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GH.Core.Helpers
{
    public class FileAccessHelper
    {
        public static readonly string[] IMAGE_EXTENSIONS_COMMON = { "jpg", "jpeg", "png" };
        public static readonly string[] IMAGE_EXTENSIONS_TRANSPARENT = { "png" };
        public static readonly string[] IMAGE_EXTENSIONS_NON_TRANSPARENT = { "jpg", "jpeg" };
        public static readonly string[] IMAGE_EXTENSIONS_ALL = { "jpg", "jpeg", "png", "bmp", "gif" };
        public static readonly string[] RUNABLE_EXTENSIONS = { "exe" };
        public static readonly string[] OFFICE_EXTENSIONS = { "doc", "docx", "xls", "xlsx" };
        public static readonly string[] ZIP_EXTENSION = { "zip", "rar" };

        public static FileInfo SaveHttpPostedFileBase(HttpPostedFileBase file, string httpDirectoryPath, string fileFullName)
        {
            try
            {
                string serverDirectory = GetServerMapPath(httpDirectoryPath);
                if (!Directory.Exists(serverDirectory))
                {
                    Directory.CreateDirectory(serverDirectory);
                }
                string fullPath = Path.Combine(serverDirectory, fileFullName);
                file.SaveAs(fullPath);
                return new FileInfo(fullPath);
            }
            catch (Exception ex)
            {
                string deleteFile = Path.Combine(GetServerMapPath(httpDirectoryPath), fileFullName);
                if (File.Exists(deleteFile))
                {
                    File.Delete(deleteFile);
                }
                throw ex;
            }
        }

        public static FileInfo SaveBase64String(string base64, string httpDirectoryPath, string fileFullName)
        {
            try
            {
                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                byte[] bytes = Convert.FromBase64String(base64);
                string serverDirectory = GetServerMapPath(httpDirectoryPath);
                if (!Directory.Exists(serverDirectory))
                {
                    Directory.CreateDirectory(serverDirectory);
                }
                string fullPath = Path.Combine(serverDirectory, fileFullName);
                File.WriteAllBytes(fullPath, bytes);
                return new FileInfo(fullPath);
            }
            catch (Exception ex)
            {
                string deleteFile = Path.Combine(GetServerMapPath(httpDirectoryPath), fileFullName);
                if (File.Exists(deleteFile))
                {
                    File.Delete(deleteFile);
                }
                throw ex;
            }
        }

        public static FileInfo SaveMultipartFileData(MultipartFileData file, string httpDirectoryPath, string fileFullName)
        {
            try
            {
                string serverDirectory = GetServerMapPath(httpDirectoryPath);
                if (!Directory.Exists(serverDirectory))
                {
                    Directory.CreateDirectory(serverDirectory);
                }
                string fullPath = Path.Combine(serverDirectory, fileFullName);
                File.Move(file.LocalFileName, fullPath);
                return new FileInfo(fullPath);
            }
            catch (Exception ex)
            {
                string deleteFile = Path.Combine(GetServerMapPath(httpDirectoryPath), fileFullName);
                if (File.Exists(file.LocalFileName))
                {
                    File.Delete(file.LocalFileName);
                }
                if (File.Exists(deleteFile))
                {
                    File.Delete(deleteFile);
                }
                throw ex;
            }
        }

        public static FileInfo MoveFile(string source, string destination, string httpDirectoryPath)
        {
            try
            {
                string serverDirectory = GetServerMapPath(httpDirectoryPath);
                if (!Directory.Exists(serverDirectory))
                {
                    Directory.CreateDirectory(serverDirectory);
                }
                File.Move(source, destination);

                return new FileInfo(destination);
            }
            catch (Exception ex)
            {
                if (File.Exists(source))
                {
                    File.Delete(source);
                }
                else if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                throw ex;
            }
        }

        public static FileInfo ResizeImage(FileInfo file, string httpDirectoryPath, string fileFullName, double maxWidth = 800, double maxHeight = 800)
        {
            var byteImageIn = File.ReadAllBytes(file.FullName);

            byte[] currentByteImageArray = byteImageIn;
            MemoryStream inputMemoryStream = new MemoryStream(byteImageIn);
            Image fullsizeImage = Image.FromStream(inputMemoryStream);

            double width = fullsizeImage.Width * 1.0;
            double height = fullsizeImage.Height * 1.0;
            double ratioWidth = width > maxWidth ? maxWidth / width : 1.0;
            double ratioHeight = height > maxHeight ? maxHeight / height : 1.0;
            double ratio = ratioWidth > ratioHeight ? ratioHeight : ratioWidth;

            int newWidth = Convert.ToInt32(width * ratio);
            int newHeight = Convert.ToInt32(height * ratio);

            Bitmap fullSizeBitmap = new Bitmap(fullsizeImage, new Size(newWidth, newHeight));
            MemoryStream resultStream = new MemoryStream();

            fullSizeBitmap.Save(resultStream, fullsizeImage.RawFormat);

            currentByteImageArray = resultStream.ToArray();
            resultStream.Dispose();
            resultStream.Close();

            string fullPath = Path.Combine(GetServerMapPath(httpDirectoryPath), fileFullName);

            File.WriteAllBytes(fullPath, currentByteImageArray);

            return new FileInfo(fullPath);
        }

        public static void DeleteFile(string httpFilePath)
        {
            string fullPath = GetServerMapPath(httpFilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public static void DeleteFileWithAbsoluteFilePath(string absoluteFilePath)
        {
            if (File.Exists(absoluteFilePath))
            {
                File.Delete(absoluteFilePath);
            }
        }

        public static string GetServerMapPath(string httpPath)
        {
            return CommonFunctions.MapPath(httpPath);
        }

        public static bool IsValidExtension(string[] allowExtensions, string fileName)
        {
            string ext = Path.GetExtension(fileName.ToLower());
            ext = ext.TrimStart('.');
            return allowExtensions.Contains(ext);
        }

        public static void DeleteDirectory(string directoryHttpPath)
        {
            string fullPath = GetServerMapPath(directoryHttpPath);
            if (Directory.Exists(fullPath))
            {
                ClearDirectory(fullPath);
                Directory.Delete(fullPath);
            }
        }

        public static void DeleteDirectoryWithAbsolutePath(string directoryAbsolutePath)
        {
            if (Directory.Exists(directoryAbsolutePath))
            {
                ClearDirectory(directoryAbsolutePath);
                Directory.Delete(directoryAbsolutePath);
            }
        }

        private static void ClearDirectory(string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearDirectory(di.FullName);
                di.Delete();
            }
        }

        public static string CombineHttpPathWithFileName(string httpDirectory, string fileFullName)
        {
            httpDirectory = httpDirectory.TrimStart('~');
            httpDirectory = httpDirectory.TrimStart('/');
            httpDirectory = httpDirectory.TrimEnd('/');

            return "/" + httpDirectory + "/" + fileFullName;
        }
    }
}