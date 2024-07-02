using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
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
            var key = GenerateCacheKey(targetMethod, args);
            var result = _cache.GetOrAdd(key, _ => targetMethod.Invoke(decorated, args));

            return result;
        }

        public static string GenerateCacheKey(MethodInfo targetMethod, object?[]? args)
        {
            Span<byte> serializedArgs = JsonSerializer.SerializeToUtf8Bytes(args);
            Span<byte> hashBytes = SHA1.HashData(serializedArgs);
            Span<char> hex = ByteToHexBitFiddle(hashBytes);

            return string.Concat(targetMethod.Name.AsSpan(), "_".AsSpan(), hex);
        }

        private static Span<char> ByteToHexBitFiddle(Span<byte> bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return c.AsSpan();
        }
    }
}
