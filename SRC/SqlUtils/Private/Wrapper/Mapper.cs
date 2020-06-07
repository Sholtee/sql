/********************************************************************************
* Mapper.cs                                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
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
        void IMapper.RegisterMapping(Type srcType, Type dstType)
        {
            CheckType(srcType);
            CheckType(dstType);

            Cache.GetOrAdd((srcType, dstType), () =>
            {
                const BindingFlags bindingFlagsBase = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                IReadOnlyList<PropertyInfo>
                    srcProps = srcType.GetProperties(bindingFlagsBase | BindingFlags.GetProperty),
                    dstProps = dstType.GetProperties(bindingFlagsBase | BindingFlags.SetProperty);

                ParameterExpression
                    p   = Expression.Parameter(typeof(object)),
                    src = Expression.Variable(srcType, nameof(src)),
                    dst = Expression.Variable(dstType, nameof(dst));

                Expression block = Expression.Block
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
                        // return (object) dst;
                        //

                        Expression.Convert(dst, typeof(object))
                    )
                );

                return Expression.Lambda<Func<object, object>>(block, p).Compile();
            }, nameof(Mapper));

            void CheckType(Type t)
            {
                if (t.IsPrimitive || t == typeof(String) || typeof(IEnumerable).IsAssignableFrom(t))
                {
                    throw MappingNotSupported(srcType, dstType);
                }
            }
        }

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
