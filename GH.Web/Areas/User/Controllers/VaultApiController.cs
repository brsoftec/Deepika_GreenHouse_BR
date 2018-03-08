using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using GH.Core.BlueCode.Entity.Message;
using Microsoft.AspNet.Identity;
using GH.Web.Areas.User.ViewModels;
using MongoDB.Bson;
using GH.Core.SignalR.Events;
using GH.Core.BlueCode.Entity.InformationVault;
using Newtonsoft.Json;
using GH.Core.BlueCode.BusinessLogic;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using GH.Core.BlueCode.DataAccess;
using GH.Core.Models;
using GH.Core.ViewModels;
using JavaPort;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [ApiAuthorize]
    [RoutePrefix("Api/Vault")]
    public class VaultApiController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet, Route("Fields")]
        public async Task<HttpResponseMessage> ListVaultFieldsAsync()
        {
            var fields = await VaultService.ListFieldsAsync();
            return Request.CreateSuccessResponse(fields, $"List {fields.Count} vault fields");
        }

        [AllowAnonymous]
        [HttpGet, Route("Field")]
        public async Task<HttpResponseMessage> GetVaultFieldAsync(string path)
        {
            var field = await VaultService.GetFieldAsync(path);
            if (field == null)
                return Request.CreateApiErrorResponse("Vault field not found", error: "vault.field.notFound");
            return Request.CreateSuccessResponse(field, "Vault field found");
        }
    }
}