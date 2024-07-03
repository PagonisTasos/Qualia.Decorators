using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{
    public class MemCache : IDecoratorBehavior
    {
        private ILogger<Memoize> _logger;
        private readonly IMemoryCache _cache;
        //private readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);//inject?

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
                //entry.AbsoluteExpirationRelativeToNow = _timeout;
                return targetMethod.Invoke(decorated, args); 
            });

            return result;
        }
    }
}
