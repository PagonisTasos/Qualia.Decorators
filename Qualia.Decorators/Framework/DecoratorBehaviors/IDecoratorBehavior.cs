namespace Qualia.Decorators.Framework
{
    public interface IDecoratorBehavior
    {
        object Invoke<TDecorated>(DecoratorContext<TDecorated> context);
    }
}
