using GH.Core.Exceptions;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.Web.Http;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using NLog;
using MongoDB.Bson;

namespace GH.Web.Areas.User.Controllers
{
    [AllowAnonymous]
    [BaseApi]
    [RoutePrefix("Api/Ucb")]
    public class UserCreatedBusinessApiController : BaseApiController
    {
        private IUserCreatedBusinessService _ucbService;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public UserCreatedBusinessApiController()
        {
            _ucbService = new UserCreatedBusinessService();
        }

        [HttpPost, Route("New")]
        public IHttpActionResult NewUcb(UserCreatedBusinessModel model)
        {
            var fromAccount = Account; //_accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            string creatorId = null;
            if (fromAccount != null)
            {
                creatorId = fromAccount.AccountId;
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error transmitting model"));
            }

            var ucb = new UserCreatedBusiness
            {
                CreatorRole = model.CreatorRole,
                Created = DateTime.Now,
                CreatorId = creatorId,
                Status = model.status,
                Name = model.name,
                Industry = model.industry,
                Country = model.country,
                City = model.city,
                Address = model.address,
                PhoneNumber = model.phone,
                Email = model.email,
                Website = model.website,
                Avatar = model.avatar,
                Description = model.description,
            };

            UserCreatedBusiness newUcb = _ucbService.CreateUcb(ucb);

            return Ok();
        }

        [HttpPut, Route("update")]
        public IHttpActionResult Update([FromBody] UserCreatedBusinessModel model)
        {
            _ucbService.UpdateUcb(new UserCreatedBusiness
            {
                Id = ObjectId.Parse(model.id),
                CreatorRole = model.CreatorRole,
                Created = model.Created,
                CreatorId = model.CreatorId,
                Status = model.status,
                Name = model.name,
                Industry = model.industry,
                Country = model.country,
                City = model.city,
                Address = model.address,
                PhoneNumber = model.phone,
                Email = model.email,
                Website = model.website,
                Avatar = model.avatar,
                Description = model.description,
            });

            return Ok();
        }

        [HttpPost]
        [Route("changestatus")]
        [AllowAnonymous]
        public IHttpActionResult ChangeStatus([FromBody] Dictionary<string,string> dic)
        {
            _ucbService.ChangeStatus(dic["id"], dic["status"]);

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [AllowAnonymous]
        public IHttpActionResult Delete(string id)
        {
            _ucbService.Delete(id);

            return Ok();
        }
        
        
        
        [HttpGet, Route("Profile/{id}")]
        public HttpResponseMessage GetUcb(string id)
        {
            try
            {
                var ucb = _ucbService.GetUcbById(id);

                if (ucb == null)
                    return Request.CreateApiErrorResponse("Business not found");


                return Request.CreateSuccessResponse(new UserCreatedBusinessModel(ucb),
                    $"Found businesses");
            }
            catch
            {
                return Request.CreateApiErrorResponse("Search error", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet, Route("Search")]
        [AllowAnonymous]
        public IHttpActionResult SearchUcb(string keyword="", string status= "approved", int start = 0,
            int length = 5)
        {
            List<UserCreatedBusiness> ucbs = _ucbService.SearchUcb(keyword, status, start, length);

            return Ok(ucbs.Select(u => new UserCreatedBusinessModel(u)).ToList());
        }

        private MultipartFormDataStreamProvider GetMultipartProvider()
        {
            var uploadFolder = "~/";
            var root = HttpContext.Current.Server.MapPath(uploadFolder);
            Directory.CreateDirectory(root + "..\\GH.Web\\Content\\UploadImages");
            return new MultipartFormDataStreamProvider(root);
        }
        
        [HttpPost, Route("Avatar/{id}")]
        public async Task<HttpResponseMessage> UploadImage(string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported image format"));
            }

            var provider = GetMultipartProvider();
            var result = await Request.Content.ReadAsMultipartAsync(provider);
            var currentUser = Account;
            string userId = id;
            var m = userId;
            string fileName = "";
            foreach (var fileData in provider.FileData)
            {
                fileName = fileData.Headers.ContentDisposition.FileName;
                if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                {
                    fileName = fileName.Trim('"');
                }
                if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                {
                    fileName = Path.GetFileName(fileName);
                }
                string extension = Path.GetExtension(fileName);
                fileName = "_ucb_profile_" + m + "" + extension;
                var uploadFolder = "~/Content/UploadImages"; // you could put this to web.config
                var StoragePath = HttpContext.Current.Server.MapPath(uploadFolder);
                if (File.Exists(Path.Combine(StoragePath, fileName)))
                    File.Delete(Path.Combine(StoragePath, fileName));

                File.Move(fileData.LocalFileName, Path.Combine(StoragePath, fileName));
            }
            fileName = "/Content/UploadImages/" + fileName;

            return this.Request.CreateResponse(HttpStatusCode.OK, new { fileName });
        }
            
        [HttpPost, Route("Claim")]
        public HttpResponseMessage ClaimUcb([FromBody] UcbClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateApiErrorResponse("Invalid parameters", HttpStatusCode.BadRequest);
            }
            var result = _ucbService.ClaimUcb(new UserCreatedBusinessService.UcbClaim
            {
                ucbId = model.ucbId,
                ucbName = model.ucbName,
                name = model.name,
                phone = model.phone,
                email = model.email,
                message = model.message,
            }); 
              
            if (!result.Success) return Request.CreateApiErrorResponse("Error sending business claim");
            return Request.CreateSuccessResponse(new { ucbId = model.ucbId }, "Business claim sent");
        }
    }

}