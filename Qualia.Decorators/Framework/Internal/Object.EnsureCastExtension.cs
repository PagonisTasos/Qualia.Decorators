using System;

namespace Qualia.Decorators.Framework
{
    internal static class ObjectExtensions
    {
        public static T EnsureCast<T>(this object o) where T : class
        {
            return o as T ?? throw new ArgumentException($"Casting to {typeof(T)} failed.");
        }
    }
}
