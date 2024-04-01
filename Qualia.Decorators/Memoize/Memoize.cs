using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace Qualia.Decorators
{
    public class Memoize : IDecoratorBehavior
    {
        private ILogger<Memoize> _logger;
        private readonly ConcurrentDictionary<string, object?> _cache = new();

        public Memoize(ILogger<Memoize> logger)
        {
            _logger = logger;
        }

        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
        {
            try
            {
                var key = GenerateCacheKey(targetMethod, args);
                var result = _cache.GetOrAdd(key, _ => targetMethod.Invoke(decorated, args));

                return result;
            }
            catch (TargetInvocationException ex)
            {
                _logger?.LogError(ex.InnerException ?? ex,
                    "Error during invocation of {decoratedClass}.{methodName}", typeof(TDecorated), targetMethod?.Name);
                throw ex.InnerException ?? ex;
            }
        }

        private static string GenerateCacheKey(MethodInfo targetMethod, object?[]? args)
        {
            var serializedArgs = JsonSerializer.Serialize(args);
            byte[] bytes = Encoding.UTF8.GetBytes(serializedArgs);
            byte[] hashBytes = SHA1.HashData(bytes);
            return $"{targetMethod.Name}_{BitConverter.ToString(hashBytes).Replace("-", "")}";
        }
    }
}
