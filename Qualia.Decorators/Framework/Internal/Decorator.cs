﻿using System.Reflection;

namespace Qualia.Decorators.Framework
{
    internal class Decorator<TDecorated> : DispatchProxy
    {
        private DecorateAttribute? _associatedAttribute;
        private TDecorated? _decorated;
        private IDecoratorBehavior? _decoratorBehavior;
        private string? _decoratorName;
        private string? _methodName;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null) return default;
            if (_decorated == null) return default;
            if (_decoratorBehavior == null) return default;

            //if the decorator is method specific, and this a delegation to a different method, just pass through
            if (!string.IsNullOrEmpty(_methodName) && _methodName != targetMethod.Name)
            {
                return targetMethod.Invoke(_decorated, args);
            }

            //else (this is a class decor || it is a method decor and we are calling that method)

            //if is a class decor, check the ignores
            var ignoresOnMethod = typeof(TDecorated).GetMethod(targetMethod.Name)?.GetCustomAttributes<DecorateIgnoreAttribute>() ?? [];
            if (string.IsNullOrEmpty(_methodName))//is class decor
            {
                bool isNamedDecor = !string.IsNullOrEmpty(_decoratorName);
                bool isIgnoredName = ignoresOnMethod.Any(i => i.Name == _decoratorName);
                if (isNamedDecor && isIgnoredName)
                {
                    return targetMethod.Invoke(_decorated, args);
                }

                bool isIgnoredType = ignoresOnMethod.Any(i => i.DecoratorBehavior == _decoratorBehavior.GetType());
                if (isIgnoredType)
                {
                    return targetMethod.Invoke(_decorated, args);
                }
            }

            try
            {
                //is a method decor and we are calling this method
                var ctx = new DecoratorContext<TDecorated> 
                { 
                    AssociatedDecorateAttribute = _associatedAttribute,
                    Decorated = _decorated, 
                    TargetMethod = targetMethod, 
                    Args = args 
                };
                return _decoratorBehavior?.Invoke(ctx);
            }
            catch (TargetInvocationException ex)
            {
                throw new TargetInvocationException($"Error during invocation of {typeof(TDecorated)}.{targetMethod?.Name}", ex.InnerException ?? ex);
            }
        }

        public static TDecorated Create(DecorateAttribute attribute, TDecorated decorated, IDecoratorBehavior decoratorBehavior, string? methodName = null)
        {
            object proxy = Create<TDecorated, Decorator<TDecorated>>()
                            ?? throw new NullReferenceException("DispatchProxy for Decorator was null.");

            ((Decorator<TDecorated>)proxy).SetParameters(attribute, decorated, decoratorBehavior, attribute.Name, methodName);

            return (TDecorated)proxy;
        }

        private void SetParameters(DecorateAttribute attribute, TDecorated decorated, IDecoratorBehavior decoratorBehavior, string? decoratorName = null, string? methodName = null)
        {
            _associatedAttribute = attribute;
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _decoratorBehavior = decoratorBehavior ?? throw new ArgumentNullException(nameof(decoratorBehavior));
            _decoratorName = decoratorName;
            _methodName = methodName;
        }
    }
}
