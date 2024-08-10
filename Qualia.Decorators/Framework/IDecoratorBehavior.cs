using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public interface IDecoratorBehavior
    {
        public object? Invoke<TDecorated>(DecoratorContext<TDecorated> context);
    }
}
