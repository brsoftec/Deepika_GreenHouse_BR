using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class UpdateMemberModel
    {
        public UpdateMemberAction Action { get; set; }
        public UpdateMemberInfo Member { get; set; }
    }

    public enum UpdateMemberAction
    {
        UPDATE, DELETE
    }

    public class UpdateMemberInfo
    {
        public string AccountId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsReviewer { get; set; }
        public bool IsEditor { get; set; }
    }
}