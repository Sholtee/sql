/********************************************************************************
* AttributesTests.cs                                                            *
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
    
    [TestFixture]
    public class AttributesTests
    {
        private static Action<ISqlQuery> ConvertToDelegate(Func<ParameterExpression, IEnumerable<MethodCallExpression>> getter) 
        {
            ParameterExpression bldr = Expression.Parameter(typeof(ISqlQuery), nameof(bldr));

            return Expression
                .Lambda<Action<ISqlQuery>>(Expression.Block(getter(bldr)), bldr)
                .Compile();
        }

        [Test]
        public void BelongsTo_ShouldSelect() 
        {
            IFragmentFactory attr = new BelongsToAttribute(typeof(Goal_Node));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Id)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.Select(typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)), typeof(View3).GetProperty(nameof(View3.Id))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void BelongsTo_ShouldGroup() 
        {
            IFragmentFactory attr = new BelongsToAttribute(typeof(Goal_Node));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Id)), true));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.Select(typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)), typeof(View3).GetProperty(nameof(View3.Id))));
            mockBuilder.Setup(q => q.GroupBy(typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockBuilder.Verify(q => q.GroupBy(It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void BelongsTo_ShouldOrder() 
        {
            IFragmentFactory attr = new BelongsToAttribute(typeof(Goal_Node), order: Order.Descending);

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Id)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.Select(typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id)), typeof(View3).GetProperty(nameof(View3.Id))));
            mockBuilder.Setup(q => q.OrderByDescending(typeof(Goal_Node).GetProperty(nameof(Goal_Node.Id))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.Select(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
            mockBuilder.Verify(q => q.OrderByDescending(It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void AverageOf_ShouldSelect()
        {
            IFragmentFactory attr = new AverageOfAttribute(typeof(Node2), column: nameof(Node2.Id));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Count)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.SelectAvg(typeof(Node2).GetProperty(nameof(Node2.Id)), typeof(View3).GetProperty(nameof(View3.Count))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.SelectAvg(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void CountOf_ShouldSelect()
        {
            IFragmentFactory attr = new CountOfAttribute(typeof(Node2), column: nameof(Node2.Id));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Count)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.SelectCount(typeof(Node2).GetProperty(nameof(Node2.Id)), typeof(View3).GetProperty(nameof(View3.Count))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.SelectCount(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void MinOf_ShouldSelect()
        {
            IFragmentFactory attr = new MinOfAttribute(typeof(Node2), column: nameof(Node2.Id));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Count)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.SelectMin(typeof(Node2).GetProperty(nameof(Node2.Id)), typeof(View3).GetProperty(nameof(View3.Count))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.SelectMin(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }

        [Test]
        public void MaxOf_ShouldSelect()
        {
            IFragmentFactory attr = new MaxOfAttribute(typeof(Node2), column: nameof(Node2.Id));

            Action<ISqlQuery> action = ConvertToDelegate(bldr => attr.GetFragments(bldr, typeof(View3).GetProperty(nameof(View3.Count)), false));

            var mockBuilder = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockBuilder.Setup(q => q.SelectMax(typeof(Node2).GetProperty(nameof(Node2.Id)), typeof(View3).GetProperty(nameof(View3.Count))));

            action.Invoke(mockBuilder.Object);

            mockBuilder.Verify(q => q.SelectMax(It.IsAny<PropertyInfo>(), It.IsAny<PropertyInfo>()), Times.Once);
        }
    }
}
