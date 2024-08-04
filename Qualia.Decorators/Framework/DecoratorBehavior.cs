using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public abstract class DecoratorBehavior<TAttribute> : IDecoratorBehavior where TAttribute : DecorateAttribute
    {
        private DecorateAttribute? _associatedDecorateAttribute { get; set; }
        protected TAttribute? AssociatedAttribute => _associatedDecorateAttribute as TAttribute;

        public void AssignAssociatedDecorateAttribute(DecorateAttribute decorateAttribute)
        {
            _associatedDecorateAttribute = decorateAttribute;
        }

        public abstract object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);

    }

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

        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args)
        {
            if (!typeof(Task).IsAssignableFrom(targetMethod.ReturnType))
                throw new InvalidOperationException($"{this.GetType().Name} behavior cannot run on synchronous methods.");

            return _invokeAsync
                    .MakeGenericMethod(typeof(TDecorated), targetMethod.ReturnType.GenericTypeArguments[0])
                    .Invoke(this, [decorated, targetMethod, args]);
        }

        public abstract Task<TReturn> InvokeAsync<TDecorated, TReturn>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);
    }
}
