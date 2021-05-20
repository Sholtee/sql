/********************************************************************************
* DefaultConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Moq;
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    [TestFixture]
    public class DefaultConfigTests: TestsBase
    {
        private static string Stringify(IDbDataParameter para) => Config.Instance.Stringify(para);

        public static string Format(string sql, params IDbDataParameter[] paramz) => Config.Instance.SqlFormat(sql, paramz);

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

        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, ?)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, @RegionDescription)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES ({0}, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, {1})")]
        public void SqlFormat_ShouldAccept(string fmt)
        {
            var mockConfig = new Mock<DefaultConfig>(MockBehavior.Strict);
            mockConfig
                .Setup(c => c.Stringify(It.IsAny<IDbDataParameter>()))
                .CallBase();
            mockConfig
                .Setup(c => c.SqlFormat(fmt, It.IsAny<IDbDataParameter[]>()))
                .CallBase();

            using (Config.UseTemporarily(mockConfig.Object))
            {
                IDbDataParameter
                    p1 = new SqlParameter { DbType = DbType.Int32, Value = 1, ParameterName = "@RegionID" },
                    p2 = new SqlParameter { DbType = DbType.String, Value = "cica", ParameterName = "RegionDescription" /*direkt nincs @*/ };

                Assert.That(Format(fmt, p1, p2), Is.EqualTo("INSERT INTO Region (RegionID, RegionDescription) VALUES (1, \"cica\")"));

                mockConfig.Verify(c => c.Stringify(p1), Times.Once);
                mockConfig.Verify(c => c.Stringify(p2), Times.Once);
            }
        }

        [Test]
        public void SqlFormat_ShouldEscape()
        {
            string sql = Format("SELECT * FROM Region WHERE RegionDescription = @RegionDescription", new SqlParameter
            {
                DbType = DbType.String,
                ParameterName = "@RegionDescription",
                Value = "cica\";\r\nDROP TABLE Region -- comment last quote"
            });
            Assert.That(sql, Is.EqualTo("SELECT * FROM Region WHERE RegionDescription = \"cica\\\";\\r\\nDROP TABLE Region -- comment last quote\""));
        }

        [Test]
        public void SqlFormat_ShouldThrowOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => Format(null));
            Assert.Throws<ArgumentNullException>(() => Format("", paramz: null));
        }

        [TestCase("INSERT INTO Region (RegionID) VALUES (?)")]
        [TestCase("INSERT INTO Region (RegionID) VALUES ({0})")]
        public void SqlFormat_ShouldValidateTheIndex(string sql) =>
            Assert.Throws<IndexOutOfRangeException>(() => Format(sql));

        [TestCase("INSERT INTO Region (RegionID) VALUES (@InvalidName)")]
        public void SqlFormat_ShouldValidateTheName(string sql) =>
            Assert.Throws<KeyNotFoundException>(() => Format(sql));
    }
}
