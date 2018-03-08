using System;

namespace GH.Core.BlueCode.BusinessLogic
{
    public enum CacheDurationType
    {
        Second = 1,
        Minute = 60,
        Hour = 3600,
        Day = 86400
    }

    public struct CachePolicy
    {
        public int Duration { get; set; }
        public CacheDurationType DurationType { get; set; }
        public CachePolicy(int duration, CacheDurationType durationType)
        {
            this.Duration = duration;
            this.DurationType = durationType;
        }
    }
}
