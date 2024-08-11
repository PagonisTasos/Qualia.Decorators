using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace Qualia.Decorators.Utils
{
    internal static class KeyGenerator
    {
        public static string CreateKey(MethodInfo targetMethod, object[] args)
        {
            byte[] serializedArgs = JsonSerializer.SerializeToUtf8Bytes(args, options);
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(serializedArgs);
            char[] hex = ByteToHexBitFiddle(hashBytes);

            return string.Concat(targetMethod.Name, "_", hex);
        }

        private static char[] ByteToHexBitFiddle(byte[] bytes)
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
            return c;
        }

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }
}
