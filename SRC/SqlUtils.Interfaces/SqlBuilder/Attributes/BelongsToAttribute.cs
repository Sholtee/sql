/********************************************************************************
*  BelongsToAttribute.cs                                                        *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents a simple database column selection.
    /// </summary>
    public sealed class BelongsToAttribute : ColumnSelectionAttribute
    {
        #region Helpers
        private static readonly MethodInfo FSelect = GetQueryMethod(bldr => bldr.Select(null!, null!));

        private static readonly MethodInfo FGroupBy = GetQueryMethod(bldr => bldr.GroupBy(null!));

        private static readonly MethodInfo[] FOrderBy =
        {
            GetQueryMethod(bldr => bldr.OrderBy(null!)), // [0] == Order.Ascending - 1
            GetQueryMethod(bldr => bldr.OrderByDescending(null!)) // [1] == Order.Descending - 1
        };
        #endregion

        /// <summary>
        /// See <see cref="ColumnSelectionAttribute.GetFragments(ParameterExpression, PropertyInfo, bool)"/>.
        /// </summary>
        public override IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy)
        {
            if (bldr == null)
                throw new ArgumentNullException(nameof(bldr));

            if (viewProperty == null)
                throw new ArgumentNullException(nameof(viewProperty));

            foreach (MethodCallExpression action in base.GetFragments(bldr, viewProperty, isGroupBy))
            {
                yield return action;
            }

            if (!isGroupBy && Order == Order.None) 
                yield break;

            string propName = Alias ?? viewProperty.Name;

            ConstantExpression sel = Expression.Constant(OrmType.GetProperty(propName) ?? throw new MissingMemberException(OrmType.Name, propName));

            if (isGroupBy)
                yield return Expression.Call(bldr, FGroupBy, sel);

            if (Order > Order.None)
                yield return Expression.Call(bldr, FOrderBy[(int) Order - 1], sel);
        }

        /// <summary>
        /// See <see cref="ColumnSelectionAttribute.GetBuilder"/>.
        /// </summary>
        /// <returns></returns>
        public override CustomAttributeBuilder GetBuilder() => new CustomAttributeBuilder
        (
            GetType().GetConstructor(new[] 
            { 
                typeof(Type), 
                typeof(bool), 
                typeof(string), 
                typeof(Order) 
            }) ?? throw new MissingMethodException(GetType().Name, "Ctor"),
            new object?[]
            {
                OrmType,
                Required,
                Alias,
                Order
            }
        );

        /// <summary>
        /// Should the result be sorted by this column?
        /// </summary>
        /// <remarks>The result can be sorted by multiple columns. In this case, the order of columns in ORDER BY clause will match the order of properties in your view.</remarks>
        public Order Order { get; }

        /// <summary>
        /// Creates a new <see cref="BelongsToAttribute"/> instance.
        /// </summary>
        public BelongsToAttribute(Type ormType, bool required = true, string? alias = null, Order order = Order.None): base(ormType, required, alias, FSelect)
        {
            Order = order;
        }
    }
}