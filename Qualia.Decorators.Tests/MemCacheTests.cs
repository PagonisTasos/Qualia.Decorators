using Moq;
using Qualia.Decorators.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Qualia.Decorators.Tests
{
    [TestFixture]
    public class MemCacheTests
    {
        private Mock<ILogger<MemCache>> _loggerMock;
        private IMemoryCache _memoryCache;
        private Mock<Foo> _fooMock;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            // Mock ILogger<MemCache>
            _loggerMock = new Mock<ILogger<MemCache>>();

            // Initialize in-memory cache
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Mock IFoo
            _fooMock = new Mock<Foo>();

            // Set up the Bar method to count invocations
            _fooMock.Setup(f => f.Bar()).Verifiable();

            // Set up DI container
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(_memoryCache);
            services.AddSingleton(_loggerMock.Object);
            services.AddScoped<IFoo, Foo>(_ => _fooMock.Object);
            services.UseDecorators();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Bar_Method_Should_Be_Cached()
        {
            // Arrange
            var foo = _serviceProvider.GetRequiredService<IFoo>();

            // Act
            foo.Bar(); // First call, should cache the result
            foo.Bar(); // Second call, should retrieve from cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to caching.");
        }

        [Test]
        public void MemCache_Should_Use_AbsoluteExpiration()
        {
            // Arrange
            var foo = _serviceProvider.GetRequiredService<IFoo>();
            var cacheEntryOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) };
            _memoryCache.Set("TestKey", "TestValue", cacheEntryOptions);

            // Act
            foo.Bar(); // First call, should cache the result
            foo.Bar(); // Second call, should retrieve from cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to caching.");
            Assert.That(_memoryCache.Get("TestKey"), Is.EqualTo("TestValue"), "Cache should contain the expected value with absolute expiration.");
        }

        [Test]
        public void MemCache_Should_Use_SlidingExpiration()
        {
            // Arrange
            var foo = _serviceProvider.GetRequiredService<IFoo>();
            var cacheEntryOptions = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(10) };
            _memoryCache.Set("TestKey", "TestValue", cacheEntryOptions);

            // Act
            foo.Bar(); // First call, should cache the result
            foo.Bar(); // Second call, should retrieve from cache

            // Assert
            _fooMock.Verify(f => f.Bar(), Times.Once, "Bar method should only be called once due to caching.");
            Assert.That(_memoryCache.Get("TestKey"), Is.EqualTo("TestValue"), "Cache should contain the expected value with sliding expiration.");
        }

        [Test]
        public void MemCache_Should_Throw_On_Invalid_ExpirationType()
        {
            // Arrange
            var context = new DecoratorContext<IFoo>(new MemCacheAttribute("TestName", expiration: (MemCacheAttribute.ExpirationType)999), _fooMock.Object, typeof(IFoo).GetMethod("Bar"), null);
            var memCache = new MemCache(_loggerMock.Object, _memoryCache);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => memCache.Invoke(context), "Invalid expiration type should throw an exception.");
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the service provider to release any resources
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _memoryCache.Dispose();
        }

        public interface IFoo
        {
            void Bar();
        }

        [MemCache]
        public class Foo : IFoo
        {
            public virtual void Bar()
            {
                // Your method implementation
            }
        }
    }
}
