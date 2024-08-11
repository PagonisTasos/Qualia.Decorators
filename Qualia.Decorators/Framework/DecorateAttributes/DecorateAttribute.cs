namespace Qualia.Decorators.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class DecorateAttribute(Type decoratorBehavior, string? name = null) : Attribute
    {
        public Type DecoratorBehavior { get; } = decoratorBehavior;
        public string? Name { get; } = name;
    }
}
