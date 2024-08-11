using System;

namespace Qualia.Decorators.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DecorateIgnoreAttribute : Attribute
    {
        public DecorateIgnoreAttribute(Type decoratorBehavior = null, string name = null)
        {
            DecoratorBehavior = decoratorBehavior;
            Name = name;
        }
        public Type DecoratorBehavior { get; }
        public string Name { get; }
    }
}
