using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Qualia.Decorators.Framework
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection UseDecorators(this IServiceCollection services)
        {
            var descriptorsWithDecorateAttribute = services.Where(DecorateAttributeFinder.HasDecorateAttribute).ToList();

            foreach (var descriptor in descriptorsWithDecorateAttribute)
            {
                services.Decorate(descriptor);
            }

            return services;
        }

        private static IServiceCollection Decorate(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            var method = CreateTheGenericMethodForDecoratingDescriptor(descriptor);

            if (method == null) return services;

            method.Invoke(null, [services, descriptor]);
            return services;
        }

        private static MethodInfo? CreateTheGenericMethodForDecoratingDescriptor(ServiceDescriptor descriptor)
        {
            var decoratorType = typeof(Decorator<>).MakeGenericType(descriptor.ServiceType); //ex: Decorator<ICustomService>
            var method = typeof(ServiceCollectionExtensions)
                            .GetMethod(nameof(DecorateWithDispatchProxy), BindingFlags.NonPublic | BindingFlags.Static)
                            ?.MakeGenericMethod(descriptor.ServiceType, decoratorType);

            //ex: method is DecorateWithDispatchProxy<ICustomService, Decorator<ICustomService>>(...)
            return method;
        }

        private static IServiceCollection DecorateWithDispatchProxy<TInterface, TProxy>(this IServiceCollection services, ServiceDescriptor serviceDescriptor)
            where TInterface : class
            where TProxy : DispatchProxy
        {
            var decorateDescriptors = DecorateDescriptorsExtractor.GetDecorateDescriptors(serviceDescriptor);

            if (decorateDescriptors.Count == 0) return services;

            RegisterTransientServicesDeclaredInDecorateDescriptors(services, decorateDescriptors);

            ServiceDescriptor descorated = DecorateTheServiceDescriptor<TInterface>(serviceDescriptor, decorateDescriptors);

            services.Remove(serviceDescriptor);
            services.Add(descorated);

            return services;
        }

        private static void RegisterTransientServicesDeclaredInDecorateDescriptors(IServiceCollection services, List<DecorateDescriptor> decorateDescriptors)
        {
            foreach (var decorateDescriptor in decorateDescriptors)
            {
                if (decorateDescriptor.DecorateAttribute?.DecoratorBehavior == null) continue;

                // If not registered, add the service
                if (!services.Any(descriptor => descriptor.ServiceType == decorateDescriptor.DecorateAttribute.DecoratorBehavior))
                {
                    services.AddTransient(decorateDescriptor.DecorateAttribute.DecoratorBehavior);
                }
            }
        }

        private static ServiceDescriptor DecorateTheServiceDescriptor<TInterface>(
            ServiceDescriptor serviceDescriptor, List<DecorateDescriptor> decorateDescriptors)
            where TInterface : class
        {
            ServiceDescriptor decorated = ServiceDescriptor.Describe(
            serviceDescriptor.ServiceType,
            sp =>
            {
                //init with actual implementation type
                TInterface? decoratedInstance = sp.CreateServiceInstance(serviceDescriptor).EnsureCast<TInterface>();

                foreach (var namedDecoratorBehavior in decorateDescriptors)
                {
                    if (namedDecoratorBehavior.DecorateAttribute?.DecoratorBehavior == null) continue;

                    var behavior = sp.GetRequiredService(namedDecoratorBehavior.DecorateAttribute.DecoratorBehavior).EnsureCast<IDecoratorBehavior>();

                    decoratedInstance = Decorator<TInterface>.Create(
                        attribute: namedDecoratorBehavior.DecorateAttribute,
                        decorated: decoratedInstance,
                        decoratorBehavior: behavior,
                        methodName: namedDecoratorBehavior.MethodName
                        );
                }

                return decoratedInstance;
            },
            serviceDescriptor.Lifetime);

            return decorated;
        }

        private static object CreateServiceInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            var type = descriptor.ImplementationType ?? throw new NullReferenceException("Service descriptor is missing ImplementationType.");
            return ActivatorUtilities.GetServiceOrCreateInstance(services, type);
        }
    }
}
