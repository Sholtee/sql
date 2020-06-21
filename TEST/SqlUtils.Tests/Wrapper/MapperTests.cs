/********************************************************************************
* MapperTests.cs                                                                *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Internals;
    using Properties;

    [TestFixture]
    public sealed class MapperTests
    {
        private sealed class A
        {
            public int Foo { get; set; }
            public string Bar { get; set; }
        }

        private sealed class B
        {
            public int Foo { get; set; }
            public object Bar { get; set; }
        }

        private sealed class C
        {
            public int Foo { get; set; }
        }

        [MapFrom(nameof(Foo))]
        private sealed class D 
        {
            public string Foo { get; set; }
        }

        private static object Map(Type srcType, Type dstType, object src) => Mapper.Create(srcType, dstType).Invoke(src);

        [Test]
        public void Mapper_ShouldMapNullValues() =>
            Assert.That(Map(typeof(A), typeof(B), null), Is.Null);

        [Test]
        public void Mapper_ShouldMapValueTypes() =>
            Assert.That(Map(typeof(string), typeof(string), "cica"), Is.EqualTo("cica"));

        [Test]
        public void Mapper_ShouldThrowIfMappingNotSupported() =>
            Assert.Throws<NotSupportedException>(() => Map(typeof(int), typeof(long), null), Resources.MAPPING_NOT_SUPPORTED);

        [Test]
        public void Mapper_ShouldMapObjects()
        {
            A a = new A { Foo = 1, Bar = "cica" };
            B b = (B) Map(typeof(A), typeof(B), a);

            Assert.That(b.Bar, Is.EqualTo(a.Bar));
            Assert.That(b.Foo, Is.EqualTo(a.Foo));

            C c = (C) Map(typeof(A), typeof(C), a);
            Assert.That(c.Foo, Is.EqualTo(a.Foo));

            b = (B) Map(typeof(C), typeof(B), c);

            Assert.That(b.Bar, Is.Null);
            Assert.That(b.Foo, Is.EqualTo(a.Foo));
        }

        [Test]
        public void Mapper_ShouldMapPropertyToValueType() =>
            Assert.That(Map(typeof(D), typeof(string), new D { Foo = "cica" }), Is.EqualTo("cica"));
    }
}
