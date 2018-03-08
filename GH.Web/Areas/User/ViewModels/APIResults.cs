using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Web.Areas.User.ViewModels
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ErrorResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ErrorResult(string message)
        {
            Success = false;
            Message = message;
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FullErrorResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string Error { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public object Data { get; set; }

        public FullErrorResult(string message, string error = null, object payload = null)
        {
            Success = false;
            Message = message;
            Error = error;
            Data = payload;
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SuccessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public object Data { get; set; }

        public SuccessResult(object data, string message = "Successful result")
        {
            Success = true;
            Message = message;
            Data = data;
        }
    }


    public static class ReponseExtensions
    {
        public static HttpResponseMessage CreateSuccessResponse(
            this HttpRequestMessage request, object payload, string message = "Successful result")
        {
            return request.CreateResponse<SuccessResult>(
                HttpStatusCode.OK,
                new SuccessResult(payload, message));
        }

        public static HttpResponseMessage CreateApiErrorResponse(
            this HttpRequestMessage request, string message,
            HttpStatusCode status = HttpStatusCode.OK,
            string error = null, object payload = null)
        {
            return request.CreateResponse<FullErrorResult>(
                status,
                new FullErrorResult(message,error:error,payload:payload));
        }
    }
}