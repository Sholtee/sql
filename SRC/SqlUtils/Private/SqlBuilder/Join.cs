/********************************************************************************
*  Join.cs                                                                      *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal static class Join
    {
        private static MethodInfo GetQueryMethod(Expression<Action<ISqlQuery>> expr) => ((MethodCallExpression) expr.Body).Method;

        public static MethodInfo Inner { get; } = GetQueryMethod(bldr => bldr.InnerJoin(null!, null!));
        public static MethodInfo Left  { get; } = GetQueryMethod(bldr => bldr.LeftJoin(null!, null!));
    }
}