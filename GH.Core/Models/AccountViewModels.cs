using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace GH.Core.Models
{
    // Models returned by AccountController actions.

    public class ExternalLoginViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string State { get; set; }
    }

    public class ManageInfoViewModel
    {
        public string LocalLoginProvider { get; set; }

        public string Email { get; set; }

        public IEnumerable<UserLoginInfoViewModel> Logins { get; set; }

        public IEnumerable<ExternalLoginViewModel> ExternalLoginProviders { get; set; }
    }

    public class UserInfoExportViewModel
    {

        public string UserName { get; set; }
        public string Email { set; get; }
    }

    public class UserInfoViewModel
    {

        public string UserName { get; set; }

        public bool HasRegistered { get; set; }

        public string LoginProvider { get; set; }

        public string AccessToken { get; set; }
    }

    public class UserLoginInfoViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
    public class UserViewforAdmin
    {
        public UserViewforAdmin()
        {
            SecurityQuesion1 = new SecurityQuestionAnswer();
            SecurityQuesion2 = new SecurityQuestionAnswer();
            SecurityQuesion3 = new SecurityQuestionAnswer();
            BusinessAccountRoles = new List<UserRole>();
        }

        public bool IsBelongBusinees { get; set; }

        public string AccountId { get; set; }
       
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
       
        public string PinCodeHash { get; set; }
        public Profile Profile { get; set; }

        public AccountType AccountType { get; set; }
     
        public List<UserRole> BusinessAccountRoles { get; set; }
     
        public SecurityQuestionAnswer SecurityQuesion1 { get; set; }

      
        public SecurityQuestionAnswer SecurityQuesion2 { get; set; }

        public string CreateDateFormat { get; set; }
       
        public SecurityQuestionAnswer SecurityQuesion3 { get; set; }
    }
    public class UserRole
    {
        public string AccountId { get; set; }
        public string RoleName { get; set; }
        public string Status { get; set; }
        public ObjectId? RoleId { get; set; }
        public DateTime? SentAt { get; set; }
    }
   
}
