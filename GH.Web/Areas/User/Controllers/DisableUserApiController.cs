using GH.Core.Exceptions;
using GH.Core.IServices;
using GH.Core.Models;
using GH.Core.Services;
using GH.Core.ViewModels;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("api/DisableUser")]
    public class DisableUserApiController : ApiController
    {
        private IDisabledUserService _disabledUserService { get; set; }
        private IAccountService _accountService { get; set; }

        public DisableUserApiController()
        {
            _disabledUserService = new DisabledUserService();
            _accountService = new AccountService();
        }
      

        private ErrorViewModel[] GetErrorViewModels(ModelStateDictionary modelState)
        {
            return
                modelState.Values.SelectMany(v => v.Errors.Select(e => new ErrorViewModel { Message = e.ErrorMessage }))
                    .ToArray();
        }

        [HttpPost, Route("Disabled")]
        public IHttpActionResult Disabled(DisableUserRequest disableUser)
        {
            try
            {
                var acc = _accountService.GetByAccountId(User.Identity.GetUserId());
              
               
                if (string.IsNullOrEmpty(disableUser.UserId))
                    disableUser.UserId = acc.Id.ToString();
                if (disableUser.EffectiveDate == null || disableUser.EffectiveDate < DateTime.Now.AddDays(-2))
                    disableUser.EffectiveDate = DateTime.Now;
                //if (!ModelState.IsValid)
                //{
                //    throw new CustomException(GetErrorViewModels(ModelState));
                //}

                if (_disabledUserService.IsExisted(ObjectId.Parse(disableUser.UserId)))
                {
                    return BadRequest("User was disabled.");
                }
             
                try
                {
                    _accountService.CloseAccount(acc);
                }
                catch { }
                var user = _disabledUserService.DisabledUser(new DisabledUser
                {
                    UserId = ObjectId.Parse(disableUser.UserId),
                    User = _accountService.GetById(ObjectId.Parse(disableUser.UserId)),
                    EffectiveDate = disableUser.EffectiveDate.AddMonths(1),
                    Until = disableUser.Until,
                    Reason = disableUser.Reason,
                    IsEnabled = false,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now
                });

                return Ok(user);

            }
            catch
            {
                return BadRequest("User was unidentified.");
            }
          
        }

        [HttpPost, Route("Enabled")]
        public async Task<IHttpActionResult> Enabled(EnableUsersRequest enableUsers)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(GetErrorViewModels(ModelState));
            }

            var result = await _disabledUserService.EnableUsers(new EnableParameters
            {
                DisableUsers = enableUsers.DisableUsers
            });

            return Ok(result);
        }

        [HttpPost, Route("Enable")]
        public IHttpActionResult EnableUser([FromUri] string userId)
        {
            ObjectId userObjectId;
            var isValidOjectId = ObjectId.TryParse(userId, out userObjectId);
            var acc = _accountService.GetById(userObjectId);
            if (!isValidOjectId)
            {
                return BadRequest("User ID was invalid.");
            }

            var isExisted = _disabledUserService.IsExisted(userObjectId);
            if (!isExisted)
            {
                return BadRequest("User ID was enabled.");
            }
            try
            {
                _accountService.UnCloseAccount(acc);
            }
            catch { }
            var result = _disabledUserService.EnableUsers(userObjectId);
            return Ok(result);
        }

        [HttpGet, Route("IsDisabled")]
        public StatusAccountModelView IsDisabledUser()
        {
            var rs = new StatusAccountModelView();
            rs.Status = "";
            rs.Email = "";
            var disabledUser = new DisabledUser();
            try
            {
                var acc = _accountService.GetByAccountId(User.Identity.GetUserId());
                if (!string.IsNullOrEmpty(acc.Status))
                    rs.Status = acc.Status;
                rs.Email = acc.Profile.Email;
                disabledUser = _disabledUserService.GetDisabledUserByEmail(rs.Email);
                if (disabledUser.EffectiveDate != null)
                    rs.EffectiveDate = disabledUser.EffectiveDate;
                if (!string.IsNullOrEmpty(disabledUser.Reason))
                    rs.Reason = disabledUser.Reason;
                if (disabledUser.Until != null)
                    rs.Until = disabledUser.Until;

            }
            catch { }
           
          


            return rs;
        }
        [HttpPost, Route("EnableUser")]
        public IHttpActionResult EnableUserByUser()
        {
            var acc = _accountService.GetByAccountId(User.Identity.GetUserId());
            var userId = acc.Id;
            ObjectId userObjectId = acc.Id;

            var isExisted = _disabledUserService.IsExisted(userObjectId);
            if (!isExisted)
            {
                return BadRequest("User ID was enabled.");
            }
            try
            {
                _accountService.UnCloseAccount(acc);
            }
            catch { }
            var result = _disabledUserService.EnableUsers(userObjectId);
            return Ok(result);
        }
        [HttpGet, Route("GetDisabledUsers")]
        public IHttpActionResult GetDisabledUsers([FromUri]PagingRequest request)
        {
            var result = _disabledUserService.GetAllDisableUser(new DisableUserParamter
            {
                Start = request.Start,
                Length = request.Length
            });

            return Ok(result);
        }

        [HttpGet, Route("CheckExistedDisableUser")]
        public IHttpActionResult CheckExistedDisableUser(string userId)
        {
            ObjectId userObjectId;
            var isValidOjectId = ObjectId.TryParse(userId, out userObjectId);
            if (!isValidOjectId)
            {
                return BadRequest("User ID was invalid.");
            }
            var result = _disabledUserService.IsExisted(userObjectId);
            return Ok(result);
        }

        [HttpGet, Route("FindUsersByKeyword")]
        public IHttpActionResult FindUsersByKeyword(string keyword)
        {
            var users = _accountService.FindUsersByKeyword(keyword);

            var result = new FindUserResponse()
            {
                Users = users.Select(u => new FindUserViewModel
                {
                    Id = u.Id,
                    DisplayName = u.Profile.DisplayName,
                    Avatar =
                        string.IsNullOrEmpty(u.Profile.PhotoUrl)
                            ? string.Empty
                            : (!u.Profile.PhotoUrl.Contains(@"/Content/")
                                ? u.Profile.PhotoUrl
                                : ConfigurationManager.AppSettings["RegitUrl"] + u.Profile.PhotoUrl)

                }).ToList()
            };
            return Ok(result);
        }

        
       [HttpGet, Route("GetDisabledUserByEmail")]
       public DisabledUser GetDisabledUserByEmail(string email)
        {
            var rs = new DisabledUser();
            try
            {
                rs = _disabledUserService.GetDisabledUserByEmail(email);

            }
            catch
            { }
          
            return rs;
        }
    }
}