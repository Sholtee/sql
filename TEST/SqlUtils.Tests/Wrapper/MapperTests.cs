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

        private IMapper Mapper { get; set; }

        [SetUp]
        public void Setup()
        {
            Mapper = new Mapper();
            Mapper.RegisterMapping(typeof(A), typeof(B));
            Mapper.RegisterMapping(typeof(A), typeof(C));
            Mapper.RegisterMapping(typeof(C), typeof(B));
        }

        [Test]
        public void Mapper_ShouldMapNullValues() =>
            Assert.That(Mapper.MapTo(typeof(A), typeof(B), null), Is.Null);

        [Test]
        public void Mapper_ShouldNotMapPrimitiveValues() =>
            Assert.Throws<NotSupportedException>(() => Mapper.RegisterMapping(typeof(string), typeof(string)), Resources.MAPPING_NOT_SUPPORTED);

        [Test]
        public void Mapper_ShouldNotMapEnumerableValues() =>
            Assert.Throws<NotSupportedException>(() => Mapper.RegisterMapping(typeof(int[]), typeof(long[])), Resources.MAPPING_NOT_SUPPORTED);

        [Test]
        public void Mapper_ShouldMapObjects()
        {
            A a = new A { Foo = 1, Bar = "cica" };
            B b = (B) Mapper.MapTo(typeof(A), typeof(B), a);

            Assert.That(b.Bar, Is.EqualTo(a.Bar));
            Assert.That(b.Foo, Is.EqualTo(a.Foo));

            C c = (C) Mapper.MapTo(typeof(A), typeof(C), a);
            Assert.That(c.Foo, Is.EqualTo(a.Foo));

            b = (B) Mapper.MapTo(typeof(C), typeof(B), c);

            Assert.That(b.Bar, Is.Null);
            Assert.That(b.Foo, Is.EqualTo(a.Foo));
        }
    }
}
