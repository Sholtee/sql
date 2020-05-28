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
                Value  = "cica\" -- comment last quote \r\nDROP TABLE Region"
            });
            Assert.That(sql, Is.EqualTo("SELECT * FROM Region WHERE RegionDescription = \"cica\\\" -- comment last quote \\r\\nDROP TABLE Region\""));
        }
    }
}
