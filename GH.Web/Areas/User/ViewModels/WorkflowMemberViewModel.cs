using System.Collections.Generic;

namespace GH.Web.Areas.User.ViewModels
{
    public class WorkflowMemberViewModel
    {
        public string memberId { get; set; }
        public string businessId { get; set; }
        public List<string> roles { get; set; }
    }

}