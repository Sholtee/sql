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
        public static Expression<TLambda> ToLambda<TLambda>(this MethodInfo method, Func<ParameterInfo, int, Expression> getArgument, ParameterExpression instance, params ParameterExpression[] parameters)
        {
            Expression call = Expression.Call
            (
                Expression.Convert(instance, method.ReflectedType), 
                method, 
                method.GetParameters().Select
                (
                    (param, i) => Expression.Convert
                    (
                        getArgument(param, i),
                        param.ParameterType
                    )
                )
            );

            call = method.ReturnType != typeof(void)
                ? (Expression) Expression.Convert(call, typeof(object))
                : Expression.Block(typeof(object), call, Expression.Default(typeof(object)));

            return Expression.Lambda<TLambda>
            (
                call,
                new[] { instance }.Concat(parameters)
            );
        }

        public static Func<object, object?[], object> ToDelegate(this MethodInfo method) => Cache.GetOrAdd(method, () =>
        {
            ParameterExpression 
                instance = Expression.Parameter(typeof(object), nameof(instance)),
                paramz   = Expression.Parameter(typeof(object[]), nameof(paramz));

            return method.ToLambda<Func<object, object?[], object>>
            (
                (param, i) => Expression.ArrayAccess(paramz, Expression.Constant(i)),
                instance,
                paramz
            ).Compile();
        });

        public static object Call(this MethodInfo method, object instance, params object?[] args) => method.ToDelegate().Invoke(instance, args);
    }
}
