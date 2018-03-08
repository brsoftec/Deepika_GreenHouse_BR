
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.FeedBack;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.Exceptions;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/FeedBackService")]
    public class FeedBackApiController : ApiController
    {

        private IAccountService _accountService;

        private IFeedBackBusinessLogic _feedBackBusinessLogic;
        public FeedBackApiController()
        {
            _accountService = new AccountService();
            _feedBackBusinessLogic = new FeedBackBusinessLogic();
        }
        [HttpGet, Route("GetFeedBack")]
        public async Task<List<FeedBackEntity>> GetFeedBack()
        {
            try
            {
                string accountId = ValidateCurrentUser();
                var lst = _feedBackBusinessLogic.LoadFeedBack(accountId);
                return lst;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        //FullFeedBackModel
        //public string UserId { get; set; }
        //public string UserIP { get; set; }
        ////UserLocal
        //public string UserLocal { get; set; }
        //public string Device { get; set; }
        //public string Name { get; set; }

        //public DateTime DateCreate { get; set; }
        //public string Description { get; set; }
        //public string Attachment { get; set; }
        //public string Component { get; set; }
        //public string Type { get; set; }
        //public string Status { get; set; }
        //public string FeedBackURL { get; set; }
        [HttpGet, Route("GetFullFeedBack")]
        public async Task<List<FullFeedBackModel>> GetFullFeedBack()

        {
            try
            {
                string accountId = ValidateCurrentUser();
                var lst = _feedBackBusinessLogic.LoadFeedBack(accountId);
                List<FullFeedBackModel> lstFull = new List<FullFeedBackModel>();
                FullFeedBackModel ffe;


                for (int i= 0; i< lst.Count; i++)
                {
                    try
                    {
                        var account = _accountService.GetByAccountId(lst[i].UserId);
                        if(!string.IsNullOrEmpty(account.AccountId))
                        {
                            ffe = new FullFeedBackModel();
                            ffe.Id = lst[i].Id;
                            ffe.UserId = lst[i].UserId;
                            if (!string.IsNullOrEmpty(lst[i].UserIP))
                                ffe.UserIP = lst[i].UserIP;
                            if (!string.IsNullOrEmpty(lst[i].UserLocal))
                                ffe.UserLocal = lst[i].UserLocal;

                            ffe.Device = lst[i].Device;

                            ffe.Name = lst[i].Name;
                            ffe.DateCreate = lst[i].DateCreate;
                            ffe.Description = lst[i].Description;
                            ffe.Attachment = lst[i].Attachment;

                            ffe.Component = lst[i].Component;
                            ffe.Type = lst[i].Type;
                            ffe.Status = lst[i].Status;
                            ffe.FeedBackURL = lst[i].FeedBackURL;

                            ffe.PhotoUrl = account.Profile.PhotoUrl;
                            ffe.DisplayName = account.Profile.DisplayName;
                            ffe.Email = account.Profile.Email;

                            lstFull.Add(ffe);
                        }
                       
                    }
                    catch { }
                   
                }
                return lstFull;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [HttpPost, Route("DeleteFeedBack")]
        public async Task DeleteFeedBack(FeedBackEntity model)
        {
            _feedBackBusinessLogic.DeleteFeedBack(model);
        }

        [HttpPost, Route("InsertFeedBack")]
        public async Task InsertFeedBack(FeedBackEntity model)
        {
            model.UserId = ValidateCurrentUser();
            _feedBackBusinessLogic.InsertFeedBack(model);
        }
        private string ValidateCurrentUser()
        {
            var accountId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(accountId))
                throw new CustomException("Can not find current user.");
            return accountId;
        }
       
        //document
        [Route("AttachFile")]
        [HttpPost]
        public HttpResponseMessage AttachFile(string fileName)
        {

            var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/FeedBack/"));

            if (!directory.Exists)
            {
                directory.Create();
            }

            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                   // var fileName = DateTime.Now.Ticks.ToString() +  httpRequest.Files[file].FileName;
                    
                    string filePath = HostingEnvironment.MapPath(Path.Combine("~/Content/FeedBack/", fileName));
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);
                }
                result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }


     
    }
}