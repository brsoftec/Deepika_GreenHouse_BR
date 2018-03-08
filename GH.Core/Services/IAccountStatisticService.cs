using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface IAccountStatisticService
    {
        List<AccountRegistrationTimeStatResult> GetAccountRegistrationTimeStatistics(DateTime from, DateTime to, TimePeriod period);
        TodayRegistrationStatisticsResult GetTodayRegistrationStatistics();
        AccountCompaignStaticViewModel GetAccountCompaignStatic(ObjectId accountId, DateTime? from, DateTime? to);
        List<RegistrationByGenderResult> GetRegistrationDemographicsReport(DateTime? from, DateTime? to, List<AgeRange> ages);
        List<MembersByLocationResult> GetMembersByLocationReport(LocationForReport location);
        List<Account> GetBusinessAccounts();
        List<Account> GetPersonalAccounts();
    }
}