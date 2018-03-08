using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TokenDeviceViewModel
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string TokenDevice { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
    }
}