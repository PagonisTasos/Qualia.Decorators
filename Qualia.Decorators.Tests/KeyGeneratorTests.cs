using Qualia.Decorators.Utils;

namespace Qualia.Decorators.Tests
{
    [TestFixture]
    public class KeyGeneratorTests
    {
        [Test]
        public void CreateKey_GeneratesConsistentKey_ForSameMethodAndArgs()
        {
            // Arrange
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            var args = new object[] { 1, 2 };

            // Act
            var key1 = KeyGenerator.CreateKey(methodInfo, args);
            var key2 = KeyGenerator.CreateKey(methodInfo, args);

            // Assert
            Assert.That(key2, Is.EqualTo(key1));
        }

        [Test]
        public void CreateKey_GeneratesDifferentKeys_ForDifferentMethods()
        {
            // Arrange
            var methodInfo1 = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            var methodInfo2 = typeof(SampleClass).GetMethod(nameof(SampleClass.AnotherMethod));
            var args = new object[] { 1, 2 };

            // Act
            var key1 = KeyGenerator.CreateKey(methodInfo1, args);
            var key2 = KeyGenerator.CreateKey(methodInfo2, args);

            // Assert
            Assert.That(key2, Is.Not.EqualTo(key1));
        }

        [Test]
        public void CreateKey_GeneratesDifferentKeys_ForDifferentArgs()
        {
            // Arrange
            var methodInfo = typeof(SampleClass).GetMethod(nameof(SampleClass.SampleMethod));
            var args1 = new object[] { 1, 2 };
            var args2 = new object[] { 2, 3 };

            // Act
            var key1 = KeyGenerator.CreateKey(methodInfo, args1);
            var key2 = KeyGenerator.CreateKey(methodInfo, args2);

            // Assert
            Assert.That(key2, Is.Not.EqualTo(key1));
        }

        private class SampleClass
        {
            public int SampleMethod(int a, int b) => a + b;

            public int AnotherMethod(int a, int b) => a * b;
        }
    }
}