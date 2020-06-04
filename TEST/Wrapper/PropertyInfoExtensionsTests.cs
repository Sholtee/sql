/********************************************************************************
* PropertyInfoExtensionsTests.cs                                                *
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
    public sealed class PropertyInfoExtensionsTests
    {
        [Test]
        public void FastGetValue_ShouldReturnThePropertyValue()
        {
            var ar = new[] {""};
            Assert.That(typeof(string[]).GetProperty(nameof(Array.Length)).FastGetValue(ar), Is.EqualTo(1));
        }

        [Test]
        public void FastSetValue_ShouldSetThePropertyValue()
        {
            var lst = new List<string>();
            Assert.That(lst.Capacity, Is.EqualTo(0));

            typeof(List<string>).GetProperty(nameof(List<string>.Capacity)).FastSetValue(lst, 1);
            Assert.That(lst.Capacity, Is.EqualTo(1));
        }

        [Test]
        public void FastSetValue_ShouldThrowOnInvalidValue()
        {
            var lst = new List<string>();
            Assert.Throws<InvalidCastException>(() => typeof(List<string>).GetProperty(nameof(List<string>.Capacity)).FastSetValue(lst, string.Empty));
        }
    }
}
