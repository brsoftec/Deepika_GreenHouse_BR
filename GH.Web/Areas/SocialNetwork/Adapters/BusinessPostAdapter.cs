using GH.Core.Adapters;
using GH.Core.Models;
using GH.Web.Areas.SocialNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.SocialNetwork.Adapters
{
    public class BusinessPostAdapter
    {
        public static BusinessPostViewModel ConvertToViewModel(BusinessPost model, Account accountOfPost, List<Account> stateFlowExecutors)
        {
            var viewModel = new BusinessPostViewModel();
            viewModel.Id = model.Id.ToString();
            viewModel.Privacy = model.Privacy;
            viewModel.Message = model.Message;
            viewModel.Photos = model.Photos == null ? viewModel.Photos : model.Photos.ToList();
            viewModel.VideoUrl = model.VideoUrl;
            viewModel.CreatedOn = model.CreatedOn;
            viewModel.Creator = AccountAdapter.ConvertToViewModel(accountOfPost);
            viewModel.SocialTypes = model.SocialTypes.Select(s => s.ToString()).ToList();
            viewModel.Status = model.Status.ToString();


            var executors = stateFlowExecutors.Select(e => AccountAdapter.ConvertToViewModel(e));
            viewModel.Workflows = model.Workflows.Select(w => new BusinessPostStateFLowViewModel
            {
                Action = w.Action.ToString(),
                Comment = w.Comment,
                ExecuteTime = w.ExecuteTime,
                Executor = executors.FirstOrDefault(e => e.Id == w.Executor.ToString())
            }).ToList();

            return viewModel;
        }
    }
}