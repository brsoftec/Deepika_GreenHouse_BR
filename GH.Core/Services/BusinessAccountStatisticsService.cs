using GH.Core.Exceptions;
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
    public class BusinessAccountStatisticsService : IBusinessAccountStatisticsService
    {
        IMongoCollection<Account> _accountCollection;

        public BusinessAccountStatisticsService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
        }

        public FollowersByTimeStatisticResult SummarizeFollowersByTime(ObjectId businessAccountId, DateTime? from, DateTime? to)
        {
            var businessAccount = _accountCollection.Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            }

            IList<IGrouping<DateTime, Follow>> aggregated = null;
            if (from.HasValue && to.HasValue)
            {
                from = from.Value.Date;
                to = to.Value.Date.AddDays(1);
                aggregated = businessAccount.Followers.Where(f => f.Time >= from.Value && f.Time < to.Value).GroupBy(f => f.Time.Date).OrderBy(f => f.Key).ToList();
            }
            else if (to.HasValue)
            {
                to = to.Value.Date.AddDays(1);
                aggregated = businessAccount.Followers.Where(f => f.Time < to.Value).GroupBy(f => f.Time.Date).OrderBy(f => f.Key).ToList();
                var fromDate = aggregated.FirstOrDefault();
                if (fromDate != null)
                {
                    from = fromDate.Key.Date;
                }
                else
                {
                    from = to.Value.AddDays(-1);
                }
            }
            else if (from.HasValue)
            {
                from = from.Value.Date;
                aggregated = businessAccount.Followers.Where(f => f.Time >= from.Value).GroupBy(f => f.Time.Date).OrderBy(f => f.Key).ToList();
                var toDate = aggregated.LastOrDefault();
                if (toDate != null)
                {
                    to = toDate.Key.Date.AddDays(1);
                }
                else
                {
                    to = from.Value.AddDays(1);
                }
            }
            else
            {
                aggregated = businessAccount.Followers.GroupBy(f => f.Time.Date).OrderBy(f => f.Key).ToList();
                from = businessAccount.CreatedDate.Date;
                to = DateTime.Now.Date.AddDays(1);

            }

            var result = new FollowersByTimeStatisticResult();
            var today = aggregated.Where(a => a.Key == DateTime.Now.Date).FirstOrDefault();
            if (today == null)
            {
                result.FollowersToday = 0;
            }
            else
            {
                result.FollowersToday = today.Count();
            }

            result.From = from.Value;
            result.To = to.Value;

            result.TotalFollowers = businessAccount.Followers.Count;
            result.FollowersByDate = new List<FollowersByDateResult>();


            for (DateTime d = from.Value; d < to.Value; d = d.AddDays(1))
            {
                var byDate = aggregated.FirstOrDefault(f => f.Key == d);
                var followByDate = new FollowersByDateResult
                {
                    Followers = 0,
                    From = d,
                    To = d.AddDays(1)
                };
                if (byDate != null)
                {
                    followByDate.Followers = byDate.Count();
                }
                result.FollowersByDate.Add(followByDate);
            }

            return result;
        }

        public List<FollowersByGenderResult> SummarizeFollowersByGender(ObjectId businessAccountId, List<AgeRange> ages)
        {
            var businessAccount = _accountCollection.Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            };

            var allFollowerIds = businessAccount.Followers.Select(f => f.AccountId);
            var grouped = _accountCollection.Aggregate().Match(g => allFollowerIds.Contains(g.Id)).Group(g => g.Profile.Gender, g => new { Gender = g.Key, Followers = g.Select(f => f.Profile.Birthdate) }).ToList();

            var result = new List<FollowersByGenderResult>();
            var genders = new string[] { "Male", "Female", null };
            var now = DateTime.Now.Date;
            var calculatedAgeDate = ages.Select(a => new { FromDate = a.ToAge.HasValue ? (DateTime?)now.AddYears(-a.ToAge.Value - 1).AddDays(1) : null, ToDate = a.FromAge.HasValue ? (DateTime?)now.AddYears(-a.FromAge.Value).AddDays(1) : null, AgeRange = a }).ToList();

            foreach (var gender in genders)
            {
                var genderStat = new FollowersByGenderResult();

                genderStat.Gender = gender;

                var stats = grouped.FirstOrDefault(g => g.Gender == gender);

                if (stats == null)
                {
                    stats = new { Gender = gender, Followers = new List<DateTime?>().AsEnumerable() };
                }

                genderStat.Followers = stats.Followers.Count();

                genderStat.FollowersByAge = new List<FollowersByGenderAge>();
                foreach (var age in calculatedAgeDate)
                {
                    var ageStat = new FollowersByGenderAge();
                    ageStat.FromAge = age.AgeRange.FromAge;
                    ageStat.ToAge = age.AgeRange.ToAge;

                    if (age.AgeRange.FromAge.HasValue && age.AgeRange.ToAge.HasValue)
                    {
                        ageStat.Followers = stats.Followers.Count(f => f.HasValue && f.Value >= age.FromDate.Value && f.Value < age.ToDate.Value);
                    }
                    else if (age.AgeRange.FromAge.HasValue)
                    {
                        ageStat.Followers = stats.Followers.Count(f => f.HasValue && f.Value < age.ToDate.Value);
                    }
                    else if (age.AgeRange.ToAge.HasValue)
                    {
                        ageStat.Followers = stats.Followers.Count(f => f.HasValue && f.Value >= age.FromDate.Value);
                    }
                    else
                    {
                        ageStat.Followers = stats.Followers.Count(f => !f.HasValue);
                    }

                    genderStat.FollowersByAge.Add(ageStat);
                }

                result.Add(genderStat);
            }

            return result;
        }

        public List<FollowersByCountryResult> SummarizeFollowersByCountry(ObjectId businessAccountId)
        {
            var businessAccount = _accountCollection.Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            };

            var allFollowerIds = businessAccount.Followers.Select(f => f.AccountId);

            return _accountCollection.Aggregate().Match(g => allFollowerIds.Contains(g.Id)).Group(g => g.Profile.Country, g => new FollowersByCountryResult { Country = g.Key, Followers = g.Count() }).ToList();
        }

        public List<FollowersByCityResult> SummarizeFollowersByCity(ObjectId businessAccountId)
        {
            var businessAccount = _accountCollection.Find(a => a.Id == businessAccountId && a.AccountType == AccountType.Business).FirstOrDefault();
            if (businessAccount == null)
            {
                throw new CustomException("Business account does not exist");
            };

            var allFollowerIds = businessAccount.Followers.Select(f => f.AccountId);

            return _accountCollection.Aggregate().Match(g => allFollowerIds.Contains(g.Id))
                .Group(g => new { Country = g.Profile.Country, City = g.Profile.City }, g => new { Location = g.Key, Followers = g.Count() }).ToList().Select(n => new FollowersByCityResult
                {
                    City = n.Location.City,
                    Country = n.Location.Country,
                    Followers = n.Followers
                }).ToList();
        }
    }
}