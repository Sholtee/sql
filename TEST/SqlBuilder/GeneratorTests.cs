/********************************************************************************
* GeneratorTests.cs                                                             *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Moq;
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;
    using Internals;
    using Primitives;

    [TestFixture]
    public sealed class GeneratorTests
    {
        private static void CallActions(Func<ParameterExpression, IEnumerable<MethodCallExpression>> getActions, ISqlQuery mock)
        {
            ParameterExpression bldr = Expression.Parameter(typeof(ISqlQuery));
            Expression.Lambda<Action<ISqlQuery>>(Expression.Block(getActions(bldr)), bldr).Compile()(mock);
        }

        [TearDown]
        public void Teardown()
        {
            Cache.AsDictionary<(Type, Type, int), IReadOnlyList<Edge>>().Clear();
            Cache.AsDictionary<Type, IReadOnlyList<Edge>>().Clear();
            Config.Use<KnownOrmTypes>();
        }

        [Test]
        public void FragmentActionGenerator_ShouldSelect()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Loose);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View3>.Type.GetProperty(nameof(View3.Id)))));

            CallActions(((IActionGenerator) new FragmentActionGenerator<View3>()).Generate, mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void FragmentActionGenerator_ShouldGroup()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Loose);
            mockSqlBuilder
                .Setup(x => x.SelectCount(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View3>.Type.GetProperty(nameof(View3.Count)))));
            mockSqlBuilder
                .Setup(x => x.GroupBy(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))));

            CallActions(((IActionGenerator) new FragmentActionGenerator<View3>()).Generate, mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.SelectCount(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.GroupBy(It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void FragmentActionGenerator_ShouldOrder()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Loose);
            mockSqlBuilder
                .Setup(x => x.OrderBy(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))));

            CallActions(((IActionGenerator) new FragmentActionGenerator<View3>()).Generate, mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.OrderBy(It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void FragmentActionGenerator_ShouldWorkWithComplexViews()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension2>.Type.GetProperty(nameof(Extension2.AnotherId)))));
            mockSqlBuilder
                .Setup(x => x.SelectCount(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension2>.Type.GetProperty(nameof(Extension2.Count)))));
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension2>.Type.GetProperty(nameof(Extension2.Id)))));
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension2>.Type.GetProperty(nameof(Extension2.ReferenceWithoutAttribute)))));
            mockSqlBuilder
                .Setup(x => x.GroupBy(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))));
            mockSqlBuilder
                .Setup(x => x.GroupBy(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))));
            mockSqlBuilder
                .Setup(x => x.GroupBy(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute)))));
            mockSqlBuilder
                .Setup(x => x.OrderBy(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))));

            CallActions(((IActionGenerator) new FragmentActionGenerator<Extension2>()).Generate, mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(3));
            mockSqlBuilder.Verify(x => x.SelectCount(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.GroupBy(It.IsAny<PropertyInfo>()), Times.Exactly(3));
            mockSqlBuilder.Verify(x => x.OrderBy(It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void JoinActionGenerator_ShouldGenerateJoinsInTheAppropriateOrder()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);

            int order = 0;
            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(0)));
            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference2))),
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(1)));
            mockSqlBuilder
                .Setup(x => x.LeftJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node5).GetProperty(nameof(Node5.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(2)));

            Config.Use(new TestOrmTypes(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)));

            CallActions(((IActionGenerator) new JoinActionGenerator<View1, Start_Node>()).Generate, mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.InnerJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(2));
            mockSqlBuilder.Verify(x => x.LeftJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void SmartSqlBuilder_ShouldUseTheGeneratorsInTheAppropriateOrder()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            int order = 0;

            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View1>.Type.GetProperty(nameof(View1.SimpleColumnSelection)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View1>.Type.GetProperty(nameof(View1.IdSelection)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Foo))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View1>.Type.GetProperty(nameof(View1.SelectionFromAlreadyJoinedTable)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Node5).GetProperty(nameof(Node5.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View1>.Type.GetProperty(nameof(View1.SelectionFromAnotherRoute)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Node5).GetProperty(nameof(Node5.Reference))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View1>.Type.GetProperty(nameof(View1.SecondSelectionFromAnotherRoute)))))
                .Callback(() => order++);

            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(0)));
            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference2))),
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(1)));
            mockSqlBuilder
                .Setup(x => x.LeftJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node5).GetProperty(nameof(Node5.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(2)));

            mockSqlBuilder
                .Setup(x => x.Finalize(It.Is<Type>(y => y == typeof(View1))))
                .Callback(() => Assert.That(order++, Is.EqualTo(8)));

            Config.Use(new TestOrmTypes(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)));

            SmartSqlBuilder<View1, Start_Node>.Initialize();
            SmartSqlBuilder<View1, Start_Node>.Build(mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.InnerJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(2));
            mockSqlBuilder.Verify(x => x.LeftJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(5));
            mockSqlBuilder.Verify(x => x.Finalize(It.IsAny<Type>()), Times.Once);
        }

        [Test]
        public void SmartSqlBuilder_ShouldWorkWithComplexViews()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            int order = 0;

            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension1>.Type.GetProperty(nameof(Extension1.Id)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension1>.Type.GetProperty(nameof(Extension1.ReferenceWithoutAttribute)))))
                .Callback(() => order++);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<Extension1>.Type.GetProperty(nameof(Extension1.IdSelection)))))
                .Callback(() => order++);

            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(0)));
            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference2))),
                    It.Is<PropertyInfo>(y => y == typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)))))
                .Callback(() => Assert.That(order++, Is.EqualTo(1)));

            mockSqlBuilder
                .Setup(x => x.Finalize(It.Is<Type>(y => y == typeof(Extension1))))
                .Callback(() => Assert.That(order, Is.EqualTo(5)));

            Config.Use(new TestOrmTypes(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)));

            SmartSqlBuilder<Extension1, Start_Node>.Initialize();
            SmartSqlBuilder<Extension1, Start_Node>.Build(mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.InnerJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(2));
            mockSqlBuilder.Verify(x => x.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Exactly(3));
            mockSqlBuilder.Verify(x => x.Finalize(It.IsAny<Type>()), Times.Once);
        }

        [Test]
        public void SmartSqlBuilder_GenerateBuildAction_GroupBy_Test()
        {
            var mockSqlBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockSqlBuilder
                .Setup(x => x.Select(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View2>.Type.GetProperty(nameof(View2.Foo)))));
            mockSqlBuilder
                .Setup(x => x.SelectCount(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Id))),
                    It.Is<PropertyInfo>(y => y == Unwrapped<View2>.Type.GetProperty(nameof(View2.Count)))));

            mockSqlBuilder
                .Setup(x => x.InnerJoin(
                    It.Is<PropertyInfo>(y => y == typeof(Node2).GetProperty(nameof(Node2.Reference))),
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.Id)))));
            mockSqlBuilder
                .Setup(x => x.GroupBy(
                    It.Is<PropertyInfo>(y => y == typeof(Start_Node).GetProperty(nameof(Start_Node.ReferenceWithoutAttribute)))));
            mockSqlBuilder.Setup(x => x.Finalize(It.Is<Type>(y => y == typeof(View2))));

            Config.Use(new TestOrmTypes(typeof(Start_Node), typeof(Goal_Node), typeof(Node2), typeof(Node4), typeof(Node5), typeof(Node6), typeof(Node7), typeof(Node8)));

            SmartSqlBuilder<View2, Start_Node>.Initialize();
            SmartSqlBuilder<View2, Start_Node>.Build(mockSqlBuilder.Object);

            mockSqlBuilder.Verify(x => x.InnerJoin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.SelectCount(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockSqlBuilder.Verify(x => x.Finalize(It.IsAny<Type>()), Times.Once);
        }
    }
}
