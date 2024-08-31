using Moq;
using Microsoft.Extensions.Logging;
using Qualia.Decorators.Framework;


namespace Qualia.Decorators.Tests
{

    [TestFixture]
    public class MemoizeTests
    {
        private Memoize _memoize;
        private Mock<ILogger<Memoize>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<Memoize>>();
            _memoize = new Memoize(_mockLogger.Object);
        }

        [Test]
        public void Invoke_CachesResult_AfterFirstCall()
        {
            // Arrange
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            var context = new DecoratorContext<SampleClass>(null, new SampleClass(), methodInfo, new object[] { 1, 2 });

            // Act
            var result1 = _memoize.Invoke(context);
            var result2 = _memoize.Invoke(context);

            // Assert
            Assert.That(result2, Is.EqualTo(result1));
        }

        [Test]
        public void Invoke_DoesNotCacheDifferentMethod()
        {
            // Arrange
            var methodInfo1 = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            var methodInfo2 = typeof(SampleClass).GetMethod(nameof(SampleClass.AnotherMethod));

            var context1 = new DecoratorContext<SampleClass>(null, new SampleClass(), methodInfo1, new object[] { 1, 2 });
            var context2 = new DecoratorContext<SampleClass>(null, new SampleClass(), methodInfo2, new object[] { 1, 2 });

            // Act
            var result1 = _memoize.Invoke(context1);
            var result2 = _memoize.Invoke(context2);

            // Assert
            Assert.That(result2, Is.Not.EqualTo(result1));
        }

        [Test]
        public void Invoke_CachesWithDifferentArguments()
        {
            // Arrange
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));

            var context1 = new DecoratorContext<SampleClass>(null, new SampleClass(), methodInfo, new object[] { 1, 2 });
            var context2 = new DecoratorContext<SampleClass>(null, new SampleClass(), methodInfo, new object[] { 2, 3 });

            // Act
            var result1 = _memoize.Invoke(context1);
            var result2 = _memoize.Invoke(context2);

            // Assert
            Assert.That(result2, Is.Not.EqualTo(result1));
        }

        private class SampleClass
        {
            public int SampleMethod(int a, int b) => a + b;

            public int AnotherMethod(int a, int b) => a * b;
        }
    }

}
