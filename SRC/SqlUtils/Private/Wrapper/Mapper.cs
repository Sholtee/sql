/********************************************************************************
* Mapper.cs                                                                     *
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
    using Properties;

    internal static class Mapper
    {
        public static Func<object?, object?> Create(Type srcType, Type dstType) => Cache.GetOrAdd((srcType, dstType), () =>
        {
            ParameterExpression p = Expression.Parameter(typeof(object));

            BlockExpression block;

            if (srcType.IsValueTypeOrString())
                block = CreateForValueType(null);
            else
            {
                PropertyInfo? propertyToMap = srcType.MapFrom();

                block = propertyToMap == null 
                    ? CreateForClass() 
                    : CreateForValueType(propertyToMap);
            }

            return Expression.Lambda<Func<object?, object?>>(block, p).Compile();

            BlockExpression CreateForValueType(PropertyInfo? property) 
            {
                //
                // TODO: int32 -> int64 pl mukodnie kene
                //

                if (srcType != dstType && property?.PropertyType != dstType)
                    throw MappingNotSupported();

                Expression src = property == null
                    //
                    // (TDst) p
                    //

                    ? (Expression) Expression.Convert(p, dstType)

                    //
                    // (TType p).Prop
                    //

                    : Expression.Property
                    (
                        Expression.Convert(p, property.DeclaringType),
                        property
                    );

                ParameterExpression dst = Expression.Variable(dstType, nameof(dst));

                return Expression.Block
                (
                    variables: new[] { dst },

                    //
                    // TDst dst = ...
                    // return (object) dst; // cast-olas a boxing-hoz kell
                    //

                    Expression.Assign(dst, src),
                    Expression.Convert(dst, typeof(object))
                )!;
            }

            BlockExpression CreateForClass() 
            {
                if (!srcType.IsClass || !dstType.IsClass)
                    throw MappingNotSupported();

                ParameterExpression
                     src = Expression.Variable(srcType, nameof(src)),
                     dst = Expression.Variable(dstType, nameof(dst));

                LabelTarget label = Expression.Label(typeof(object));

                const BindingFlags bindingFlagsBase = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                IReadOnlyList<PropertyInfo>
                    srcProps = srcType.GetProperties(bindingFlagsBase | BindingFlags.GetProperty),
                    dstProps = dstType.GetProperties(bindingFlagsBase | BindingFlags.SetProperty);

                return Expression.Block
                (
                    variables: new[] { src, dst },
                    expressions: new Expression[]
                    {
                        //
                        // if (p == null) return null
                        //

                        Expression.IfThen
                        (
                            Expression.Equal(p, Expression.Default(typeof(object))),
                            Expression.Return(label, Expression.Default(typeof(object)))
                        ),

                        //
                        // TSrc src = (TSrc) p;
                        // TDst dst = new TDst();
                        //

                        Expression.Assign(src, Expression.Convert(p, srcType)),
                        Expression.Assign(dst, Expression.New(dstType))
                    }
                    .Concat
                    (
                        //
                        // dst.Prop_1 = src.Prop_1;
                        // ...
                        // dst.Prop_N = src.Prop_N;
                        //

                        from srcProp in srcProps
                        let dstProp = dstProps.SingleOrDefault(dstProp => srcProp.CanBeMappedIn(dstProp))
                        where dstProp != null
                        select Expression.Assign(Expression.Property(dst, dstProp), Expression.Property(src, srcProp))
                    )
                    .Concat
                    (
                        //
                        // return dst;
                        //

                        new Expression[]
                        {
                            Expression.Return(label, dst),
                            Expression.Label(label, Expression.Default(typeof(object)))
                        }
                    )
                )!;
            }

            NotSupportedException MappingNotSupported()
            {
                var ex = new NotSupportedException(Resources.MAPPING_NOT_SUPPORTED);
                ex.Data["mapping"] = $"{srcType} -> {dstType}";

                return ex!;
            }
        });
    }
}
