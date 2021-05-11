/********************************************************************************
* EdgeOperationsTests.cs                                                        *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;
    using Internals;
    using Primitives;
    using Properties;

    [TestFixture]
    public sealed class EdgeOperationsTests
    {
        [TearDown]
        public void Teardown()
        {
            Cache.Clear<int, IReadOnlyList<Edge>>();
            Cache.Clear<Type, IReadOnlyList<Edge>>();
            Config.Use(new SpecifiedDataTables());
        }

        [Test]
        public void Edge_ShouldBeDefinedByPropertyOrExpression()
        {
            Edge[] edges =
            {
                new Edge(typeof(OrmType).GetProperty(nameof(OrmType.Id)), typeof(OrmTypeWithReference).GetProperty(nameof(OrmTypeWithReference.Reference))),
                Edge.Create<OrmType, OrmTypeWithReference>(x => x.Id, y => y.Reference)
            };

            foreach (Edge a in edges)
                foreach (Edge b in edges)
                    Assert.That(a.Equals(b));
        }

        [Test]
        public void GetEdgesFrom_ShouldReturnAnEmptyListIfThereIsNoEdgeFound()
        {
            Config.Use(new SpecifiedDataTables(typeof(OrmType), typeof(OrmTypeWithReference)));

            IReadOnlyList<Edge> edges = EdgeOperations.GetEdgesFrom(typeof(OrmType));
            Assert.That(edges, Is.Empty);
        }

        [Test]
        public void GetEdgesFrom_ShouldReturnTheCorrectEdge()
        {
            Config.Use(new SpecifiedDataTables(typeof(OrmType), typeof(OrmTypeWithReference)));

            IReadOnlyList<Edge> edges = EdgeOperations.GetEdgesFrom(typeof(OrmTypeWithReference));
            Assert.That(edges.Count, Is.EqualTo(1));

            Edge edge = edges.Single();
            Assert.That(edge.SourceTable, Is.EqualTo(typeof(OrmTypeWithReference)));
            Assert.That(edge.DestinationTable, Is.EqualTo(typeof(OrmType)));
        }


        [Test]
        public void GetEdgesTo_ShouldReturnAnEmptyListIfThereIsNoEdgeFound()
        {
            Config.Use(new SpecifiedDataTables(typeof(OrmType), typeof(OrmTypeWithReference)));

            IReadOnlyList<Edge> edges = EdgeOperations.GetEdgesTo(typeof(OrmTypeWithReference));
            Assert.That(edges, Is.Empty);
        }

        [Test]
        public void GetEdgesTo_ShouldReturnTheCorrectEdge()
        {
            Config.Use(new SpecifiedDataTables(typeof(OrmType), typeof(OrmTypeWithReference)));

            IReadOnlyList<Edge> edges = EdgeOperations.GetEdgesTo(typeof(OrmType));
            Assert.That(edges.Count, Is.EqualTo(1));

            Edge edge = edges.First();
            Assert.That(edge.SourceTable, Is.EqualTo(typeof(OrmTypeWithReference)));
            Assert.That(edge.DestinationTable, Is.EqualTo(typeof(OrmType)));
        }

        public static IEnumerable<(IKnownDataTables Tables, (Type Src, Type dst)[] Path)> ShortestPaths 
        {
            get 
            {
                yield return
                (
                    new SpecifiedDataTables(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)),
                    new (Type Src, Type dst)[] { (typeof(Node2), typeof(Start_Node)), (typeof(Node2), typeof(Goal_Node)) }
                );

                yield return
                (
                    new SpecifiedDataTables(typeof(Start_Node), typeof(Goal_Node), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)),
                    new (Type Src, Type dst)[] { (typeof(Node7), typeof(Start_Node)), (typeof(Node6), typeof(Node7)), (typeof(Node6), typeof(Goal_Node)) }
                );
            }
        }

        [TestCaseSource(nameof(ShortestPaths))]
        public void ShortestPath_ShouldFindTheShortestPath((IKnownDataTables Tables, (Type Src, Type Dst)[] Path) ctx)
        {
            Config.Use(ctx.Tables);
            IReadOnlyList<Edge> path = EdgeOperations.ShortestPath(typeof(Start_Node), typeof(Goal_Node));

            Assert.That(path, Is.Not.Null);
            Assert.That(path.Count, Is.EqualTo(ctx.Path.Length));

            path.ForEach((x, i) => 
            {
                Assert.That(x.SourceTable, Is.EqualTo(ctx.Path[i].Src));
                Assert.That(x.DestinationTable, Is.EqualTo(ctx.Path[i].Dst));
            });
        }

        [Test]
        public void ShortestPath_ShouldThrowIfThePathCanNotBeDetermined()
        {
            Config.Use(new SpecifiedDataTables(typeof(Start_Node), typeof(Goal_Node), typeof(Node4), typeof(Node5), typeof(Node7), typeof(Node8)));

            Assert.Throws<InvalidOperationException>(() => EdgeOperations.ShortestPath(typeof(Start_Node), typeof(Goal_Node)), Resources.NO_SHORTEST_PATH);
        }

        [Test]
        public void ShortestPath_ShouldTakeCustomEdgesIntoAccount()
        {
            Config.Use(new SpecifiedDataTables(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)));

            IReadOnlyList<Edge> path = EdgeOperations.ShortestPath(typeof(Start_Node), typeof(Goal_Node), Edge.Create<Goal_Node, Start_Node>(x => x.Id, x => x.ReferenceWithoutAttribute));
            Assert.That(path, Is.Not.Null);
            Assert.That(path.Count, Is.EqualTo(1));
            Assert.That(path[0].SourceTable, Is.EqualTo(typeof(Goal_Node)));
            Assert.That(path[0].DestinationTable, Is.EqualTo(typeof(Start_Node)));
        }
    }
}
