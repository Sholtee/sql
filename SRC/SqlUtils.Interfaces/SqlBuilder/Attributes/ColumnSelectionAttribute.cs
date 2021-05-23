/********************************************************************************
*  ColumnSelectionAttribute.cs                                                  *
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
    using Properties;

    /// <summary>
    /// Represents an abstract database column selection.
    /// </summary>
    public abstract class ColumnSelectionAttribute : Attribute, IFragmentFactory, IBuildableAttribute
    {
        /// <summary>
        /// The <see cref="Type"/> that represents the data table.
        /// </summary>
        public Type OrmType { get; }

        /// <summary>
        /// Specifies if the selection must return a value or not.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// The name of the data table column (if it differs from the view column).
        /// </summary>
        public string? Column { get; }

        /// <summary>
        /// The action belongs to this selection.
        /// </summary>
        public MethodInfo Action { get; }

        /// <summary>
        /// Gets a <see cref="MethodInfo"/> from the <see cref="ISqlQuery"/> interface. 
        /// </summary>
        protected static MethodInfo GetQueryMethod(Expression<Action<ISqlQuery>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return ((MethodCallExpression) expr.Body).Method;
        }

        /// <summary>
        /// See <see cref="IFragmentFactory.GetFragments(ParameterExpression, PropertyInfo, bool)"/>.
        /// </summary>
        public virtual IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy)
        {
            if (bldr == null)
                throw new ArgumentNullException(nameof(bldr));

            if (viewProperty == null)
                throw new ArgumentNullException(nameof(viewProperty));

            if (!viewProperty.PropertyType.IsValueType && viewProperty.PropertyType != typeof(string))
                throw new ArgumentException(Resources.NOT_VALUE_TYPE, nameof(viewProperty));

            string property = Column ?? viewProperty.Name;

            yield return Expression.Call(
                bldr,
                Action,
                Expression.Constant(OrmType.GetProperty(property) ?? throw new MissingMemberException(OrmType.Name, property)),
                Expression.Constant(viewProperty));
        }

        /// <summary>
        /// See <see cref="IBuildableAttribute.GetBuilder"/>.
        /// </summary>
        public abstract CustomAttributeBuilder GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters);

        /// <summary>
        /// Creates a new <see cref="ColumnSelectionAttribute"/> instance.
        /// </summary>
        protected ColumnSelectionAttribute(Type ormType, bool required, string? column, MethodInfo action)
        {
            OrmType  = ormType ?? throw new ArgumentNullException(nameof(ormType));
            Action   = action  ?? throw new ArgumentNullException(nameof(action));
            Required = required;
            Column   = column;         
        }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}({nameof(OrmType)}={OrmType.FullName}, {nameof(Column)}={(Column is not null ? $"\"{Column}\"" : "NULL")}, {nameof(Required)}={Required})";
    }
}