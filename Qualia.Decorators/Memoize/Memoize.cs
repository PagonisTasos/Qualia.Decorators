using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{

    public class Memoize : DecoratorBehavior<MemoizeAttribute>
    {
        private ILogger<Memoize> _logger;
        private readonly ConcurrentDictionary<string, object?> _cache = new();

        public Memoize(ILogger<Memoize> logger)
        {
            _logger = logger;
        }

        public override object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
        {
            var key = KeyGenerator.CreateKey(targetMethod, args);
            var result = _cache.GetOrAdd(key, _ => targetMethod.Invoke(decorated, args));

            return result;
        }
    }
}
