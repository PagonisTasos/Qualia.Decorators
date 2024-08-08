using System.Security.AccessControl;

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

        public object? Next<TDecorated>(DecoratorContext<TDecorated> context)
        {
            return context.TargetMethod.Invoke(context.Decorated, context.Args);
        }

        public abstract object? Invoke<TDecorated>(DecoratorContext<TDecorated> context);
    }
}
