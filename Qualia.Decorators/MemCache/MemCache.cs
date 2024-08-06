using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System.Diagnostics;

namespace Qualia.Decorators
{
    public class MemCache : DecoratorBehavior<MemCacheAttribute>
    {
        private ILogger<MemCache> _logger;
        private readonly IMemoryCache _cache;

        public MemCache(ILogger<MemCache> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public override object? Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            var key = KeyGenerator.CreateKey(context.TargetMethod, context.Args);
            var result = _cache.GetOrCreate(key, entry => 
            {
                ConfigureExpiration(ref entry);

                return context.Next(); 
            });

            return result;
        }

        private void ConfigureExpiration(ref ICacheEntry entry)
        {
            _ = AssociatedAttribute?.Expiration switch
            {
                MemCacheAttribute.ExpirationType.Absolute => entry.AbsoluteExpirationRelativeToNow = AssociatedAttribute?.TimeSpan,
                MemCacheAttribute.ExpirationType.Sliding => entry.SlidingExpiration = AssociatedAttribute?.TimeSpan,
                _ => throw new UnreachableException(),
            };
        }
    }
}
