using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{
    public class MemoizeAttribute : DecorateAttribute
    {
        public MemoizeAttribute(string name = null) : base(typeof(Memoize), name) { }
    }
}
