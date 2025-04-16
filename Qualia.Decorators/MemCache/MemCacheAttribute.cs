using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System;

namespace Qualia.Decorators
{
    public class MemCacheAttribute : DecorateAttribute
    {
        public TimeSpan? TimeSpan { get; set; }
        public ExpirationType Expiration { get; set; }
        public string[] VaryBy { get; set; }

        public MemCacheAttribute(
            string name = null, 
            string timespan = "1m", 
            ExpirationType expiration = ExpirationType.Absolute,
            string[] varyBy = null) 
            : base(typeof(MemCache), name) 
        {
            TimeSpan = StringToTimeSpan.Parse(timespan);
            Expiration = expiration;
            VaryBy = varyBy;
        }

        public enum ExpirationType { Absolute, Sliding }
    }
}
