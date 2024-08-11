using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public class DecoratorContext<TDecorated>
    {
        public DecoratorContext(DecorateAttribute associatedDecorateAttribute, TDecorated decorated, MethodInfo targetMethod, object[] args)
        {
            AssociatedDecorateAttribute = associatedDecorateAttribute;
            Decorated = decorated;
            TargetMethod = targetMethod;
            Args = args;
        }

        public DecorateAttribute AssociatedDecorateAttribute { get; }
        public TDecorated Decorated { get; }
        public MethodInfo TargetMethod { get; }
        public object[] Args { get; }
    }
}
