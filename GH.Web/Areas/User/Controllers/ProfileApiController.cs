using System;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GH.Core.Models;
using java.lang;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [ApiAuthorize]
    [RoutePrefix("Api/Profile")]
    public class ProfileApiController : BaseApiController
    {
        static readonly IAccountService AccountService = new AccountService();
        static readonly IUserService UserService = new UserService();

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ProfileApiController()
        {
        }


        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SetAccountStatusModel
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }  
        [HttpPost, Route("Status")]
        public async Task<HttpResponseMessage> SetAccountStatusAsync(SetAccountStatusModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Key))
                return Request.CreateApiErrorResponse("Missing status key", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Value))
                return Request.CreateApiErrorResponse("Missing value", HttpStatusCode.BadRequest);
            var key = model.Key;
            var value = model.Value;

            var result = await AccountService.SetAccountStatusAsync(key, value, AccountId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "profile.update.not":
                        return Request.CreateSuccessResponse(new
                        {
                            key = key,
                            status = "notUpdated"
                        }, "No change, skipped update");
                }

                return Request.CreateApiErrorResponse("Error updating account status", error: "account.status.update.error");
            }

            return Request.CreateSuccessResponse(new
            {
                key,
                status = "updated"
            }, "Account status updated successfully");
        }
        
        
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SetProfileModel
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public bool IsInitial { get; set; }
        }      
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SetProfileManyModel
        {
            public IList<SetProfileModel> Properties { get; set; }
        }
        [HttpPost, Route("Set")]
        public async Task<HttpResponseMessage> SetProfile(SetProfileModel model)
        {
            if (model == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Key))
                return Request.CreateApiErrorResponse("Missing profile key", HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(model.Value))
                return Request.CreateApiErrorResponse("Missing value", HttpStatusCode.BadRequest);
            var key = model.Key.ToLower();
            var value = model.Value;

            if (key == "gender")
            {
                value = value.ToLower();
                if (value != "male" && value != "female" && value != "other")
                    return Request.CreateApiErrorResponse($"Invalid value: Gender accepts Male, Female or Other",
                        error: "profile.value.invalid");
            }
            else if (key == "dob")
            {
                if (!Regex.IsMatch(value, @"\d\d\d\d-\d\d-\d\d"))
                    return Request.CreateApiErrorResponse($"Invalid value: DOB expects 'YYYY-MM-DD' string",
                        error: "profile.value.invalid");
                if (!DateTime.TryParse(value, out var dob))
                    return Request.CreateApiErrorResponse($"Invalid value: DOB rejects invalid date",
                        error: "profile.value.invalid");
                if (dob.Year < 1900 || dob.Year > 2017)
                    return Request.CreateApiErrorResponse($"Invalid value: DOB rejects invalid year",
                        error: "profile.value.invalid");
                value = dob.ToString("o");
            }

            var result = await AccountService.SetProfile(key, value, AccountId);
            if (!result.Success)
            {
                switch (result.Status)
                {
                    case "profile.key.invalid":
                        return Request.CreateApiErrorResponse($"Invalid key: {model.Key}",
                            error: "profile.key.invalid");
                    case "profile.update.not":
                        return Request.CreateSuccessResponse(new
                        {
                            key = key,
                            status = "notUpdated"
                        }, "No change, skipped update");
                }

                return Request.CreateApiErrorResponse("Error updating profile", error: "profile.update.error");
            }

            if (model.IsInitial && key == "country")
            {
                await AccountService.SetAccountStatusAsync("LocationDetected", false, AccountId);
            }

            return Request.CreateSuccessResponse(new
            {
                key = key,
                status = "updated"
            }, "Profile updated successfully");
        }
        
        [HttpPost, Route("SetMany")]
        public async Task<HttpResponseMessage> SetProfileMany(SetProfileManyModel model)
        {
            if (model == null || model.Properties == null)
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);

            var props = model.Properties;
            if (props.Count <= 0) 
                    return Request.CreateApiErrorResponse("Missing profile property list", HttpStatusCode.BadRequest);

            foreach (var prop in model.Properties)
            {
                if (string.IsNullOrEmpty(prop.Key))
                    return Request.CreateApiErrorResponse("Missing profile key", HttpStatusCode.BadRequest);
                if (string.IsNullOrEmpty(prop.Value))
                    return Request.CreateApiErrorResponse("Missing value", HttpStatusCode.BadRequest);
                var key = prop.Key.ToLower();
                var value = prop.Value;

                if (key == "gender")
                {
                    value = value.ToLower();
                    if (value != "male" && value != "female" && value != "other")
                        return Request.CreateApiErrorResponse($"Invalid value: Gender accepts Male, Female or Other",
                            error: "profile.value.invalid");
                }
                else if (key == "dob")
                {
                    if (!Regex.IsMatch(value, @"\d\d\d\d-\d\d-\d\d"))
                        return Request.CreateApiErrorResponse($"Invalid value: DOB expects 'YYYY-MM-DD' string",
                            error: "profile.value.invalid");
                    if (!DateTime.TryParse(value, out var dob))
                        return Request.CreateApiErrorResponse($"Invalid value: DOB rejects invalid date",
                            error: "profile.value.invalid");
                    if (dob.Year < 1900 || dob.Year > 2017)
                        return Request.CreateApiErrorResponse($"Invalid value: DOB rejects invalid year",
                            error: "profile.value.invalid");
                    value = dob.ToString("o");
                }

                var result = await AccountService.SetProfile(key, value, AccountId);
            }

            return Request.CreateSuccessResponse(new
            {
                status = "updated"
            }, "Profile updated successfully");
        }

        [HttpPost, Route("Picture")]
        public async Task<HttpResponseMessage> UploadPictureAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateApiErrorResponse("Invalid or unsupported file format",
                    HttpStatusCode.UnsupportedMediaType, "upload.invalid");
            }

            var filePath = $"~/Content/ProfilePictures";
            var provider = new MultipartFormDataStreamProvider(HttpContext.Current.Server.MapPath("~/Content/UploadImages"));
            await Request.Content.ReadAsMultipartAsync(provider);
            
            var fileData = provider.FileData.FirstOrDefault();
            if (fileData == null)
                return Request.CreateApiErrorResponse("Missing file",
                    HttpStatusCode.BadRequest, "upload.missing");
            
            var fileName = fileData.Headers.ContentDisposition.FileName;
            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
            {
                fileName = fileName.Trim('"');
            }

            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
            {
                fileName = Path.GetFileName(fileName);
            }

            var fileExt = Path.GetExtension(fileName);
            var newFileName = AccountId + "_profile_pic_" + DateTime.Now.Ticks + fileExt;

            try
            {
                var storagePath = HttpContext.Current.Server.MapPath(filePath);
                File.Move(fileData.LocalFileName, Path.Combine(storagePath, newFileName));
            }
            catch
            {
                return Request.CreateApiErrorResponse("Error uploading profile picture",
                    HttpStatusCode.InternalServerError, "upload.error");
            }

            var url = filePath.Trim('~') + "/" + newFileName;
            var result = await AccountService.SetProfile("picture", url, AccountId);
            if (!result.Success)
            {
                return Request.CreateApiErrorResponse("Error updating profile picture", 
                    error: "profile.update.error");
            }
            
            return Request.CreateSuccessResponse(new
            {
                pictureUrl = url,
                status = "updated"
            }, "Profile picture updated");
        }
    }
}