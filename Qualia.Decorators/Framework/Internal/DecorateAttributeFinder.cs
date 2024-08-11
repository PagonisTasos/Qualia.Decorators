using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Qualia.Decorators.Framework
{
    internal static class DecorateAttributeFinder
    {
        public static bool HasDecorateAttribute(ServiceDescriptor descriptor)
            => HasDecorateAttribute(descriptor.ImplementationType ?? descriptor.ServiceType);

        private static bool HasDecorateAttribute(Type type)
            => HasClassDecorateAttribute(type) || HasMethodDecorateAttribute(type);

        private static bool HasClassDecorateAttribute(Type type)
            => type.GetCustomAttributes(true).Any(IsDecorateAttributeOrDerivedFromIt);

        private static bool HasMethodDecorateAttribute(Type type)
            => type.GetMethods().Any(m => m.GetCustomAttributes(true).Any(IsDecorateAttributeOrDerivedFromIt));

        private static bool IsDecorateAttributeOrDerivedFromIt(object o)
            => typeof(DecorateAttribute).IsAssignableFrom(o.GetType());
    }
}
