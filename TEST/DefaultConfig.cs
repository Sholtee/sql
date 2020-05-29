/********************************************************************************
* DefaultConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Data.SqlClient;

using Moq;
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    [TestFixture]
    public class DefaultConfigTests
    {
        private IConfig FPrevious;

        [OneTimeSetUp]
        public void Setup()
        {
            FPrevious = Config.Instance;
            Config.Use<DefaultConfig>();
        }

        [OneTimeTearDown]
        public void Teardown() => Config.Use(FPrevious);

        private static string Stringify(IDataParameter para) => Config.Instance.Stringify(para);

        [Test]
        public void Stringify_ShouldHandleNulls() =>
            Assert.That(Stringify(new SqlParameter { Value = null }), Is.EqualTo("NULL"));

        [Test]
        public void Stringify_ShouldQuoteStrings() =>
            Assert.That(Stringify(new SqlParameter { Value = "cica" }), Is.EqualTo("\"cica\""));

        [Test]
        public void Stringify_ShouldThrowOnNull() => Assert.Throws<ArgumentNullException>(() => Stringify(null));

        [Test]
        public void Stringify_ShouldCallToStringOnNonStringValue() 
        {
            var mockObject = new Mock<object>(MockBehavior.Strict);
            mockObject
                .Setup(o => o.ToString())
                .Returns(string.Empty);

            Stringify(new SqlParameter { Value = mockObject.Object });

            mockObject.Verify(o => o.ToString(), Times.Once);
        }

        [TestCase(@"""cica""", @"""\""cica\""""")]
        [TestCase("'cica'", @"""\'cica\'""")]
        [TestCase("\r\n", @"""\r\n""")]
        [TestCase("\\", @"""\\""")]
        public void Stringify_ShouldEscape(string s, string expected) =>
            Assert.That(Stringify(new SqlParameter { Value = s }), Is.EqualTo(expected)); 
    }
}
