using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System.Diagnostics;
using System;
using System.Linq;

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

        public override object Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            var att = (context.AssociatedDecorateAttribute as MemCacheAttribute);

            var varyByIndices = 
                context.TargetMethod
                .GetParameters()
                .Select((p,i) => (p.Name,i))
                .Where(t => att?.VaryBy?.Contains(t.Name) ?? false)
                .Select(t => t.i)
                .ToArray();

            var cacheKeyArgs = 
                context.Args?
                .Select((a,i) => (a,i))
                .Where(t => varyByIndices.Contains(t.i))
                .Select(t => t.a)
                .ToArray()
                ?? Enumerable.Empty<object>().ToArray();

            var key = KeyGenerator.CreateKey(context.TargetMethod, cacheKeyArgs);
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

            if (att?.Expiration == MemCacheAttribute.ExpirationType.Absolute)
            {
                entry.AbsoluteExpirationRelativeToNow = att?.TimeSpan;
                return;
            }
            if (att?.Expiration == MemCacheAttribute.ExpirationType.Sliding)
            {
                entry.SlidingExpiration = att?.TimeSpan;
                return;
            }

            throw new InvalidOperationException("MemCache decorator behavior failed while determining attribute's expiration type.");
        }
    }
}
