using System;

namespace Qualia.Decorators.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class DecorateAttribute : Attribute
    {
        public DecorateAttribute(Type decoratorBehavior, string name = null)
        {
            DecoratorBehavior = decoratorBehavior;
            Name = name;
        }
        public Type DecoratorBehavior { get; }
        public string Name { get; }
    }
}
