using Moq;
using Qualia.Decorators.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Qualia.Decorators.Tests
{
    [TestFixture]
    public class ImplicitDecorationIntegrationTests
    {
        [Test]
        public void ConcreteImplementationType_Should_Be_Memoized()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var loggerMock = new Mock<ILogger<Memoize>>();
            services.AddScoped(_ => loggerMock.Object);
            services.AddSingleton<IFoo, Foo>();
            services.AddSingleton<IFoo2, Foo2>();

            // Apply the decorators
            services.UseDecorators(true);

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            Assert.That(foo.Counter, Is.EqualTo(1));
        }

        [Test]
        public void ConcreteImplementationFactory_Should_Be_Memoized()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var loggerMock = new Mock<ILogger<Memoize>>();
            services.AddScoped(_ => loggerMock.Object);
            var foo2 = new Foo2();
            services.AddSingleton<IFoo, Foo>(_ => new Foo(foo2));
            services.AddSingleton<IFoo2, Foo2>(_ => foo2);

            // Apply the decorators
            services.UseDecorators(true);

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            Assert.That(foo.Counter, Is.EqualTo(1));
        }

        [Test]
        public void AbstractImplementationFactory_Should_Be_Memoized()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var loggerMock = new Mock<ILogger<Memoize>>();
            services.AddScoped(_ => loggerMock.Object);
            var foo2 = new Foo2();
            services.AddSingleton<IFoo>(_ => new Foo(foo2));
            services.AddSingleton<IFoo2>(_ => foo2);

            // Apply the decorators
            services.UseDecorators(true);

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            Assert.That(foo.Counter, Is.EqualTo(1));
        }

        [Test]
        public void AbstractImplementationInstance_Should_Be_Memoized()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var loggerMock = new Mock<ILogger<Memoize>>();
            services.AddScoped(_ => loggerMock.Object);
            var foo2 = new Foo2();
            services.AddSingleton<IFoo>(new Foo(foo2));
            services.AddSingleton<IFoo2>(foo2);

            // Apply the decorators
            services.UseDecorators(true);

            // Build the service provider
            var _serviceProvider = services.BuildServiceProvider();
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call
            foo.Bar(); // Second call - should hit the cache

            // Assert
            Assert.That(foo.Counter, Is.EqualTo(1));
        }

        public interface IFoo
        {
            void Bar();
            int Counter { get; }
        }

        [Memoize]
        public class Foo : IFoo
        {
            public Foo(IFoo2 foo)
            {
                    
            }
            public virtual void Bar()
            { 
                Counter++;
            }

            public int Counter { get; private set; }
        }

        public interface IFoo2
        {
            void Bar();
        }

        [Memoize]
        public class Foo2 : IFoo2
        {
            public virtual void Bar()
            { }
        }
    }
}
