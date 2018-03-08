using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class InviteFriendModel
    {
        [Required]
        public string ReceiverId { get; set; }

    }

    public class TrustEmergencyModel
    {
        [Required]
        public string ReceiverId { get; set; }
        public string NetworkName { get; set; }
        public string Relationship { get; set; }

        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

    }


    public class InviteEmergencyModel
    {
        [Required]
        public string ReceiverId { get; set; }
        public string NetworkName { get; set; }
        public string Relationship { get; set; }
      
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

    }

    public class AcceptDenyInvitationModel
    {
        [Required]
        public string InvitationId { get; set; }
        public string NetworkName { get; set; }
        public string Relationship { get; set; }
        public bool IsEmergency { get; set; }

        public int Rate { get; set; }
    }

    public class EmailInviteEmergency
    {
        [Required]
        public string ToEmail { get; set; }
        public string InviteId { get; set; }
        public string FromAccountId { get; set; }
        public string Relationship { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

    }
    public class ListEmailInviteEmergency
    {
        [Required]
        public string[] ToEmail { get; set; }
        public string InviteId { get; set; }
        public string FromAccountId { get; set; }
        public string Relationship { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }

    }

}