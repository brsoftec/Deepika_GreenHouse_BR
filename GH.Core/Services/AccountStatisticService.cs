using GH.Core.Extensions;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public class AccountStatisticService : IAccountStatisticService
    {
        private IMongoCollection<Account> _accountCollection;
        private IMongoCollection<SocialPost> _socialCollection;

        public AccountStatisticService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _socialCollection = db.SocialPosts;
        }

        public List<AccountRegistrationTimeStatResult> GetAccountRegistrationTimeStatistics(DateTime from, DateTime to, TimePeriod period)
        {
            from = from.Date;
            to = to.Date.AddDays(1);

            var times = _accountCollection.Find(a => a.CreatedDate >= from && a.CreatedDate < to).Project(Builders<Account>.Projection.Expression(a => new { Date = a.CreatedDate, Type = a.AccountType })).ToList();

            List<AccountRegistrationTimeStatResult> result = new List<AccountRegistrationTimeStatResult>();

            switch (period)
            {
                case TimePeriod.Day:
                    var dayGroups = times.GroupBy(t => t.Date.Date);
                    for (DateTime d = from; d < to; d = d.AddDays(1))
                    {
                        var date = dayGroups.FirstOrDefault(k => k.Key == d);
                        int count = 0;
                        int countBusiness = 0;
                        int countPersonal = 0;
                        if (date != null)
                        {
                            count = date.Count();
                            countBusiness = date.Count(t => t.Type == AccountType.Business);
                            countPersonal = date.Count(t => t.Type == AccountType.Personal);
                        }
                        result.Add(new AccountRegistrationTimeStatResult
                        {
                            From = d,
                            To = d.AddDays(1),
                            Period = TimePeriod.Day,
                            Count = count,
                            CountBusiness = countBusiness,
                            CountPersonal = countPersonal
                        });
                    }
                    break;
                case TimePeriod.Week:
                    var startWeek = from.AddDays(((int)DayOfWeek.Monday - (int)from.DayOfWeek - 7) % 7);
                    for (DateTime d = startWeek; d < to; d = d.AddDays(7))
                    {
                        var count = times.Count(t => t.Date >= d && t.Date < d.AddDays(7));
                        var countBusiness = times.Count(t => t.Date >= d && t.Date < d.AddDays(7) && t.Type == AccountType.Business);
                        var countPersonal = times.Count(t => t.Date >= d && t.Date < d.AddDays(7) && t.Type == AccountType.Personal);
                        result.Add(new AccountRegistrationTimeStatResult
                        {
                            From = d,
                            To = d.AddDays(7),
                            Period = TimePeriod.Week,
                            Count = count,
                            CountBusiness = countBusiness,
                            CountPersonal = countPersonal
                        });
                    }

                    var firstWeek = result.First();
                    var lastWeek = result.Last();
                    firstWeek.From = from;
                    lastWeek.To = to;
                    break;
                case TimePeriod.Month:
                    var monthGroups = times.GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1));
                    for (DateTime d = from; d < to; d = d.AddMonths(1))
                    {
                        d = new DateTime(d.Year, d.Month, 1);
                        var month = monthGroups.FirstOrDefault(k => k.Key == d);
                        int count = 0;
                        int countBusiness = 0;
                        int countPersonal = 0;
                        if (month != null)
                        {
                            count = month.Count();
                            countBusiness = month.Count(t => t.Type == AccountType.Business);
                            countPersonal = month.Count(t => t.Type == AccountType.Personal);
                        }
                        result.Add(new AccountRegistrationTimeStatResult
                        {
                            From = d,
                            To = d.AddMonths(1),
                            Period = TimePeriod.Month,
                            Count = count,
                            CountBusiness = countBusiness,
                            CountPersonal = countPersonal
                        });
                    }

                    var firstMonth = result.First();
                    var lastMonth = result.Last();
                    firstMonth.From = from;
                    lastMonth.To = to;
                    break;
                case TimePeriod.Year:
                    var yearGroups = times.GroupBy(t => t.Date.Year);
                    for (DateTime d = from; d < to; d = d.AddYears(1))
                    {
                        var year = yearGroups.FirstOrDefault(k => k.Key == d.Year);
                        int count = 0;
                        int countBusiness = 0;
                        int countPersonal = 0;
                        if (year != null)
                        {
                            count = year.Count();
                            countBusiness = year.Count(t => t.Type == AccountType.Business);
                            countPersonal = year.Count(t => t.Type == AccountType.Personal);
                        }
                        result.Add(new AccountRegistrationTimeStatResult
                        {
                            From = d,
                            To = d.AddYears(1),
                            Period = TimePeriod.Year,
                            Count = count,
                            CountBusiness = countBusiness,
                            CountPersonal = countPersonal
                        });
                    }
                    var lastYear = result.Last();
                    lastYear.To = to;
                    break;
                default:
                    break;
            }
            return result;
        }

        public TodayRegistrationStatisticsResult GetTodayRegistrationStatistics()
        {
            var todayRegistration = _accountCollection.Find(a => a.CreatedDate >= DateTime.Now.Date).Project(a => a.AccountType).ToList();
            var yesterdayRegistration = _accountCollection.Find(a => a.CreatedDate >= DateTime.Now.Date.AddDays(-1) && a.CreatedDate < DateTime.Now.Date).Project(a => a.AccountType).ToList();
            return new TodayRegistrationStatisticsResult
            {
                Count = todayRegistration.Count,
                DifferenceWithYesterdayOfBusiness = todayRegistration.Count(t => t == AccountType.Business) - yesterdayRegistration.Count(t => t == AccountType.Business),
                DifferenceWithYesterdayOfPersonal = todayRegistration.Count(t => t == AccountType.Personal) - yesterdayRegistration.Count(t => t == AccountType.Personal)
            };
        }

        public AccountCompaignStaticViewModel GetAccountCompaignStatic(ObjectId accountId, DateTime? from, DateTime? to)
        {
            AccountCompaignStaticViewModel result = new AccountCompaignStaticViewModel()
            {
                FromDate = from,
                Todate = to
            };

            //var query = Builders<SocialPost>.Filter.
            var posts = _socialCollection.Find(s => s.AccountId == accountId
                                              && (s.CreatedOn >= from || from == null)
                                              && (s.CreatedOn <= to || to == null)).ToList();
            result.TotalPosts = posts.Count;
            result.TotalComments = posts.SelectMany(p => p.Comments).Count();
            result.TotalLikes = posts.Select(s => s.Like).Sum(s => s.TotalLikes);
            result.TotalShares = posts.Sum(s => s.Shares);
            return result;
        }

        public List<RegistrationByGenderResult> GetRegistrationDemographicsReport(DateTime? from, DateTime? to, List<AgeRange> ages)
        {
            if (!from.HasValue)
            {
                from = DateTime.MinValue;
            }

            if (!to.HasValue)
            {
                to = DateTime.MaxValue;
            }

            var grouped = _accountCollection.Aggregate().Match(g => g.CreatedDate >= from.Value && g.CreatedDate < to.Value).Group(g => g.Profile.Gender, g => new { Gender = g.Key, Members = g.Select(f => f.Profile.Birthdate) }).ToList();

            var result = new List<RegistrationByGenderResult>();
            var genders = new string[] { "Male", "Female", null };
            var now = DateTime.Now.Date;
            var calculatedAgeDate = ages.Select(a => new { FromDate = a.ToAge.HasValue ? (DateTime?)now.AddYears(-a.ToAge.Value - 1).AddDays(1) : null, ToDate = a.FromAge.HasValue ? (DateTime?)now.AddYears(-a.FromAge.Value).AddDays(1) : null, AgeRange = a }).ToList();

            foreach (var gender in genders)
            {
                var genderStat = new RegistrationByGenderResult();

                genderStat.Gender = gender;

                var stats = grouped.FirstOrDefault(g => g.Gender == gender);

                if (stats == null)
                {
                    stats = new { Gender = gender, Members = new List<DateTime?>().AsEnumerable() };
                }

                genderStat.Count = stats.Members.Count();

                genderStat.CountByAges = new List<RegistrationByGenderAge>();
                foreach (var age in calculatedAgeDate)
                {
                    var ageStat = new RegistrationByGenderAge();
                    ageStat.FromAge = age.AgeRange.FromAge;
                    ageStat.ToAge = age.AgeRange.ToAge;

                    if (age.AgeRange.FromAge.HasValue && age.AgeRange.ToAge.HasValue)
                    {
                        ageStat.Count = stats.Members.Count(f => f.HasValue && f.Value >= age.FromDate.Value && f.Value < age.ToDate.Value);
                    }
                    else if (age.AgeRange.FromAge.HasValue)
                    {
                        ageStat.Count = stats.Members.Count(f => f.HasValue && f.Value < age.ToDate.Value);
                    }
                    else if (age.AgeRange.ToAge.HasValue)
                    {
                        ageStat.Count = stats.Members.Count(f => f.HasValue && f.Value >= age.FromDate.Value);
                    }
                    else
                    {
                        ageStat.Count = stats.Members.Count(f => !f.HasValue);
                    }

                    genderStat.CountByAges.Add(ageStat);
                }

                result.Add(genderStat);
            }

            return result;
        }

        public List<MembersByLocationResult> GetMembersByLocationReport(LocationForReport location)
        {
            if (location == LocationForReport.Country)
            {
                return _accountCollection.Aggregate().Group(g => g.Profile.Country, g => new MembersByLocationResult { Country = g.Key, Count = g.Count() }).ToList();
            }
            else
            {
                return _accountCollection.Aggregate()
                .Group(g => new { Country = g.Profile.Country, City = g.Profile.City }, g => new { Location = g.Key, Followers = g.Count() }).ToList().Select(n => new MembersByLocationResult
                {
                    City = n.Location.City,
                    Country = n.Location.Country,
                    Count = n.Followers
                }).ToList();
            }
        }

        public List<Account> GetBusinessAccounts()
        {
            return _accountCollection.Find(s => s.AccountType == AccountType.Business).ToList();
        }

        public List<Account> GetPersonalAccounts()
        {
            return _accountCollection.Find(s => s.AccountType == AccountType.Personal).ToList();
        }
    }
}