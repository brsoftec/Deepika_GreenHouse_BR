using System.Net.Http;
using GH.Web.Areas.User.ViewModels;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using System.Net;
using System.Linq;
using GH.Core.Services;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Web;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Threading.Tasks;
using GH.Core.BlueCode.Entity.Notification;
using MongoDB.Bson;
using RegitSocial.Business.Notification;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Util;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/InformationVaultService")]
    public class InformationVaultController : ApiController
    {
        public IInfomationVaultBusinessLogic InfomationVaultBusinessLogic { get; set; }
        public IAccountService AccountService;
        public IProfileBusinessLogic ProfileBusinessLogic { get; set; }
        public IBusinessMemberLogic BusinessMemberLogic { get; set; }
        public IPostBusinessLogic PostBusinessLogic { get; set; }

        public InformationVaultController()
        {
            InfomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
            ProfileBusinessLogic = new ProfileBusinessLogic();
            AccountService = new AccountService();
            BusinessMemberLogic = new BusinessMemberLogic();
            PostBusinessLogic = new PostBusinessLogic();
        }

        [Route("GetVaultInformation")]
        [HttpPost]
        public IHttpActionResult GetVaultInformation(VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            var currentUser = AccountService.GetByAccountId(User.Identity.GetUserId());
            string userid = currentUser.AccountId.ToString();

            string jsoninfomationVault = InfomationVaultBusinessLogic.GetInformationVaultJsonByUserId(userid);
            vm.VaultInformation = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsoninfomationVault);
            vm.ReturnStatus = true;

            //WRITE LOG
            if (vm != null)
            {
                var _accountService = new AccountService();
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordVault)
                {
                    string title = "You accessed your infomation vault.";
                    string type = "vault";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }

            return Ok(vm);
        }

        //Hoang Vu
        [HttpPost, Route("GetInfoVaultToJson")]
        public IHttpActionResult GetInforVaultToJson(VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            var currentUser = AccountService.GetByAccountId(User.Identity.GetUserId());
            string userid = "";
            if (string.IsNullOrEmpty(vm.UserId))
                userid = currentUser.AccountId.ToString();
            else
                userid = vm.UserId;

            string jsoninfomationVault = InfomationVaultBusinessLogic.GetInformationVaultJsonByUserId(userid);
            vm.VaultInformation = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsoninfomationVault);
            vm.ReturnStatus = true;

            return Ok(vm);
        }

        [HttpPost, Route("GetJsonFromInformationvaultId")]
        public IHttpActionResult GetJsonFromInformationvaultId(VaultInformationModelView vm)
        {
            try
            {
                string jsoninfomationVault =
                    InfomationVaultBusinessLogic.GetJsonFromInformationvaultId(vm.VaultInformationId);
                vm.VaultInformation =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsoninfomationVault);
            }
            catch
            {
            }
            vm.ReturnStatus = true;

            return Ok(vm);
        }

        [HttpGet, Route("GetCurrentUserId")]
        public IHttpActionResult GetCurrentUserId()
        {
            var _accountService = new AccountService();
            var account = _accountService.GetByAccountId(User.Identity.GetUserId());
            string userId = "";
            if (string.IsNullOrEmpty(account.AccountId))
                userId = account.AccountId;
            return Ok(userId);
        }

        [Route("SaveVaultInformation")]
        [HttpPost]
        public IHttpActionResult SaveVaultInformation(VaultInformationModelView vm)
        {
            if (vm == null)
                return BadRequest();


            try
            {
                var _accountService = new AccountService();
                var accountid = new InfomationVaultBusinessLogic().GetInformationVaultById(vm.UserId)["userId"]
                    .AsString;

                InfomationVaultBusinessLogic.SaveInformationVault(vm.UserId, vm.StrVaultInformation);


                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] {"Update successfully"}.ToList();
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] {"Update fail", ex.ToString()}.ToList();
            }

            //WRITE LOG
            if (vm != null)
            {
                var _accountService = new AccountService();
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordVault)
                {
                    string title = "You changed infomation vault in Regit.";
                    string type = "Keep a record of information vault operations";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }

            return Ok(vm);
        }

        [Route("UpdateInformationVaultById")]
        [HttpPost]
        public IHttpActionResult UpdateInformationVaultById(VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            try
            {
                string userId = vm.UserId;

                var userAccount = AccountService.GetByAccountId(User.Identity.GetUserId());

                if (string.IsNullOrEmpty(userId))
                {
                    userId = userAccount.AccountId;
                }

                //check valid
                for (int i = 0; i < vm.Listvaults.Count(); i++)
                {
                    if (vm.Listvaults[i].type == "date" && vm.Listvaults[i].model == "0001-01-01T00:00:00.000Z")
                        vm.Listvaults[i].model = null;
                }

                InfomationVaultBusinessLogic.UpdateInformationVaultById(userId, vm.Listvaults);

                if (vm.status == "Accecpt" || vm.status == "Deny")
                {
                    var businessAccount = AccountService.GetByAccountId(vm.BusinessUserId);

                    var usernotifycation = businessAccount;

                    var notificationMessage = new NotificationMessage();
                    notificationMessage.Id = ObjectId.GenerateNewId();
                    notificationMessage.Type = vm.status == "Accecpt"
                        ? EnumNotificationType.NotifyPushToVaultAccecpt
                        : EnumNotificationType.NotifyPushToVaultDeny;
                    notificationMessage.FromAccountId = userAccount.AccountId;
                    notificationMessage.FromUserDisplayName = userAccount.Profile.DisplayName;
                    notificationMessage.ToAccountId = usernotifycation.AccountId;
                    notificationMessage.ToUserDisplayName = usernotifycation.Profile.DisplayName;
                    notificationMessage.PreserveBag = vm.CampaignId;
                    var notificationBus = new NotificationBusinessLogic();
                    notificationBus.SendNotification(notificationMessage);
                }


                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] {"Update successfully"}.ToList();
            }
            catch
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] {"Update fail"}.ToList();
            }

            //WRITE LOG
            if (vm != null)
            {
                var accountService = new AccountService();
                var account = accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordVault)
                {
                    string title = "You updated your infomation vault.";
                    string type = "vault";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }

            return Ok(vm);
        }


        //UpdateInfoFieldById
        [Route("UpdateInfoFieldById")]
        [HttpPost]
        public IHttpActionResult UpdateInfoFieldById(InfoViewModel info)
        {
            if (info == null)
                info = new InfoViewModel();
            try
            {
                string userId = info.UserId;

                var userAccount = AccountService.GetByAccountId(User.Identity.GetUserId());

                if (string.IsNullOrEmpty(userId))
                {
                    userId = userAccount.AccountId;
                }

                InfomationVaultBusinessLogic.UpdateInfoFieldById(userId, info.infoField);

                info.ReturnStatus = true;
                info.ReturnMessage = new string[] {"Update successfully"}.ToList();
            }
            catch
            {
                info.ReturnStatus = false;
                info.ReturnMessage = new string[] {"Update fail"}.ToList();
            }

            //WRITE LOG
            if (info != null)
            {
                var accountService = new AccountService();
                var account = accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordVault)
                {
                    string title = "You updated infomation vault in Regit.";
                    string type = "Keep a record of information vault operations";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }

            var response = Request.CreateResponse<object>(HttpStatusCode.OK, info);
            return Ok(info);
        }


        #region version2

        [Route("UploadDocument")]
        [HttpPost]
        public IHttpActionResult UploadDocument()
        {
            var rs = new DocumentVaultViewModel();
            var _accountService = new AccountService();
            string userid = HttpContext.Current.User.Identity.GetUserId();
            var directory =
                new DirectoryInfo(HostingEnvironment.MapPath(Path.Combine("~/Content/vault/documents/", userid)));
            if (!directory.Exists)
            {
                directory.Create();
            }

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                try
                {
                    for (var i = 0; i < httpRequest.Files.Count; i++)
                    {
                        string extension = Path.GetExtension(httpRequest.Files[i].FileName);
                        if (extension == ".jpg" || extension == ".bmp" || extension == ".png" || extension == ".gif" ||
                            extension == ".tiff" || extension == ".jpeg")
                        {
                            var postedFile = httpRequest.Files[i];
                            var fileName = postedFile.FileName;
                            try
                            {
                                fileName = InfomationVaultBusinessLogic.CheckFileNameDocument(userid,
                                    postedFile.FileName);
                            }
                            catch
                            {
                            }

                            var rootFolder = "~/" + ConfigHelp.GetStringValue("folderVault");
                            string filePath = HostingEnvironment.MapPath(Path.Combine(rootFolder, userid, fileName));
                            rs.FileName = fileName;
                            postedFile.SaveAs(filePath);
                            docfiles.Add(filePath);
                        }
                        else
                        {
                            rs.Status = "Error";
                            rs.Message = "Unsupported file type" + extension;
                        }
                    }
                }
                catch
                {
                    rs.Status = "Error";
                    rs.Message = "File error";
                }
            }
            else
            {
                rs.Status = "Error";
                rs.Message = "Not find file";
            }
            return Ok(rs);
        }

        [AllowAnonymous]
        [Route("AttachFile")]
        [HttpPost]
        public HttpResponseMessage AttachFile(string userid, string fileName)
        {
            HttpResponseMessage result = null;
            if (string.IsNullOrEmpty(userid))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var directory =
                new DirectoryInfo(HostingEnvironment.MapPath(Path.Combine("~/Content/vault/documents/", userid)));
            if (!directory.Exists)
            {
                directory.Create();
            }

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                for (var i = 0; i < httpRequest.Files.Count; i++)
                {
                    var postedFile = httpRequest.Files[i];
                    string filePath =
                        HostingEnvironment.MapPath(Path.Combine("~/Content/vault/documents/", userid, fileName));
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


        [AllowAnonymous]
        [Route("DownLoadFile")]
        [HttpGet]
        public HttpResponseMessage DownLoadFile(string UserId, string FileName)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(UserId))
                userId = UserId;
            Byte[] bytes = null;
            if (FileName != null)
            {
                try
                {
                    string filePath =
                        HostingEnvironment.MapPath(Path.Combine("~/Content/vault/documents/", userId, FileName));
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    bytes = br.ReadBytes((Int32) fs.Length);
                    br.Close();
                    fs.Close();
                }
                catch
                {
                }
            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            MemoryStream stream = new MemoryStream(bytes);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = FileName
            };
            return (result);
        }

        [AllowAnonymous]
        [HttpPost, Route("DeleteDocumentFile")]
        public async Task DeleteDocumentFile(DocumentVaultViewModel fileDocument)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(fileDocument.UserId))
                userId = fileDocument.UserId;
            if (fileDocument.FileName != null)
            {
                try
                {
                    string filePath = HostingEnvironment.MapPath(Path.Combine("~/Content/vault/documents/", userId,
                        fileDocument.FileName));
                    File.Delete(filePath);
                }
                catch
                {
                }
            }
        }


        [HttpPost, Route("GetFormVault")]
        public IHttpActionResult GetFormVault(FormVaultViewModel formVault)
        {
            var currentUserAccountId = "";
            if (!string.IsNullOrEmpty(formVault.AccountId))
            {
                currentUserAccountId = formVault.AccountId;
            }
            else
                currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();

            string jsonFormVault = "";

            try
            {
                jsonFormVault =
                    InfomationVaultBusinessLogic.GetFormVaultByAccountId(currentUserAccountId, formVault.FormName);
            }
            catch
            {
            }
            var rs = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsonFormVault);

            return Ok(rs);
        }

        [HttpPost, Route("UpdateFormVault")]
        public IHttpActionResult UpdateFormVault(FormVaultViewModel formVault)
        {
            var currentUserAccountId = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(formVault.AccountId))
            {
                currentUserAccountId = formVault.AccountId;
            }

            try
            {
                InfomationVaultBusinessLogic.UpdateFormByAccountId(currentUserAccountId, formVault.FormName,
                    formVault.FormString);
                return Ok(formVault);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost, Route("GetVault")]
        public IHttpActionResult GetVault(VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            var currentUser = AccountService.GetByAccountId(User.Identity.GetUserId());
            string userid = "";
            if (string.IsNullOrEmpty(vm.UserId))
                userid = currentUser.AccountId.ToString();
            else
                userid = vm.UserId;
            string jsoninfomationVault = InfomationVaultBusinessLogic.GetVaultJsonByUserId(userid);
            vm.VaultInformation = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsoninfomationVault);
            vm.ReturnStatus = true;

            return Ok(vm);
        }


        [Route("UpdateVault")]
        [HttpPost]
        public IHttpActionResult UpdateVault(VaultInformationModelView vm)
        {
            if (vm == null)
                vm = new VaultInformationModelView();
            try
            {
                var _accountService = new AccountService();
                InfomationVaultBusinessLogic.SaveVault(vm.UserId, vm.StrVaultInformation);
                var accountid = new InfomationVaultBusinessLogic().GetInformationVaultById(vm.UserId)["userId"]
                    .AsString;
                //Call postHandShake
                new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(accountid);
                vm.ReturnStatus = true;
                vm.ReturnMessage = new string[] {"Update successfully"}.ToList();
            }
            catch (Exception ex)
            {
                vm.ReturnStatus = false;
                vm.ReturnMessage = new string[] {"Update fail"}.ToList();
            }


            //WRITE LOG
            if (vm != null)
            {
                var _accountService = new AccountService();
                var account = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (account.AccountActivityLogSettings.RecordVault)
                {
                    string title = "You changed infomation vault in Regit.";
                    string type = "Keep a record of information vault operations";
                    var act = new ActivityLogBusinessLogic();
                    act.WriteActivityLogFromAcc(account.AccountId, title, type);
                }
            }
            return Ok(vm);
        }

        [Route("InsertDocumentField")]
        [HttpPost]
        public IHttpActionResult InsertDocumentField()
        {
            var rs = false;
            var doc = new Document();
            doc.SaveName = "asave";
            doc.Category = "Custom";
            doc.FileName = "Aaa";
            doc.UploadDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            doc.ExpiredDate = "";
            doc.NoSearch = false;
            doc.Path = "aaPath";

            var currentUser = AccountService.GetByAccountId(User.Identity.GetUserId());
            var accountId = currentUser.AccountId;
            InfomationVaultBusinessLogic.InsertDocumentFieldByAccountId(accountId, doc);
            return Ok(rs);
        }

        [HttpPost, Route("UploadSaveDocument")]
        public async Task<IHttpActionResult> UploadSaveDocument(string name = null, string category = null,
            string path = null)
        {
            var rs = new DocumentVaultViewModel();
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var currentUser = AccountService.GetByAccountId(User.Identity.GetUserId());
            string userId = currentUser.AccountId.ToString();

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            // Read the form data.
            await Request.Content.ReadAsMultipartAsync(provider);
            var SaveName = "";
            var doc = new Document();
            foreach (var fileData in provider.FileData)
            {
                SaveName = fileData.Headers.ContentDisposition.FileName;
                if (SaveName.StartsWith("\"") && SaveName.EndsWith("\""))
                {
                    SaveName = SaveName.Trim('"');
                }
                if (SaveName.Contains(@"/") || SaveName.Contains(@"\"))
                {
                    SaveName = Path.GetFileName(SaveName);
                }
                string extension = Path.GetExtension(SaveName);

                var uploadFolder = "~/" + ConfigHelp.GetStringValue("folderVault") + userId;
                var directory = new DirectoryInfo(HostingEnvironment.MapPath(uploadFolder));
                if (!directory.Exists)
                {
                    directory.Create();
                }

                try
                {
                    doc.SaveName = SaveName;
                    doc.FileName = SaveName;
                    doc.Category = "Custom";
                    doc.Path = "Custom";
                    if (!string.IsNullOrEmpty(name))
                    {
                        doc.FileName = name;
                    }
                    if (!string.IsNullOrEmpty(category))
                        doc.Category = category;

                    if (!string.IsNullOrEmpty(path))
                    {
                        doc.Path = path;
                    }

                    doc.UploadDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    doc.ExpiredDate = "";
                    doc.NoSearch = false;

                    var docrs = InfomationVaultBusinessLogic.InsertDocumentFieldByAccountId(userId, doc);
                    if (docrs != null)
                    {
                        doc.SaveName = docrs.SaveName;
                        rs.Status = "Successful";
                    }
                }
                catch
                {
                }


                var StoragePath = HttpContext.Current.Server.MapPath(uploadFolder);
                if (File.Exists(Path.Combine(StoragePath, doc.SaveName)))
                    File.Delete(Path.Combine(StoragePath, doc.SaveName));
                File.Move(fileData.LocalFileName, Path.Combine(StoragePath, doc.SaveName));
                rs.SaveName = doc.SaveName;
                rs.FileName = doc.FileName;
                rs.Path = doc.Path;
            }

            return Ok(rs);
        }

        [HttpPost, Route("TestVault")]
        public async Task<IHttpActionResult> TestVault()
        {
            var info = new InfomationVaultBusinessLogic();

            string userid = HttpContext.Current.User.Identity.GetUserId();
            var rs = info.GetBasicFormVaultByUserId(userid);

            return Ok(rs);
        }

        //GetBsonBasic

        #endregion version2
    }
}