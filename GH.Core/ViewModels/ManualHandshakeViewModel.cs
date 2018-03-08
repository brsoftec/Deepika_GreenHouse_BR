using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class ManualHandshakeViewModel
    {
        public ManualHandshakeViewModel()
        {
            fields = new List<FieldManualHandshakeViewModel>();
            expiry = new ExpiryViewModel();

        }
        public string Id { get; set; }
        public string accountId { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public ExpiryViewModel expiry { get; set; }
        public DateTime synced { get; set; }
        public string toAccountId { get; set; }
        public string toName { get; set; }
        public string toEmail { get; set; }
        public string notifyFormat { get; set; }

        public List<FieldManualHandshakeViewModel> fields { get; set; }
    }
    public class FieldManualHandshakeViewModel
    {
        public string label { get; set; }
        public string jsPath { get; set; }
        public bool selected { get; set; }

    }

    public class ExpiryViewModel
    {
        public bool indefinite { get; set; }
        public DateTime date { get; set; }
    }

}