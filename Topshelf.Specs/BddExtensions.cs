namespace Topshelf.Specs
{
    using System.Collections.Generic;
    using MbUnit.Framework;

    public static class BddExtensions
    {
        public static void ShouldContain<T>(this IEnumerable<T> list, T value)
        {
                Assert.Contains(list, value);
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldNotBeNull<T>(this T actual)
        {
            Assert.IsNotNull(actual);
        }

        public static void ShouldBeTrue(this bool actual)
        {
            Assert.IsTrue(actual);
        }

        public static void ShouldBeTrue(this bool actual, string message)
        {
            Assert.IsTrue(actual, message);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            Assert.IsFalse(actual);
        }

        public static void ShouldBeFalse(this bool actual, string message)
        {
            Assert.IsFalse(actual, message);
        }
    }
}