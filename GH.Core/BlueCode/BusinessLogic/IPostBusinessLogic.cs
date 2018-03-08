using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.InformationVault;

namespace GH.Core.BlueCode.BusinessLogic
{
    public interface IPostBusinessLogic
    {
        void DeletePostByCampaign(string campaignId);
        void RegisterCampaign(Account userAcount, Account businessUserAccount, string campainId, string campainType, 
            string dateadd = "", List<FieldinformationVault> listvaults = null, string delegationId=null, string delegateeId=null);
        void DeRegisterCampaign(Account userAcount, Account businessUserAccount, string campainId, string delegateeId=null);
        List<Post> GetPostList();
        List<Post> GetPostListbyUserId(string userId);
        DataList<Follower> GetFollowersList(string campaignId, int pageIndex = 1, int pageSize = 10);
        DataList<Follower> GetFollowedBusinessByUserId(string userId, int pageIndex = 1, int pageSize = int.MaxValue);
        List<DataRegisCampaign> GetDataChartByCampaignByTime(string campaignid, string datestart);
        List<DataRegisCampaign> GetDataChartByBusidByTime(string busid, string datestart);
        List<Follower> GetFollowersByCampaignId(string campaignId);
        List<FieldinformationVault> checkQA(string campaignid, string userid, List<FieldinformationVault> fields);
        List<FieldinformationVault> CheckGroupField(List<FieldinformationVault> fields, string campaignId);
        Post GetPost( string postType, string campaignId);
        string AddPost(string postType, string campaignId, string FromUserId, Follower follower = null);
        Follower GetFollowerPost(string userId, string campaignId);
    }
}