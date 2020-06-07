/********************************************************************************
*  QueryMethods.cs                                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal static class QueryMethods
    {
        private static MethodInfo GetQueryMethod(Expression<Action<ISqlQuery>> expr) => ((MethodCallExpression) expr.Body).Method;

        public static readonly MethodInfo
            InnerJoin = GetQueryMethod(bldr => bldr.InnerJoin(null!, null!)),
            LeftJoin  = GetQueryMethod(bldr => bldr.LeftJoin(null!, null!)),
            Finalize  = GetQueryMethod(bldr => bldr.Finalize(null!));
    }
}