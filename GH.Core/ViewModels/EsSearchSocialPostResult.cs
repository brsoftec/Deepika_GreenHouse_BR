using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class EsSearchShardsResult
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int failed { get; set; }
    }

    public class EsSearchHitsResult
    {
        public int total { get; set; }
        public double? max_score { get; set; }
        public dynamic[] hits { get; set; }
    }

    public class EsSearchHitResult
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double? _score { get; set; }
    }

    public class EsSearchResult
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public EsSearchShardsResult _shards { get; set; }
        public EsSearchHitsResult hits { get; set; }
    }
}