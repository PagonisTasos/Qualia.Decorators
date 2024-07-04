using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public interface IDecoratorBehavior
    {
        public DecorateAttribute? AssociatedDecorateAttribute { get; set; }
        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);
    }
}
