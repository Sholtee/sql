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

    internal sealed class Mapper: IMapper
    {
        void IMapper.RegisterMapping(Type srcType, Type dstType) => Cache.GetOrAdd((srcType, dstType), () =>
        {
            if (srcType.IsValueTypeOrString() != dstType.IsValueTypeOrString())
                throw MappingNotSupported(srcType, dstType);

            ParameterExpression p = Expression.Parameter(typeof(object));

            Expression block;

            if (srcType.IsValueTypeOrString())
            {
                //
                // TODO: int32 -> int64 pl mukodnie kene
                //

                if (srcType != dstType)
                    throw MappingNotSupported(srcType, dstType);

                ParameterExpression dst = Expression.Variable(dstType, nameof(dst));

                block = Expression.Block
                (
                    variables: new[] { dst },
                    
                    //
                    // TDst dst = (TDst) p;
                    // return (object) dst; // cast-olas a boxing-hoz kell
                    //

                    Expression.Assign(dst, Expression.Convert(p, dstType)),
                    Expression.Convert(dst, typeof(object))
                );
            }
            else 
            { 
                ParameterExpression
                    src = Expression.Variable(srcType, nameof(src)),
                    dst = Expression.Variable(dstType, nameof(dst));

                const BindingFlags bindingFlagsBase = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                IReadOnlyList<PropertyInfo>
                    srcProps = srcType.GetProperties(bindingFlagsBase | BindingFlags.GetProperty),
                    dstProps = dstType.GetProperties(bindingFlagsBase | BindingFlags.SetProperty);

                block = Expression.Block
                (
                    variables: new[] { src, dst }, 
                    expressions: new Expression[] 
                    {
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

                        from   srcProp in srcProps
                        let    dstProp = dstProps.SingleOrDefault(dstProp => dstProp.Name == srcProp.Name && dstProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                        where  dstProp != null
                        select Expression.Assign(Expression.Property(dst, dstProp), Expression.Property(src, srcProp))
                    )
                    .Append
                    (
                        //
                        // return dst;
                        //

                        dst
                    )
                );
            }

            return Expression.Lambda<Func<object, object>>(block, p).Compile();
        }, nameof(Mapper));

        object? IMapper.MapTo(Type srcType, Type dstType, object? source)
        {
            if (source == null) return null;

            Func<object, object> map = Cache.GetOrAdd((srcType, dstType), new Func<Func<object, object>>(() => throw MappingNotSupported(srcType, dstType)), nameof(Mapper));

            return map.Invoke(source);
        }

        private static NotSupportedException MappingNotSupported(Type srcType, Type dstType) 
        {
            var ex = new NotSupportedException(Resources.MAPPING_NOT_SUPPORTED);
            ex.Data["mapping"] = $"{srcType} -> {dstType}";

            return ex;
        }
    }
}
