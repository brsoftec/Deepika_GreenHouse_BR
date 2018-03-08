using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class FollowersByTimeStatisticResult
    {
        public int FollowersToday { get; set; }
        public int TotalFollowers { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<FollowersByDateResult> FollowersByDate { get; set; }
        
    }

    public class FollowersByDateResult
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Followers { get; set; }
    }

    public class FollowersByGenderResult
    {
        public string Gender { get; set; }
        public int Followers { get; set; }

        public List<FollowersByGenderAge> FollowersByAge { get; set; }
    }

    public class FollowersByGenderAge
    {
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public int Followers { get; set; }
    }
    
    public class FollowersByCountryResult
    {
        public string Country { get; set; }
        public int Followers { get; set; }
    }

    public class FollowersByCityResult
    {
        public string Country { get; set; }
        public string City { get; set; }
        public int Followers { get; set; }
    }
}