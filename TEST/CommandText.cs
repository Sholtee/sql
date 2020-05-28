/********************************************************************************
* CommandText.cs                                                                *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Data;
using System.Data.SqlClient;

using Moq;
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    [TestFixture]
    public class CommandTextTests
    {
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, ?)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, @RegionDescription)")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES ({0}, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, {1})")]
        [TestCase("INSERT INTO Region (RegionID, RegionDescription) VALUES (?, {1})")]
        public void Format_ShouldAccept(string fmt) 
        {
            var @base = new DefaultConfig();

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
    }
}
