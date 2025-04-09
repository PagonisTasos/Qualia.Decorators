using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Qualia.Decorators.Framework
{
    public static partial class ServiceCollectionExtensions
    {
        private static bool _implicitDecoration;
        private static readonly List<ServiceDescriptor> _decoratedDescriptors = new List<ServiceDescriptor>();

        public static IServiceCollection AddDecoratedSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddSingleton<TService, TImplementation>();
            var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddSingleton<TService, TImplementation>(_ => new TImplementation());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            //services.AddSingleton<TService>(_ => new TService());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedSingleton<TService>(this IServiceCollection services, TService implementationInstance)
            where TService : class
        {
            //services.AddSingleton<TService>(new TService());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationInstance);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddScoped<TService, TImplementation>();
            var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddScoped<TService, TImplementation>(_ => new TImplementation());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            //services.AddScoped<TService, TImplementation>(_ => new TService());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddTransient<TService, TImplementation>();
            var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            //services.AddTransient<TService, TImplementation>(_ => new TImplementation());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Transient);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection AddDecoratedTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            //services.AddTransient<TService>(_ => new TService());
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Transient);
            _decoratedDescriptors.Add(descriptor);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection UseDecorators(this IServiceCollection services, bool implicitDecoration = false)
        {
            _implicitDecoration = implicitDecoration;

            GetAllDecoratorBehaviors().ForEach(decor => services.AddTransient(decor));

            var descriptorsWithDecorateAttribute =
                _implicitDecoration
                ? services.Where(service => service.ServiceType.IsInterface).ToList()
                : _decoratedDescriptors
                ;

            foreach (var descriptor in descriptorsWithDecorateAttribute)
            {
                services.Decorate(descriptor);
            }

            return services;
        }

        public static List<Type> GetAllDecoratorBehaviors()
        {
            var interfaceType = typeof(IDecoratorBehavior);

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic) // skip dynamic assemblies like generated proxies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                })
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
        }

        private static IServiceCollection Decorate(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            var method = CreateTheGenericMethodForDecoratingDescriptor(descriptor);

            if (method == null) return services;

            method.Invoke(null, new object[] { services, descriptor });
            return services;
        }

        private static MethodInfo CreateTheGenericMethodForDecoratingDescriptor(ServiceDescriptor descriptor)
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
            ServiceDescriptor descorated = DecorateTheServiceDescriptor<TInterface>(serviceDescriptor);

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
            ServiceDescriptor serviceDescriptor)
            where TInterface : class
        {
            ServiceDescriptor decorated = ServiceDescriptor.Describe(
            serviceDescriptor.ServiceType,
            sp =>
            {
                //init with actual implementation type
                TInterface decoratedInstance = sp.CreateServiceInstance(serviceDescriptor).EnsureCast<TInterface>();

                var serviceConcreteType = decoratedInstance.GetType();
                bool needsDecoration = DecorateAttributeFinder.HasDecorateAttribute(serviceConcreteType);
                if (!needsDecoration) return decoratedInstance;

                List<DecorateDescriptor> decorateDescriptors = DecorateDescriptorsExtractor.GetDecorateDescriptors(serviceConcreteType);

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
