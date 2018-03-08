using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class CompanyViewModel
    {
     

            public string Industry { get; set; }
            public string CompanyName { get; set; }
            public string Website { get; set; }
            public string Description { get; set; }
            public string Phone { get; set; }
            public string Country { get; set; }
            public string Region { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string Email { get; set; }
            public string BusinessId { get; set; }

            public string BsonString { get; set; }
      
    }
}