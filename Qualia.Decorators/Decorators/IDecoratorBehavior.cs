using System.Reflection;

namespace Qualia.Decorators
{
    public interface IDecoratorBehavior
    {
        public object? Invoke<TDecorated>(TDecorated decorated, MethodInfo targetMethod, object?[]? args);
    }
}
