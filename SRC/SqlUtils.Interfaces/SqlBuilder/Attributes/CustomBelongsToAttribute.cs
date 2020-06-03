/********************************************************************************
*  CustomBelongsToAttribute.cs                                                  *
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
    /// The base of the <see cref="BelongsToAttribute"/>.
    /// </summary>
    public abstract class CustomBelongsToAttribute : ColumnSelectionAttribute
    {
        #region Helpers
        private static readonly MethodInfo mGroupBy = GetQueryMethod(bldr => bldr.GroupBy(null!));

        private static readonly MethodInfo[] mOrderBy =
        {
            GetQueryMethod(bldr => bldr.OrderBy(null!)), // [0] == Order.Ascending - 1
            GetQueryMethod(bldr => bldr.OrderByDescending(null!)) // [1] == Order.Descending - 1
        };
        #endregion

        /// <summary>
        /// See <see cref="ColumnSelectionBaseAttribute.GetFragments(ParameterExpression, PropertyInfo, bool)"/>
        /// </summary>
        protected override IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy)
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
                yield return Expression.Call(bldr, mGroupBy, sel);

            if (Order > Order.None)
                yield return Expression.Call(bldr, mOrderBy[(int) Order - 1], sel);
        }

        /// <summary>
        /// See <see cref="ColumnSelectionBaseAttribute.GetBuilder"/>.
        /// </summary>
        /// <returns></returns>
        protected override CustomAttributeBuilder GetBuilder() => new CustomAttributeBuilder(
            GetType().GetConstructor(new[] 
            { 
                typeof(Type), 
                typeof(bool), 
                typeof(string), 
                typeof(Order) 
            }) ?? throw new MissingMemberException(),
            new object?[]
            {
                OrmType,
                Required,
                Alias,
                Order
            });

        /// <summary>
        /// Rendezés iránya.
        /// </summary>
        public Order Order { get; }

        /// <summary>
        /// Creates a new <see cref="CustomBelongsToAttribute"/> instance.
        /// </summary>
        protected CustomBelongsToAttribute(Type ormType, bool required = true, string? alias = null, Order order = Order.None): base(ormType, required, alias)
        {
            Order = order;
        }
    }
}