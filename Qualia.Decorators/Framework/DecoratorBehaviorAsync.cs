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
            //to apply async behavior, the target method should be a Task.
            //If that is not the case the client will run synchronously,
            //and sync over async should be avoided. A runtime exeption is throw (which should be move at compile time in the future)
            if (!typeof(Task).IsAssignableFrom(context.TargetMethod.ReturnType))
                throw new InvalidOperationException($"{typeof(TDecorated).Name} behavior cannot run on synchronous methods.");

            //The target method may be a Task or Task<T>. We want to handle both cases with one InvokeAsync method.
            //In case of Task, null will be returned, so we can handle that as an "object" return type.
            Type returnType = context.TargetMethod.ReturnType.GenericTypeArguments.Length > 0
                        ? context.TargetMethod.ReturnType.GenericTypeArguments[0]
                        : typeof(object);

            return _invokeAsync
                            .MakeGenericMethod(typeof(TDecorated), returnType)
                            .Invoke(this, [context]);
        }

        public async Task<TReturn> Next<TDecorated, TReturn>(DecoratorContext<TDecorated> context)
        {
            //targetMethod.Invoke returns object, but we know this will be a Task or Task<T> so we
            //use dynamic to allow awaiting.

            if (context.TargetMethod.ReturnType.GenericTypeArguments.Length == 0)
            {
                //in case of Task we cant return the result of type void, so we'll return null.
                //the client does not wait for a result anyway.
                await (dynamic?)context.TargetMethod.Invoke(context.Decorated, context.Args);
                return default(TReturn)!;
            }

            //object from Invoke is Task<T> here, so it is ok to await it and return the result.
            return await (dynamic?)context.TargetMethod.Invoke(context.Decorated, context.Args);
        }

        public abstract Task<TReturn> InvokeAsync<TDecorated, TReturn>(DecoratorContext<TDecorated> context);
    }
}
