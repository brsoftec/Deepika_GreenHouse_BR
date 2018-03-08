using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class QuestionAnswerViewModel
    {
       public string email { get; set; }
        public string QuestionId1 { get; set; }
        public string Question1 { get; set; }
        public string Answer1 { get; set; }
     
        public string QuestionId2 { get; set; }
        public string Question2 { get; set; }
        public string Answer2 { get; set; }
        public string QuestionId3 { get; set; }
        public string Question3 { get; set; }
        public string Answer3 { get; set; }
        public string AccountType { get; set; }
        public string AccountStatus { get; set; }


    }
}