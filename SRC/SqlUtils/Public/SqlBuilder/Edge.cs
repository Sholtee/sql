/********************************************************************************
*  Edge.cs                                                                      *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL
{
    using Properties;

    /// <summary>
    /// Defines an edge between two data tables.
    /// </summary>
    public record Edge
    {
        /// <summary>
        /// Represents the source data table.
        /// </summary>
        public Type SourceTable => SourceProperty.ReflectedType;

        /// <summary>
        /// Represents the destination data table.
        /// </summary>
        public Type DestinationTable => DestinationProperty.ReflectedType;

        /// <summary>
        /// Represents the source table column.
        /// </summary>
        public PropertyInfo SourceProperty { get; }

        /// <summary>
        /// Represents the destination table column.
        /// </summary>
        public PropertyInfo DestinationProperty { get; }

        /// <summary>
        /// Creates a new <see cref="Edge"/> instance.
        /// </summary>
        internal Edge(PropertyInfo src, PropertyInfo dst)
        {
            SourceProperty      = src ?? throw new ArgumentNullException(nameof(src));
            DestinationProperty = dst ?? throw new ArgumentNullException(nameof(dst));
        }

        /// <summary>
        /// Creates a new <see cref="Edge"/> instance.
        /// </summary>
        public static Edge Create<TSrc, TDst>(Expression<Func<TSrc, object>> src, Expression<Func<TDst, object>> dst)
        {
            //
            // Az ExtractProperty validal.
            //

            return new Edge(ExtractProperty(src), ExtractProperty(dst));

            static PropertyInfo ExtractProperty<T>(Expression<Func<T, object>> property)
            {
                if (property == null)
                    throw new ArgumentNullException(nameof(property));

                Expression body = property.Body;
                UnaryExpression? unaryExpression = body as UnaryExpression;
                MemberExpression? memberExpression = (unaryExpression?.Operand ?? body) as MemberExpression;

                if (memberExpression == null) throw new InvalidOperationException(Resources.NOT_A_PROPERTY);

                //
                // A PropertyInfo.ReferencedType typeof(T)-re fog mutatni nem a deklaralo tipusra
                //

                return typeof(T).GetProperty(memberExpression.Member.Name);
            }
        }

        /// <summary>
        /// Stringigies this edge.
        /// </summary>
        public override string ToString() => $"Edge({SourceTable.FullName} -> {DestinationTable.FullName})";
    }
}