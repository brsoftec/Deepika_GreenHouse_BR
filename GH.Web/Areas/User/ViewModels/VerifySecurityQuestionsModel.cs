using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class VerifySecurityQuestionsModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        
        public AnswerSecurityQuestionViewModel Question1 { get; set; }

        public AnswerSecurityQuestionViewModel Question2 { get; set; }

        public AnswerSecurityQuestionViewModel Question3 { get; set; }
    }
}