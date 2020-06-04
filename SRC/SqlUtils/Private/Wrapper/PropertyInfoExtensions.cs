/********************************************************************************
* PropertyInfoExtensions.cs                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal static class PropertyInfoExtensions
    {
        public static object FastGetValue(this PropertyInfo src, object instance)
        {
            Func<object, object> getter = Cache.GetOrAdd(src, () =>
            {
                ParameterExpression p = Expression.Parameter(typeof(object), nameof(instance));

                return Expression
                    .Lambda<Func<object, object>>(Expression.Convert(Expression.Property(Expression.Convert(p, src.ReflectedType), src), typeof(object)), p)
                    .Compile();
            });

            return getter.Invoke(instance);
        }

        public static void FastSetValue(this PropertyInfo src, object instance, object? value)
        {
            Action<object, object?> setter = Cache.GetOrAdd(src, () =>
            {
                ParameterExpression 
                    inst = Expression.Parameter(typeof(object), nameof(instance)),
                    val  = Expression.Parameter(typeof(object), nameof(value));

                return Expression
                    .Lambda<Action<object, object?>>(
                        Expression.Assign
                        (
                            Expression.Property(Expression.Convert(inst, src.ReflectedType), src), 
                            Expression.Convert(val, src.PropertyType)
                        ), 
                        inst, 
                        val)
                    .Compile();
            });

            setter.Invoke(instance, value);
        }
    }
}
