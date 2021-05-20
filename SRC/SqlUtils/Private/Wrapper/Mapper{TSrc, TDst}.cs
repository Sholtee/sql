/********************************************************************************
* Mapper{TSrc, TDst}.cs                                                         *
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
    using Properties;
    using Primitives.Patterns;

    internal sealed class Mapper<TSrc, TDst>: Singleton<Mapper<TSrc, TDst>>
    {
        private Func<TSrc?, TDst?> Core { get; } = CreateDelegate();

        //
        // Magat a delegate-et kozvetlen ne tegyuk elerhetove mert annak Method tulajdonsagat
        // nem lehet dinamikus kifejezesekben hasznalni mivel o maga is dinamikus.
        //

        public static TDst? Map(TSrc? src) => Instance.Core.Invoke(src);

        private static Func<TSrc?, TDst?> CreateDelegate()
        {
            ParameterExpression p = Expression.Parameter(typeof(TSrc));

            PropertyInfo? mapFrom = typeof(TSrc).MapFrom();

            return Expression
                .Lambda<Func<TSrc?, TDst?>>
                (
                    mapFrom is null 
                        ? CreateForClass(p) 
                        : CreateForValueType(p, mapFrom), 
                    p
                )
                .Compile();
        }

        private static BlockExpression CreateForValueType(ParameterExpression p, PropertyInfo property) 
        {
            Type dstType = typeof(TDst);

            if (property?.PropertyType != dstType)
                throw MappingNotSupported();

            return Expression.Block
            (
                //
                // return (TDst) p.Prop;
                //

                Expression.Convert
                (
                    Expression.Property(p, property), 
                    dstType
                )
            );
        }

        private static BlockExpression CreateForClass(ParameterExpression p) 
        {
            Type
                srcType = typeof(TSrc),
                dstType = typeof(TDst);

            if (!srcType.IsClass || !dstType.IsClass)
                throw MappingNotSupported();

            ParameterExpression dst = Expression.Variable(dstType, nameof(dst));

            LabelTarget label = Expression.Label(dstType);

            const BindingFlags bindingFlagsBase = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            IReadOnlyList<PropertyInfo>
                srcProps = srcType.GetProperties(bindingFlagsBase | BindingFlags.GetProperty),
                dstProps = dstType.GetProperties(bindingFlagsBase | BindingFlags.SetProperty);

            return Expression.Block
            (
                variables: new[] { dst },
                expressions: GetBlockExpressions()
            )!;

            IEnumerable<Expression> GetBlockExpressions() 
            {
                //
                // if (p == null) return null
                //

                yield return Expression.IfThen
                (
                    Expression.Equal(p, Expression.Default(srcType)),
                    Expression.Return(label, Expression.Default(dstType))
                );

                //
                // TDst dst = new TDst();
                //

                yield return Expression.Assign(dst, Expression.New(dstType));

                //
                // dst.Prop_1 = p.Prop_1;
                // ...
                // dst.Prop_N = p.Prop_N;
                //

                foreach (PropertyInfo srcProp in srcProps) 
                {
                    PropertyInfo dstProp = dstProps.SingleOrDefault(dstProp => srcProp.CanBeMappedIn(dstProp));
                    if (dstProp is null) continue;

                    yield return Expression.Assign(Expression.Property(dst, dstProp), Expression.Property(p, srcProp));
                }

                //
                // return dst;
                //

                yield return Expression.Return(label, dst);
                yield return Expression.Label(label, Expression.Default(dstType));
            }
        }

        private static NotSupportedException MappingNotSupported()
        {
            var ex = new NotSupportedException(Resources.MAPPING_NOT_SUPPORTED);
            ex.Data["mapping"] = $"{typeof(TSrc)} -> {typeof(TDst)}";

            return ex!;
        }
    }
}
