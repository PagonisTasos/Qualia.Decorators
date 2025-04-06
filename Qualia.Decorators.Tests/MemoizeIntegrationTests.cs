using Moq;
using Qualia.Decorators.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Qualia.Decorators.Tests
{
    [TestFixture]
    public class MemoizeIntegrationTests
    {
        [Test]
        public void ImplementationFactory_w_ConcreteClassReturnType_Bar_Method_Should_Be_Memoized()
        {
            // Arrange

            IServiceCollection services = new ServiceCollection();

            // Create a mock of the Foo class implementing IFoo interface
            var _fooMock = new Mock<Foo>();
            var loggerMock = new Mock<ILogger<Memoize>>();

            // Set up the Bar method to count invocations
            _fooMock.Setup(f => f.Bar()).Verifiable();

            // Register the mock in the service collection
            services.AddScoped(_ => loggerMock.Object);

            services.AddSingleton<IFoo, Foo>(_ => _fooMock.Object);

            // Apply the decorators
            services.UseDecorators();

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to memoization.");
        }

        [Test]
        public void ImplementationInstance_w_AbstractClassType_Bar_Method_Should_Be_Memoized()
        {
            // Arrange

            IServiceCollection services = new ServiceCollection();

            // Create a mock of the Foo class implementing IFoo interface
            var _fooMock = new Mock<Foo>();
            var loggerMock = new Mock<ILogger<Memoize>>();

            // Set up the Bar method to count invocations
            _fooMock.Setup(f => f.Bar()).Verifiable();

            // Register the mock in the service collection
            services.AddScoped(_ => loggerMock.Object);

            services.AddSingleton<IFoo>(_fooMock.Object);

            // Apply the decorators
            services.UseDecorators();

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to memoization.");
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
