using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;
using System.Linq;

namespace Qualia.Decorators
{

    public class Memoize : DecoratorBehavior
    {
        private ILogger<Memoize> _logger;
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public Memoize(ILogger<Memoize> logger)
        {
            _logger = logger;
        }

        public override object Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            var att = (context.AssociatedDecorateAttribute as MemoizeAttribute);

            var varyByIndices =
                context.TargetMethod
                .GetParameters()?
                .Select((p, i) => (p.Name, i))
                .Where(t => att?.VaryBy?.Contains(t.Name) ?? false)
                .Select(t => t.i)
                .ToArray();

            var cacheKeyArgs =
                context.Args?
                .Select((a, i) => (a, i))
                .Where(t => varyByIndices.Contains(t.i))
                .Select(t => t.a)
                .ToArray()
                ?? Enumerable.Empty<object>().ToArray();

            var key = KeyGenerator.CreateKey(context.TargetMethod, cacheKeyArgs);
            var result = _cache.GetOrAdd(key, _ => Next(context));

            return result;
        }
    }
}
