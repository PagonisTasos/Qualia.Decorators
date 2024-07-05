using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{
    public class MemCacheAttribute : DecorateAttribute
    {
        public TimeSpan? TimeSpan { get; set; }
        public ExpirationType Expiration { get; set; }

        public MemCacheAttribute(string? name = null, string timespan = "1m", ExpirationType expiration = ExpirationType.Absolute) 
            : base(typeof(MemCache), name) 
        {
            TimeSpan = StringToTimeSpan.Parse(timespan);
            Expiration = expiration;
        }

        public enum ExpirationType { Absolute, Sliding }
    }
}
