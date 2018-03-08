using System.Collections.Generic;
using GH.Core.ViewModels;

namespace GH.Core.Models
{
    
    public class FuncResult
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public FuncResult(bool success = true, string status = null, object data = null, string message = null)
        {
            Success = success;
            Status = status;
            Message = message;
            Data = data;
        }
    }

    public class OkResult : FuncResult
    {
        public OkResult(string status = "ok", object data = null, string message = null)
        : base (true, status, data, message)
        {
            
        }
    }    
    public class ErrResult : FuncResult
    {
        public ErrResult(string status = "error", object data = null, string message = null)
        : base (false, status, data, message)
        {
            
        }
    }

}