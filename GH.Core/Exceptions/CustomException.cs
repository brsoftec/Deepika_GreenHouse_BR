using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace GH.Core.Exceptions
{
    /// <summary>
    /// Custom validate
    /// </summary>
    public class CustomException : Exception
    {
        public ErrorViewModel[] Errors { get; private set; }
        private Logger log = LogManager.GetCurrentClassLogger();

        public CustomException(Exception ex)
        {
            log.Error(ex);
        }

        public CustomException(params ErrorViewModel[] errors)
        {
            Errors = errors;
        }

        public CustomException(string errorMessage)
        {
            Errors = new ErrorViewModel[] { new ErrorViewModel { Error = ErrorCode.Unknown, Message = errorMessage } };
        }

        public CustomException(ErrorCode error, string message = null)
        {
            Errors = new ErrorViewModel[] { new ErrorViewModel { Error = error, Message = message } };
        }

        public CustomException(ModelStateDictionary modelState)
        {
            Errors = modelState.Values.SelectMany(v => v.Errors.Select(e => new ErrorViewModel { Message = e.ErrorMessage })).ToArray();
        }
    }

    public class ResponseHelper
    {
        /// <summary>
        /// Return error
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //public HttpResponseException BadRequest(params string[] errors)
        //{
        //    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    var content = errors.Select(t => t);
        //    message.Content = new ObjectContent<IEnumerable<string>>(content, new JsonMediaTypeFormatter());
        //    throw new HttpResponseException(message);
        //}
        public HttpResponseException BadRequest(string message)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var obj = new { Message = message };
            response.Content = new ObjectContent(obj.GetType(), obj, new JsonMediaTypeFormatter());
            throw new HttpResponseException(response);
        }
    }

}