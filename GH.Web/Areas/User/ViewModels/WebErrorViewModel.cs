using System;

namespace GH.Web.Areas.User.ViewModels
{
    public class WebErrorViewModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }

//        public Exception Exception { get; set; }

    }   
    
    public class WorkflowErrorViewModel : WebErrorViewModel
    {
        public string Resource { get; set; }
        public string ResourceName { get; set; }
        public string Roles { get; set; }
        public string EffectivePermissions { get; set; }

    }
}