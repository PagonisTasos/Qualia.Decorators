namespace Qualia.Decorators.Framework
{
    public static partial class ServiceCollectionExtensions
    {
        internal class DecorateDescriptor
        {
            public string? MethodName { get; set; }
            public DecorateAttribute? DecorateAttribute { get; set; }
        }

    }
}
