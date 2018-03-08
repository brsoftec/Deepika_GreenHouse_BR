using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Core.Services
{
    public interface IBusinessAccountStatisticsService
    {
        FollowersByTimeStatisticResult SummarizeFollowersByTime(ObjectId businessAccountId, DateTime? from, DateTime? to);
        List<FollowersByGenderResult> SummarizeFollowersByGender(ObjectId businessAccountId, List<AgeRange> ages);
        List<FollowersByCountryResult> SummarizeFollowersByCountry(ObjectId businessAccountId);
        List<FollowersByCityResult> SummarizeFollowersByCity(ObjectId businessAccountId);
    }
}
