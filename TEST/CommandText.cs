﻿/********************************************************************************
* CommandText.cs                                                                *
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
    public class CommandTextTests: TestsBase
    {
        private static string Stringify(IDataParameter para) => Config.Instance.Stringify(para);

        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, ?)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, @RegionDescription)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES ({0}, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, {1})")]
        public void Format_ShouldAccept(string fmt)
        {
            var mockConfig = new Mock<DefaultConfig>(MockBehavior.Strict);
            mockConfig
                .Setup(c => c.Stringify(It.IsAny<IDataParameter>()))
                .CallBase();

            using (Config.UseTemporarily(mockConfig.Object))
            {
                IDataParameter
                    p1 = new SqlParameter { DbType = DbType.Int32, Value = 1, ParameterName = "@RegionID" },
                    p2 = new SqlParameter { DbType = DbType.String, Value = "cica", ParameterName = "RegionDescription" /*direkt nincs @*/ };

                Assert.That(CommandText.Format(fmt, p1, p2), Is.EqualTo("INSERT INTO Region (RegionID, RegionDescription) VALUES (1, \"cica\")"));

                mockConfig.Verify(c => c.Stringify(p1), Times.Once);
                mockConfig.Verify(c => c.Stringify(p2), Times.Once);
            }
        }

        [Test]
        public void Format_ShouldEscape()
        {
            string sql = CommandText.Format("SELECT * FROM Region WHERE RegionDescription = @RegionDescription", new SqlParameter
            {
                DbType = DbType.String,
                ParameterName = "@RegionDescription",
                Value = "cica\";\r\nDROP TABLE Region -- comment last quote"
            });
            Assert.That(sql, Is.EqualTo("SELECT * FROM Region WHERE RegionDescription = \"cica\\\";\\r\\nDROP TABLE Region -- comment last quote\""));
        }

        [Test]
        public void Format_ShouldThrowOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => CommandText.Format(null));
            Assert.Throws<ArgumentNullException>(() => CommandText.Format("", paramz: null));
        }

        [TestCase("INSERT INTO Region (RegionID) VALUES (?)")]
        [TestCase("INSERT INTO Region (RegionID) VALUES ({0})")]
        public void Format_ShouldValidateTheIndex(string sql) =>
            Assert.Throws<IndexOutOfRangeException>(() => CommandText.Format(sql));

        [TestCase("INSERT INTO Region (RegionID) VALUES (@InvalidName)")]
        public void Format_ShouldValidateTheName(string sql) =>
            Assert.Throws<KeyNotFoundException>(() => CommandText.Format(sql));
    }
}
