using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public interface IDecoratorBehavior
    {
        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);
    }
}
