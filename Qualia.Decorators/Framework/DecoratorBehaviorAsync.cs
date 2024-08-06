using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public abstract class DecoratorBehaviorAsync<TAttribute> : IDecoratorBehavior where TAttribute : DecorateAttribute
    {
        private DecorateAttribute? _associatedDecorateAttribute { get; set; }
        protected TAttribute? AssociatedAttribute => _associatedDecorateAttribute as TAttribute;

        private static MethodInfo _invokeAsync = typeof(DecoratorBehaviorAsync<TAttribute>)
                                            .GetMethods()
                                            .First(m => m.Name == nameof(InvokeAsync) && m.ContainsGenericParameters);

        public void AssignAssociatedDecorateAttribute(DecorateAttribute decorateAttribute)
        {
            _associatedDecorateAttribute = decorateAttribute;
        }

        public object? Invoke<TDecorated>(DecoratorContext<TDecorated> context)
        {
            if (!typeof(Task).IsAssignableFrom(context.TargetMethod.ReturnType))
                throw new InvalidOperationException($"{typeof(TDecorated).Name} behavior cannot run on synchronous methods.");

            return _invokeAsync
                    .MakeGenericMethod(typeof(TDecorated), context.TargetMethod.ReturnType.GenericTypeArguments[0])
                    .Invoke(this, [context]);
        }

        public abstract Task<TReturn> InvokeAsync<TDecorated, TReturn>(DecoratorContext<TDecorated> context);
    }
}
