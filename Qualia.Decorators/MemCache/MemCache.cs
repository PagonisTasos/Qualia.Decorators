using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System.Diagnostics;

namespace Qualia.Decorators
{

    public class MemCache : IDecoratorBehavior
    {
        public DecorateAttribute? AssociatedDecorateAttribute { get; set; }

        private MemCacheAttribute? _memCacheAttribute => AssociatedDecorateAttribute as MemCacheAttribute;

        private ILogger<Memoize> _logger;
        private readonly IMemoryCache _cache;

        public MemCache(ILogger<Memoize> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
        {
            var key = KeyGenerator.CreateKey(targetMethod, args);
            var result = _cache.GetOrCreate(key, entry => 
            {
                ConfigureExpiration(ref entry);

                return targetMethod.Invoke(decorated, args); 
            });

            return result;
        }

        private void ConfigureExpiration(ref ICacheEntry entry)
        {
            _ = _memCacheAttribute?.Expiration switch
            {
                MemCacheAttribute.ExpirationType.Absolut => entry.AbsoluteExpirationRelativeToNow = _memCacheAttribute?.TimeSpan,
                MemCacheAttribute.ExpirationType.Sliding => entry.SlidingExpiration = _memCacheAttribute?.TimeSpan,
                _ => throw new UnreachableException(),
            };
        }
    }
}
