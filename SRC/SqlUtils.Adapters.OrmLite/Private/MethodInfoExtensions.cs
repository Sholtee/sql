/********************************************************************************
* MethodInfoExtensions.cs                                                       *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal static class MethodInfoExtensions
    {
        public static Expression<TLambda> ToLambda<TLambda>(this MethodInfo method, Func<ParameterInfo, int, Expression> getArgument, params ParameterExpression[] parameters)
        {
            Expression call = Expression.Call(method, method.GetParameters().Select((param, i) => Expression.Convert
            (
                getArgument(param, i),
                param.ParameterType
            )));

            call = method.ReturnType != typeof(void)
                ? (Expression) Expression.Convert(call, typeof(object))
                : Expression.Block(typeof(object), call, Expression.Default(typeof(object)));

            return Expression.Lambda<TLambda>
            (
                call,
                parameters
            );
        }

        public static Func<object?[], object> ToDelegate(this MethodInfo method) => Cache.GetOrAdd(method, () =>
        {
            ParameterExpression paramz = Expression.Parameter(typeof(object[]), nameof(paramz));

            return method.ToLambda<Func<object?[], object>>
            (
                (param, i) => Expression.ArrayAccess(paramz, Expression.Constant(i)),
                paramz
            ).Compile();
        });

        public static object Call(this MethodInfo method, params object?[] args) => method.ToDelegate().Invoke(args);
    }
}
