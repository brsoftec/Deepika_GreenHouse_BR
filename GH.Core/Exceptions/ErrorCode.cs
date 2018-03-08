using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Exceptions
{
    public enum ErrorCode
    {
        Unknown = 0,

        #region common errors
        /// <summary>
        /// This error occur when user try to access a resource require authorization
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        /// This error occur when logged in user does not have role/permission to do action <br/>
        /// For example: User update non-owned listing ...
        /// </summary>
        PermissionDenied = 403,
        
        /// <summary>
        /// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource for the requested method <br/>
        /// For example: API only support for multipart/form-data content type, but request is in application/json content type
        /// </summary>
        UnsupportedMediaType = 415,

        /// <summary>
        /// Internal server error, please log and report system admin
        /// </summary>
        InternalServerError = 500,

        /// <summary>
        /// Bad Request
        /// </summary>
        BadRequest = 400,
        #endregion


        #region CMS errors 900xyz

        #endregion

        #region GALLERY errors 901xyz

        #endregion

        #region AUTHORIZATION errors 902xyz
        AUTH_VerifiedPhoneTokenExpired = 902001,
        AUTH_PhoneNumberInvalid = 902002,
        AUTH_EmailDuplicated = 902003,
        AUTH_PasswordInvalid = 902004,
        #endregion

        #region PIN AUTHENTICATION errors 903xyz
        PINAUTH_CHECK_ERROR = 903001,
        #endregion

        #region FIELD VALIDATING 999xyz
        PHONE_NUMBER_INVALID_FORMAT = 999001,
        PHONE_NUMBER_MOBILE_REQUIRED = 999002,
        #endregion
    }
}