/********************************************************************************
* ISqlQueryExtensionsTests.cs                                                   *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

using Moq;
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;
    using Internals;   

    [TestFixture]
    public sealed class ISqlQueryExtensionsTests
    {
        [Test]
        public void RunTest()
        {
            Type unwrapped = Unwrapped<WrappedView1>.Type;

            MethodInfo run = typeof(ISqlQuery).GetMethod(nameof(ISqlQuery.Run));

            ParameterExpression param = Expression.Parameter(typeof(ISqlQuery));

            Expression<Func<ISqlQuery, IList>> expr = Expression.Lambda<Func<ISqlQuery, IList>>(Expression.Call(param, run, Expression.Constant(unwrapped)), param);

            var mockSqlQuery = new Mock<ISqlQuery>(MockBehavior.Strict);
            mockSqlQuery
                .Setup(expr)
                .Returns((IList) null);

            ISqlQuery sqlQuery = mockSqlQuery.Object;
            sqlQuery.Run<WrappedView1>();

            mockSqlQuery.Verify(expr, Times.Once);
        }
    }
}
