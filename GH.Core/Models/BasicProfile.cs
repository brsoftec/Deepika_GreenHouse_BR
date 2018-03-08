using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class BasicProfile
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}