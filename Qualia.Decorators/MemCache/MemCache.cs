using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System.Diagnostics;

namespace Qualia.Decorators
{
    public class MemCache : DecoratorBehavior
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
                ConfigureExpiration(ref entry, context);

                return Next(context); 
            });

            return result;
        }

        private void ConfigureExpiration<TDecorated>(ref ICacheEntry entry, DecoratorContext<TDecorated> context)
        {
            var att = (context.AssociatedDecorateAttribute as MemCacheAttribute);
            _ = att?.Expiration switch
            {
                MemCacheAttribute.ExpirationType.Absolute => entry.AbsoluteExpirationRelativeToNow = att?.TimeSpan,
                MemCacheAttribute.ExpirationType.Sliding => entry.SlidingExpiration = att?.TimeSpan,
                _ => throw new UnreachableException(),
            };
        }
    }
}
