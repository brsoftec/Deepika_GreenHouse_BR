using GH.Core.Models;
using GH.Core.Services;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GH.Core.Adapters
{
    public class AccountAdapter
    {
        public static AccountViewModel ConvertToViewModel(Account model)
        {
            var viewModel = new AccountViewModel();
            if (model == null)
                return viewModel;
            viewModel.Id = model.Id.ToString();
            viewModel.AccountId = model.AccountId;
            if (model.Profile != null)
            {
                viewModel.DisplayName = model.Profile.DisplayName;
                viewModel.FirstName = model.Profile.FirstName;
                viewModel.LastName = model.Profile.LastName;
                viewModel.Email = model.Profile.Email;
                viewModel.PhotoUrl = model.Profile.PhotoUrl;
                viewModel.Status = model.Profile.Status;
                viewModel.Country = model.Profile.Country;
                viewModel.City = model.Profile.City;
                viewModel.AccountType = model.AccountType.ToString();
                viewModel.Description = model.Profile.Description;
                viewModel.Region = model.Profile.Region;
                viewModel.Street = model.Profile.Street;
                viewModel.ZipPostalCode = model.Profile.ZipPostalCode;
                viewModel.Birthdate = "";
                try {
                    var dt = new DateTime();
                    if(model.Profile.Birthdate != null)
                    {
                        dt = Convert.ToDateTime(model.Profile.Birthdate);
                        viewModel.Birthdate = dt.ToShortDateString();
                    }
                }
                catch { }
                viewModel.IsShowProfile = model.AccountPrivacies.ViewMyProfile;
                viewModel.BusinessPrivacies = model.BusinessPrivacies;
                viewModel.NotificationSettings = model.NotificationSettings;
                viewModel.BusinessAccountVerified = model.BusinessAccountVerified;
                if (model.AccountType == AccountType.Business)
                {
                    viewModel.WebsiteURL = model.CompanyDetails.Website;
                    viewModel.Phone = model.Profile.PhoneNumber;
                }
            }
            if (model.AccountActivityLogSettings != null)
            {
                viewModel.ActivityLogSettings = model.AccountActivityLogSettings;
              }

            var statusAccount = new StatusAccount();
            statusAccount.Status = model.Status;
            statusAccount.Email = model.Profile.Email;

            var disabledUser = new DisabledUser();
            try
            {
                //var disabledUser = new DisabledUserService();
                var _disabledUserService = new DisabledUserService();
               disabledUser = _disabledUserService.GetDisabledUserByEmail(statusAccount.Email);
                if (disabledUser.EffectiveDate != null)
                    statusAccount.EffectiveDate = disabledUser.EffectiveDate.ToShortDateString();
                if (!string.IsNullOrEmpty(disabledUser.Reason))
                    statusAccount.Reason = disabledUser.Reason;
                if (disabledUser.Until != null)
                {
                    var dt = new DateTime();
                    dt = Convert.ToDateTime(disabledUser.Until);
                    statusAccount.Until = dt.ToShortDateString();
                }
            }

            catch { }
            viewModel.StatusAccount = statusAccount;
            var aswerSercurityQuestions = new AnswerSecurityQuestionModel();
            try
            {

                if (model.SecurityQuesion1.QuestionId !=null)
                    aswerSercurityQuestions.Question1.QuestionId = Convert.ToString(model.SecurityQuesion1.QuestionId);
                 
                if (!string.IsNullOrEmpty(model.SecurityQuesion1.Answer))
                    aswerSercurityQuestions.Question1.Answer = model.SecurityQuesion1.Answer;

                if (model.SecurityQuesion2.QuestionId != null)
                    aswerSercurityQuestions.Question2.QuestionId = Convert.ToString(model.SecurityQuesion2.QuestionId);
                if (!string.IsNullOrEmpty(model.SecurityQuesion2.Answer))
                    aswerSercurityQuestions.Question2.Answer = model.SecurityQuesion2.Answer;

                if (model.SecurityQuesion3.QuestionId != null)
                    aswerSercurityQuestions.Question3.QuestionId = Convert.ToString(model.SecurityQuesion3.QuestionId);
                if (!string.IsNullOrEmpty(model.SecurityQuesion3.Answer))
                    aswerSercurityQuestions.Question3.Answer = model.SecurityQuesion3.Answer;
            }
            catch { }
          
            var lstQuestion = new List<SecurityQuestion>();
            try
            {
                ISecurityQuestionService _questionService = new SecurityQuestionService();
                lstQuestion = _questionService.GetAll().ToList();
            }
            catch { }
            viewModel.SercurityQuestions = lstQuestion;

            for (var i = 0; i < lstQuestion.Count; i++)
            {
                if (!string.IsNullOrEmpty(aswerSercurityQuestions.Question1.QuestionId) && aswerSercurityQuestions.Question1.QuestionId == lstQuestion[i].Id.ToString())
                    aswerSercurityQuestions.Question1.Question = lstQuestion[i].Question;
                if (!string.IsNullOrEmpty(aswerSercurityQuestions.Question2.QuestionId) && aswerSercurityQuestions.Question2.QuestionId == lstQuestion[i].Id.ToString())
                    aswerSercurityQuestions.Question2.Question = lstQuestion[i].Question;
                if (!string.IsNullOrEmpty(aswerSercurityQuestions.Question3.QuestionId) && aswerSercurityQuestions.Question3.QuestionId == lstQuestion[i].Id.ToString())
                    aswerSercurityQuestions.Question3.Question = lstQuestion[i].Question;
            }

          viewModel.AnswerSercurityQuestions = aswerSercurityQuestions;

           
           

            return viewModel;
        }

        public static List<FolloweeViewModel> ConvertToFolloweeViewModel(List<Account> accounts, Account currentAccount)
        {
            var followees = new List<FolloweeViewModel>();
            foreach(var account in accounts)
            {
                try
                {
                    var temp = currentAccount.Followees.Single(t => t.AccountId == account.Id);
                    if (!string.IsNullOrEmpty(account.Profile.DisplayName))
                    {
                        followees.Add(new FolloweeViewModel
                        {
                            Id = account.Id.ToString(),
                            DisplayName = account.Profile.DisplayName,
                            PhotoUrl = account.Profile.PhotoUrl,
                            Time = temp.Time
                        });

                    }

                }
                catch { }


            }
            return followees;
        }
    }
}