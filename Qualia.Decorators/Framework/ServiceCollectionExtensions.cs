using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Qualia.Decorators.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseDecorators(this IServiceCollection services)
        {
            var descriptorsWithDecorators = services.Where(HasDecorator).ToList();

            foreach (var descriptor in descriptorsWithDecorators)
            {
                services.Decorate(descriptor);
            }

            return services;
        }

        private static IServiceCollection Decorate(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            var decoratorType = typeof(Decorator<>).MakeGenericType(descriptor.ServiceType);
            var method = typeof(ServiceCollectionExtensions)
                            .GetMethod(nameof(DecorateWithDispatchProxy), BindingFlags.NonPublic | BindingFlags.Static)
                            ?.MakeGenericMethod(descriptor.ServiceType, decoratorType);

            if (method == null) return services;

            method.Invoke(null, new object[] { services, descriptor });
            return services;
        }

        private static IServiceCollection DecorateWithDispatchProxy<TInterface, TProxy>(this IServiceCollection services, ServiceDescriptor descriptor)
            where TInterface : class
            where TProxy : DispatchProxy
        {
            if (descriptor.ImplementationType == null) return services;

            var classDecoratorBehaviors = descriptor.ImplementationType.GetCustomAttributes<DecorateAttribute>()
                                            .Select(d => new NamedDecor { MethodName = null, DecorateAttribute = d }).ToList();

            var methodDecoratorBehaviors = descriptor.ImplementationType.GetMethods().SelectMany(m => 
                                            m.GetCustomAttributes<DecorateAttribute>()
                                            .Select(d => new NamedDecor { MethodName = m.Name, DecorateAttribute = d })).ToList();

            var namedDecoratorBehaviors = classDecoratorBehaviors.Concat(methodDecoratorBehaviors);

            foreach (var namedDecoratorBehavior in namedDecoratorBehaviors.Reverse())
            {
                if (namedDecoratorBehavior.DecorateAttribute?.DecoratorBehavior == null) continue;

                // If not registered, add the service
                if (!services.Any(descriptor => descriptor.ServiceType == namedDecoratorBehavior.DecorateAttribute.DecoratorBehavior))
                {
                    services.AddTransient(namedDecoratorBehavior.DecorateAttribute.DecoratorBehavior);
                }
            }

            ServiceDescriptor descorated = ServiceDescriptor.Describe(
            descriptor.ServiceType,
            sp =>
            {
                //init with actual implementation type
                TInterface? decoratedInstance = sp.CreateInstance(descriptor).EnsureCast<TInterface>();

                foreach (var namedDecoratorBehavior in namedDecoratorBehaviors.Reverse())
                {
                    if (namedDecoratorBehavior.DecorateAttribute?.DecoratorBehavior == null) continue;

                    var behavior = sp.GetRequiredService(namedDecoratorBehavior.DecorateAttribute.DecoratorBehavior).EnsureCast<IDecoratorBehavior>();
                    behavior.AssignAssociatedDecorateAttribute(namedDecoratorBehavior.DecorateAttribute);

                    decoratedInstance = Decorator<TInterface>.Create(
                        decorated: decoratedInstance,
                        decoratorBehavior: behavior,
                        decoratorName: namedDecoratorBehavior.DecorateAttribute.Name,
                        methodName: namedDecoratorBehavior.MethodName
                        );
                }

                return decoratedInstance;
            },
            descriptor.Lifetime);

            services.Remove(descriptor);
            services.Add(descorated);

            return services;
        }

        private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            var type = descriptor.ImplementationType ?? throw new NullReferenceException("Service descriptor is missing ImplementationType.");
            return ActivatorUtilities.GetServiceOrCreateInstance(services, type);
        }

        private static bool HasDecorator(ServiceDescriptor descriptor) 
            => HasDecorator(descriptor.ImplementationType ?? descriptor.ServiceType);

        private static bool HasDecorator(Type type) 
            => HasTypeDecorator(type) || HasMethodDecorator(type);

        private static bool HasTypeDecorator(Type type) 
            => type.GetCustomAttributes(true).Any(IsDecorateAttributeOrDerivedFromIt);

        private static bool HasMethodDecorator(Type type) 
            => type.GetMethods().Any(m => m.GetCustomAttributes(true).Any(IsDecorateAttributeOrDerivedFromIt));

        private static bool IsDecorateAttributeOrDerivedFromIt(object o)
            => typeof(DecorateAttribute).IsAssignableFrom(o.GetType());

        private class NamedDecor
        {
            public string? MethodName { get; set; }
            public DecorateAttribute? DecorateAttribute { get; set; }
        }

    }
}
