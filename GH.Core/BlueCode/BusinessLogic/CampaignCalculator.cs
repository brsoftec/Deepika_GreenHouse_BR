using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Profile;
using GH.Core.Models;
using GH.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace GH.Core.BlueCode.BusinessLogic
{
    public sealed class CampaignCalculator : ICampaignCaculator
    {
        IProfileRepository profileRepository;
        public CampaignCalculator(IProfileRepository profileRepository)
        {
            this.profileRepository = profileRepository;
        }

        public CampaignCalculator()
        {
        }

        public CampaignUserFilterResult CalculateNumberOfUser(CampaignFilter campaignFilter)
        {
            InfomationVaultBusinessLogic infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
            var accountService = new AccountService();
            IEnumerable<string> memberIds = null;
            if(!string.IsNullOrEmpty(campaignFilter.TargetNetwork) && campaignFilter.TargetNetwork.Equals(EnumTargetNetwork.CustomerNetwork))
            {
                var businessMemberBus = new BusinessMemberLogic();
                var businessMembers = businessMemberBus.GetMembersOfBusiness(campaignFilter.BusinessUserId, EnumBusinessMemberStatus.Following);
                if(businessMembers!=null)
                {
                    memberIds = businessMembers.Select(m => m.UserId);
                }
            }
            List<string> memberinVaults = new List<string>();
            bool iscomparevaultkeyworks = true;
            if (campaignFilter.ListKeywork != null && campaignFilter.ListKeywork.Count > 0)
            {
                iscomparevaultkeyworks = false;
                memberinVaults = infomationVaultBusinessLogic.Getaccountidsfromkeyworkinvault(campaignFilter.ListKeywork);
            }
           

            if (memberIds == null) memberIds = new List<string>();

            var allRegitUsers = campaignFilter.TargetNetwork.Equals(EnumTargetNetwork.RegitNetwork);
            var allCountries = string.IsNullOrEmpty(campaignFilter.Country) || campaignFilter.Country.ToLower().Equals("all");
            var allCities = string.IsNullOrEmpty(campaignFilter.City) || campaignFilter.City.ToLower().Equals("all");
            var allAges = campaignFilter.MinAge == 0 && campaignFilter.MaxAge == 0;
            var allGenders = string.IsNullOrEmpty(campaignFilter.Gender) || campaignFilter.Gender.ToLower().Equals("all");
            var minAge = DateTime.Now.AddYears(-campaignFilter.MinAge);
            var maxAge = DateTime.Now.AddYears(-campaignFilter.MaxAge);

            Expression<Func<Account, bool>> express = a =>  (allRegitUsers || memberIds.Contains(a.AccountId)) && (iscomparevaultkeyworks || memberinVaults.Contains(a.AccountId)) &&
                                                            (allCountries || (!string.IsNullOrEmpty(a.Profile.Country) && a.Profile.Country.Equals(campaignFilter.Country))) &&
                                                            (allCities || (!string.IsNullOrEmpty(a.Profile.City) && a.Profile.City.Equals(campaignFilter.City))) &&
                                                            (allAges || (a.Profile.Birthdate.HasValue && (a.Profile.Birthdate <= minAge) && (a.Profile.Birthdate >= maxAge))) &&
                                                            (allGenders || (!string.IsNullOrEmpty(a.Profile.Gender) && a.Profile.Gender.Equals(campaignFilter.Gender)));


            //find people who born before x years ago
            //minAge = DateTime.Now.AddYears(-1);
            //express = a => a.Profile.Birthdate.HasValue && a.Profile.Birthdate <= minAge;
            //var test = accountService.Search(express).ToList();

            var totalUsers = accountService.Search(express). Count();
            var userBaseOnSpend = GetUserBaseOnSpend(campaignFilter.Money);
            if (userBaseOnSpend > totalUsers)
            {
                userBaseOnSpend = totalUsers;
            }

            //if (campaignFilter.Flash)
            //{
            //    userBaseOnSpend = userBaseOnSpend - 10 > 0 ? userBaseOnSpend - 10 : 0;
            //}

            return new CampaignUserFilterResult
            {
                TotalUsers = totalUsers,
                UsersBaseOnSpend = userBaseOnSpend
            };
        }

        public long GetUserBaseOnSpend(decimal money)
        {
            Decimal moneyUserDay = 0.1M;
            try
            {
                Decimal.TryParse(ConfigurationManager.AppSettings["MONEY_USER_DAY"], out moneyUserDay);
            }
            catch (Exception)
            {
            }

            //int users = (int)(money / moneyUserDay) * days;

            int users =  (int) Math.Floor(money / moneyUserDay);

            return users;
        }
    }
}
