using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Qualia.Decorators.Utils;
using Qualia.Decorators.Framework;

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
            var key = KeyGenerator.CreateKey(context.TargetMethod, context.Args);
            var result = _cache.GetOrAdd(key, _ => Next(context));

            return result;
        }
    }
}
