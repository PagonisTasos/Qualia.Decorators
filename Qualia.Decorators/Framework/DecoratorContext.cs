using System.Reflection;

namespace Qualia.Decorators.Framework
{
    public class DecoratorContext<TDecorated>
    {
        public required TDecorated Decorated { get; init; }
        public required MethodInfo TargetMethod { get; init; }
        public required object?[]? Args { get; init; }
    }
}
