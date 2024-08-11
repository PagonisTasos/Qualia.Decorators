namespace Qualia.Decorators.Framework
{
    public abstract class DecoratorBehavior : IDecoratorBehavior
    {
        public object Next<TDecorated>(DecoratorContext<TDecorated> context)
        {
            return context.TargetMethod.Invoke(context.Decorated, context.Args);
        }

        public abstract object Invoke<TDecorated>(DecoratorContext<TDecorated> context);
    }
}
