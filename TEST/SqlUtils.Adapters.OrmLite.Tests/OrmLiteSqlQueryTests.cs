/********************************************************************************
* OrmLiteSqlQueryTests.cs                                                       *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NUnit.Framework;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.OrmLite.Tests
{
    using Interfaces;
    using Interfaces.DataAnnotations;
    using Internals;

    [TestFixture]
    public sealed class OrmLiteSqlQueryTests
    {
        private static readonly IDbConnectionFactory ConnectionFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);

        private IDbConnection FConnection;

        [DatabaseEntity]
        private class OrmType
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public int Order { get; set; }
        }

        [DatabaseEntity]
        private class OrmTypeWithReference
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public Guid Reference { get; set; }
        }

        private sealed class MyView
        {
            [BelongsTo(typeof(OrmType))]
            public Guid Azonosito { get; set; }
        }

        private sealed class CountView
        {
            public int Count { get; set; }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Config.Use<OrmLiteConfig>();
            FConnection = ConnectionFactory.OpenDbConnection();
        }

        [SetUp]
        public void Setup()
        {
            FConnection.CreateTableIfNotExists<OrmTypeWithReference>();
            FConnection.CreateTableIfNotExists<OrmType>();
        }

        [TearDown]
        public void TearDown()
        {
            FConnection.DropTables(typeof(OrmType), typeof(OrmTypeWithReference));
        }

        [Test]
        public void SelectTest()
        {
            Guid id = Guid.NewGuid();

            FConnection.Insert(new OrmType {Id = id});

            SqlExpression<OrmType> expression = FConnection.From<OrmType>();

            ISqlQuery query = new OrmLiteSqlQuery(expression);
            query.Select(typeof(OrmType).GetProperty(nameof(OrmType.Id)), typeof(MyView).GetProperty(nameof(MyView.Azonosito)));

            List<MyView> result = query.Run<MyView>(FConnection);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().Azonosito, Is.EqualTo(id));
        }

        [Test]
        public void JoinTest()
        {
            Guid id = Guid.NewGuid();

            using (IBulkedDbConnection bulk = FConnection.CreateBulkedDbConnection())
            {
                bulk.Insert(new OrmType { Id = id });
                bulk.Insert(new OrmTypeWithReference { Id = Guid.NewGuid(), Reference = id });

                bulk.Flush();
            }

            SqlExpression<OrmType> expression = FConnection.From<OrmType>();

            ISqlQuery query = new OrmLiteSqlQuery(expression);
            query.Select(typeof(OrmType).GetProperty(nameof(OrmType.Id)), typeof(MyView).GetProperty(nameof(MyView.Azonosito)));
            query.InnerJoin(typeof(OrmType).GetProperty(nameof(OrmType.Id)), typeof(OrmTypeWithReference).GetProperty(nameof(OrmTypeWithReference.Reference)));

            List<MyView> result = query.Run<MyView>(FConnection);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().Azonosito, Is.EqualTo(id));
        }

        [Test]
        public void AggregateTest()
        {
            Guid reference = Guid.NewGuid();

            using (IBulkedDbConnection bulk = FConnection.CreateBulkedDbConnection())
            {
                bulk.Insert(new OrmType { Id = reference });

                bulk.InsertAll(new[]
                {
                    new OrmTypeWithReference {Id = Guid.NewGuid(), Reference = reference},
                    new OrmTypeWithReference {Id = Guid.NewGuid(), Reference = reference}
                });

                bulk.Flush();
            }

            SqlExpression<OrmTypeWithReference> expression = FConnection.From<OrmTypeWithReference>();

            ISqlQuery query = new OrmLiteSqlQuery(expression);
            query.GroupBy(typeof(OrmTypeWithReference).GetProperty(nameof(OrmTypeWithReference.Reference)));
            query.SelectCount(typeof(OrmTypeWithReference).GetProperty(nameof(OrmTypeWithReference.Id)), typeof(CountView).GetProperty(nameof(CountView.Count)));

            List<CountView> result = query.Run<CountView>(FConnection);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().Count, Is.EqualTo(2));
        }

        [Test]
        public void OrderByTest()
        {
            using (IBulkedDbConnection bulk = FConnection.CreateBulkedDbConnection())
            {
                bulk.Insert(new OrmType { Id = Guid.NewGuid(), Order = 1 }, new OrmType { Id = Guid.NewGuid(), Order = 2 });

                bulk.Flush();
            }

            SqlExpression<OrmType> expression = FConnection.From<OrmType>();
            expression.PrefixFieldWithTableName = true;

            ISqlQuery query = new OrmLiteSqlQuery(expression);
            query.Select(typeof(OrmType).GetProperty(nameof(OrmType.Order)), typeof(OrmType).GetProperty(nameof(OrmType.Order)));
            query.OrderBy(typeof(OrmType).GetProperty(nameof(OrmType.Order)));
            query.OrderByDescending(typeof(OrmType).GetProperty(nameof(OrmType.Id)));

            List<OrmType> result = query.Run<OrmType>(FConnection);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Order, Is.EqualTo(1));
            Assert.That(result[1].Order, Is.EqualTo(2));
        }
    }
}
