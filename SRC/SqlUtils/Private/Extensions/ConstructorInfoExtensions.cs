/********************************************************************************
* ConstructorInfoExtensions.cs                                                  *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal static class ConstructorInfoExtensions
    {
        public static Expression<TLambda> ToLambda<TLambda>(this ConstructorInfo ctor, Func<ParameterInfo, int, Expression> getArgument, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<TLambda>
            (
                Expression.Convert(Expression.New(ctor, GetArguments()), typeof(object)),
                parameters
            );

            IEnumerable<UnaryExpression> GetArguments() => ctor.GetParameters().Select((param, i) => Expression.Convert
            (
                getArgument(param, i),
                param.ParameterType
            ));
        }

        public static Func<object?[], object> ToDelegate(this ConstructorInfo ctor) => Cache.GetOrAdd(ctor, () =>
        {
            ParameterExpression paramz = Expression.Parameter(typeof(object[]), nameof(paramz));

            return ctor.ToLambda<Func<object?[], object>>
            (
                (param, i) => Expression.ArrayAccess(paramz, Expression.Constant(i)),
                paramz
            ).Compile();
        });
    }
}
