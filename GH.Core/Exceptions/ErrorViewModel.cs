using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GH.Core.Exceptions
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ErrorViewModel
    {
        public bool Success => false;
        
        public ErrorCode Error { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
}