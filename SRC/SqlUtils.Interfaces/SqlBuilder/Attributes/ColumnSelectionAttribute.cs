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
    /// <summary>
    /// Represents an abstract database column selection.
    /// </summary>
    public abstract class ColumnSelectionAttribute : Attribute, IFragment, IBuildableAttribute
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
        /// The (optional) alias of the column name.
        /// </summary>
        public string? Alias { get; }

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
        /// See <see cref="IFragment.GetFragments(ParameterExpression, PropertyInfo, bool)"/>.
        /// </summary>
        public virtual IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy)
        {
            if (bldr == null)
                throw new ArgumentNullException(nameof(bldr));

            if (viewProperty == null)
                throw new ArgumentNullException(nameof(viewProperty));

            //
            // Ha van Alias akkor a nezet oszlop neve nem egyezik meg a tabla oszlop nevevel (Alias kicsit geci modon itt a tabla
            // oszlopnevet jelenti).
            //

            string property = Alias ?? viewProperty.Name;

            yield return Expression.Call(
                bldr,
                Action,
                Expression.Constant(OrmType.GetProperty(property) ?? throw new MissingMemberException(OrmType.Name, property)),
                Expression.Constant(viewProperty));
        }

        /// <summary>
        /// See <see cref="IBuildableAttribute.GetBuilder"/>.
        /// </summary>
        public abstract CustomAttributeBuilder GetBuilder();

        /// <summary>
        /// Creates a new <see cref="ColumnSelectionAttribute"/> instance.
        /// </summary>
        protected ColumnSelectionAttribute(Type ormType, bool required, string? alias, MethodInfo action)
        {
            OrmType  = ormType ?? throw new ArgumentNullException(nameof(ormType));
            Action   = action  ?? throw new ArgumentNullException(nameof(action));
            Required = required;
            Alias    = alias;         
        }
    }
}