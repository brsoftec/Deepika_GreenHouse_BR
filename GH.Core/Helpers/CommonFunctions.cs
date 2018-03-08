using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GH.Core.Helpers
{
    public static class CommonFunctions
    {
        public static string MapPath(string path)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(path);
            }

            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var directoryName = Path.GetDirectoryName(absolutePath);
            return Path.Combine(directoryName, ".." + path.TrimStart('~').Replace('/', '\\'));
        }

        public static string GetAppHostDomain()
        {
            return HttpContext.Current.Request.Url.Scheme + System.Uri.SchemeDelimiter + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port);
        }

        public static bool JsonHasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType.Name == "JsonObject")
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }

        public static ObjectId ParseToObjectId(this string id)
        {
            return new ObjectId(id);
        }

        public static bool ParseBool(this string str)
        {
            var bol = false;
            bool.TryParse(str, out bol);
            return bol;
        }

        public static int ParseInt(this string value){
            int i = 0;
            Int32.TryParse(value, out i);
            return i;
        }

        public static byte[] DownLoadImageFromURL(string url)
        {
            try
            {
                var webclient = new WebClient();
                byte[] data = webclient.DownloadData(url);
                return data;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }
        public static string ToMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        public static string CreateSHA512Hash(string Phrase)
        {
            SHA512Managed HashTool = new SHA512Managed();
            Byte[] PhraseAsByte = System.Text.Encoding.UTF8.GetBytes(string.Concat(Phrase));
            Byte[] EncryptedBytes = HashTool.ComputeHash(PhraseAsByte);
            HashTool.Clear();
            return Convert.ToBase64String(EncryptedBytes);
        }
        //
        public static string GenerateSaltedSHA1(string plainTextString)
        {
            HashAlgorithm algorithm = new SHA1Managed();
            var saltBytes = GenerateSalt(4);
            var plainTextBytes = Encoding.ASCII.GetBytes(plainTextString);

            var plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            var saltedSHA1Bytes = algorithm.ComputeHash(plainTextWithSaltBytes);
            var saltedSHA1WithAppendedSaltBytes = AppendByteArray(saltedSHA1Bytes, saltBytes);

            return Convert.ToBase64String(saltedSHA1WithAppendedSaltBytes);
        }
        private static byte[] GenerateSalt(int saltSize)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[saltSize];
            rng.GetBytes(buff);
            return buff;
        }
        private static byte[] AppendByteArray(byte[] byteArray1, byte[] byteArray2)
        {
            var byteArrayResult =
                    new byte[byteArray1.Length + byteArray2.Length];

            for (var i = 0; i < byteArray1.Length; i++)
                byteArrayResult[i] = byteArray1[i];
            for (var i = 0; i < byteArray2.Length; i++)
                byteArrayResult[byteArray1.Length + i] = byteArray2[i];

            return byteArrayResult;
        }



    }
}