using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Qualia.Decorators.Framework
{
    public static partial class ServiceCollectionExtensions
    {
        internal static class DecorateDescriptorsExtractor
        {
            public static List<DecorateDescriptor> GetDecorateDescriptors(ServiceDescriptor descriptor)
            {

                var classDecorateDescriptors = GetAllClassDecorateDescriptors(descriptor);
                var methodDecorateDescriptors = GetAllMethodDecorateDescriptors(descriptor);

                var decorateDescriptors = classDecorateDescriptors.Concat(methodDecorateDescriptors).Reverse().ToList();
                //in reverse, so that the ones declared first correspond to outer decorators,
                //and the ones declared last correspond to inner decorators

                return decorateDescriptors;
            }

            private static List<DecorateDescriptor> GetAllClassDecorateDescriptors(ServiceDescriptor descriptor)
            {
                if (descriptor.ImplementationType == null) return Enumerable.Empty<DecorateDescriptor>().ToList();

                var classDecoratorBehaviors = descriptor.ImplementationType.GetCustomAttributes<DecorateAttribute>()
                                                .Select(d => new DecorateDescriptor { MethodName = null, DecorateAttribute = d }).ToList();

                return classDecoratorBehaviors;
            }

            private static List<DecorateDescriptor> GetAllMethodDecorateDescriptors(ServiceDescriptor descriptor)
            {
                if (descriptor.ImplementationType == null) return Enumerable.Empty<DecorateDescriptor>().ToList();

                var methodDecoratorBehaviors = descriptor.ImplementationType.GetMethods().SelectMany(m =>
                                    m.GetCustomAttributes<DecorateAttribute>()
                                    .Select(d => new DecorateDescriptor { MethodName = m.Name, DecorateAttribute = d })).ToList();

                return methodDecoratorBehaviors;
            }
        }

    }
}
