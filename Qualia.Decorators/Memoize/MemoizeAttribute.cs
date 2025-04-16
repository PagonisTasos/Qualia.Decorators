using Qualia.Decorators.Framework;

namespace Qualia.Decorators
{
    public class MemoizeAttribute : DecorateAttribute
    {
        public string[] VaryBy { get; set; }

        public MemoizeAttribute(string name = null, string[] varyBy = null) : base(typeof(Memoize), name)
        {
            VaryBy = varyBy;
        }
    }
}
