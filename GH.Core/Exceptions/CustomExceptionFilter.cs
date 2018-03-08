using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Filters;

namespace GH.Core.Exceptions
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        private bool showException = false;
        public CustomExceptionFilter(bool showEx)
        {
            showException = showEx;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            var exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(CustomException))
            {
                var customException = (CustomException)context.Exception;
                ErrorViewModel[] errors = null;
                if (showException)
                {
                    errors = customException.Errors;
                }
                else
                {
                    errors = customException.Errors.Select(e => new ErrorViewModel { Error = e.Error, Message = e.Message }).ToArray();
                }

                var attachExceptions = customException.Errors.Where(e => e.Exception != null).Select(e => e.Exception);
                if (attachExceptions.Any())
                {
                    foreach (var exception in attachExceptions)
                    {
                    //    Logger logger = LogManager.GetCurrentClassLogger();
                    //    logger.Error(exception, "Handling exception occur when request " + context.Request.RequestUri.OriginalString);
                    }
                }

                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new ObjectContent<IEnumerable<ErrorViewModel>>(errors, new JsonMediaTypeFormatter());
                context.Response = response;
            }
            else if (exceptionType == typeof(HttpException))
            {
                //Logger logger = LogManager.GetCurrentClassLogger();
                //logger.Error(context.Exception, "Http exception occur when request " + context.Request.RequestUri.OriginalString);

                var response = new HttpResponseMessage((HttpStatusCode)((HttpException)context.Exception).GetHttpCode());
                if (showException)
                {
                    response.Content = new ObjectContent(exceptionType, context.Exception, new JsonMediaTypeFormatter());
                }
                context.Response = response;
            }
            else
            {
                if (exceptionType != typeof(OperationCanceledException))
                {
                    //Logger logger = LogManager.GetCurrentClassLogger();
                    //logger.Error(context.Exception, "Unhandling exception occur when request " + context.Request.RequestUri.OriginalString);
                }

                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                if (showException)
                {
                    response.Content = new ObjectContent(exceptionType, context.Exception, new JsonMediaTypeFormatter());
                }
                context.Response = response;
            }
        }
    }
}