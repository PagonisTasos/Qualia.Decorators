using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Qualia.Decorators.Framework
{
    internal static class ServiceDescriptorExtensions
    {
        public static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor.ServiceKey == null)
            {
                if (descriptor.ImplementationType != null)
                {
                    return descriptor.ImplementationType;
                }
                else if (descriptor.ImplementationInstance != null)
                {
                    return descriptor.ImplementationInstance.GetType();
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    return descriptor.ImplementationFactory.GetType().GenericTypeArguments[1];
                }
            }
            else
            {
                if (descriptor.KeyedImplementationType != null)
                {
                    return descriptor.KeyedImplementationType;
                }
                else if (descriptor.KeyedImplementationInstance != null)
                {
                    return descriptor.KeyedImplementationInstance.GetType();
                }
                else if (descriptor.KeyedImplementationFactory != null)
                {
                    return descriptor.ImplementationFactory.GetType().GenericTypeArguments[2];
                }
            }

            Debug.Assert(false, "ImplementationType, ImplementationInstance, ImplementationFactory or KeyedImplementationFactory must be non null");
            return null;
        }
    }
}
