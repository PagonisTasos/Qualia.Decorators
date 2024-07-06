using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public interface IDecoratorBehavior
    {
        public void AssignAssociatedDecorateAttribute(DecorateAttribute decorateAttribute);
        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);
    }
}
