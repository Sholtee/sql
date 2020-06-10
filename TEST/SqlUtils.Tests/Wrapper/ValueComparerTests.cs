/********************************************************************************
* ValueComparerTests.cs                                                         *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Internals;

    [TestFixture]
    public sealed class ValueComparerTests
    {
        private static readonly IEqualityComparer<object> Comparer = ValueComparer.Instance;
        private static new readonly Func<object, object, bool> Equals = Comparer.Equals;

        private static bool CompareByHash(object a, object b) => Comparer.GetHashCode(a) == Comparer.GetHashCode(b);

        [Test]
        public void Equals_ShouldWorkWithNulls()
        {
            Assert.That(Equals(null, null));
            Assert.That(Equals(null, 0), Is.False);
            Assert.That(Equals(null, new {}), Is.False);
        }

        private static void PrimitiveTest(Func<object, object, bool> comparer)
        {
            Assert.That(comparer(0, 0));
            Assert.That(comparer(1, 1));
            Assert.That(comparer("cica", "cica"));

            Assert.That(comparer(0, 1),  Is.False);
            Assert.That(comparer(0, ""), Is.False);
        }

        [Test]
        public void Equals_ShouldWorkWithPrimitives() => PrimitiveTest(Equals);

        [Test]
        public void GetHashCode_ShouldWorkWithPrimitives() => PrimitiveTest(CompareByHash);

        private static void EnumerableTest(Func<object, object, bool> comparer)
        {
            Assert.That(comparer(new int[] { }, new int[] { }));
            Assert.False(comparer(new string[] { }, new int[] { }));
            Assert.That(comparer(new int[] { 1 }, new int[] { 1 }));
            Assert.False(comparer(new int[] { 1 }, new int[] { 2 }));
            Assert.False(comparer(new object[] { new object() }, new object[] { new object() }));
        }

        [Test]
        public void Equals_ShouldWorkWithEnumerables() => EnumerableTest(Equals);

        [Test]
        public void GetHashCode_ShouldWorkWithEnumerables() => EnumerableTest(CompareByHash);

        private class MyClass
        {
            public string Cica { get; set; }
        }

        private class MyClassA : MyClass
        {
            public int Foo { get; set; }
        }

        private class MyClassB : MyClass
        {
            public int Bar { get; set; }
        }

        private static void ObjectTest(Func<object, object, bool> comparer)
        {
            Assert.That(comparer(new { }, new { }));
            Assert.That(comparer(new MyClass(), new MyClass()));

            Assert.That(comparer(new { Cica = "Kutya" }, new { Cica = "Kutya" }));
            Assert.That(comparer(new MyClass{ Cica = "Kutya" }, new MyClass { Cica = "Kutya" }));
            Assert.That(comparer(new MyClass { Cica = "Kutya" }, new { Cica = "Kutya" }), Is.False);
            Assert.That(comparer(new { Cica = "Kutya" }, new MyClass { Cica = "Kutya" }), Is.False);

            Assert.That(comparer(new { Cica = "Kutya" }, new { Cica = "Cica" }),  Is.False);
            Assert.That(comparer(new MyClass { Cica = "Kutya" }, new MyClass { Cica = "Cica" }), Is.False);
            Assert.That(comparer(new MyClass { Cica = "Kutya" }, new { Cica = "Cica" }), Is.False);

            Assert.That(comparer(new MyClass { Cica = "Kutya" }, new { Cica = "Kutya", Foo = 1 }), Is.False);
            Assert.That(comparer(new MyClass { Cica = "Kutya" }, new MyClassA{ Cica = "Kutya", Foo = 1 }), Is.False);
            Assert.That(comparer(new MyClassA { Cica = "Kutya" }, new MyClassA { Cica = "Kutya", Foo = 1 }), Is.False);

            Assert.That(comparer(new { Cica = "Kutya", Bar = 1 }, new { Cica = "Kutya", Foo = 1 }), Is.False);
            Assert.That(comparer(new MyClassB{ Cica = "Kutya", Bar = 1 }, new MyClassA{ Cica = "Kutya", Foo = 1 }), Is.False);
        }

        [Test]
        public void Equals_ShouldWorkWithObjects() => ObjectTest(Equals);

        [Test]
        public void GetHashCod_ShouldWorkWithObjects() => ObjectTest(CompareByHash);
    }
}
