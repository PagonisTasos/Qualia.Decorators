namespace Qualia.Decorators
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DecorateIgnoreAttribute(Type? decoratorBehavior = null, string? name = null) : Attribute
    {
        public Type? DecoratorBehavior { get; } = decoratorBehavior;
        public string? Name { get; } = name;
    }
}
