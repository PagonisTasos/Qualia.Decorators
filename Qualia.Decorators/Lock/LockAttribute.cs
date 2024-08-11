using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{
    public class LockAttribute : DecorateAttribute
    {
        public LockAttribute(string name = null) : base(typeof(Lock), name)
        { }
    }
}