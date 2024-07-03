using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace Qualia.Decorators.Utils
{
    internal static class KeyGenerator
    {
        public static string CreateKey(MethodInfo targetMethod, object?[]? args)
        {
            Span<byte> serializedArgs = JsonSerializer.SerializeToUtf8Bytes(args, options);
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

        private static readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }
}
