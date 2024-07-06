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
}
