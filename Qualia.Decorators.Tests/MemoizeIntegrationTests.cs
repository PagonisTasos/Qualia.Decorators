using Moq;
using Qualia.Decorators.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Qualia.Decorators.Tests
{
    [TestFixture]
    public class MemoizeIntegrationTests : IDisposable
    {
        private IServiceProvider _serviceProvider;
        private Mock<Foo> _fooMock;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();

            // Create a mock of the Foo class implementing IFoo interface
            _fooMock = new Mock<Foo>();
            var loggerMock = new Mock<ILogger<Memoize>>();

            // Set up the Bar method to count invocations
            _fooMock.Setup(f => f.Bar()).Verifiable();

            // Register the mock in the service collection
            services.AddScoped(_ => loggerMock.Object);
            services.AddScoped<IFoo, Foo>(_ => _fooMock.Object);

            // Apply the decorators
            services.UseDecorators();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Bar_Method_Should_Be_Memoized()
        {
            // Arrange
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to memoization.");
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the service provider to release any resources
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public void Dispose()
        {
            TearDown();
        }
        public interface IFoo
        {
            void Bar();
        }

        [Memoize]
        public class Foo : IFoo
        {
            public virtual void Bar()
            { }
        }
    }
}
