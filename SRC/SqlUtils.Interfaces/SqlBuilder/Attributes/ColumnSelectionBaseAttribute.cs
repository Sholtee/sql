/********************************************************************************
*  ColumnSelectionBaseAttribute.cs                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Defines an abstract database column selection.
    /// </summary>
    public abstract class ColumnSelectionBaseAttribute : Attribute, IFragment, IBuildable
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

        IEnumerable<MethodCallExpression> IFragment.GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy) => GetFragments(
            bldr ?? throw new ArgumentNullException(nameof(bldr)), 
            viewProperty ?? throw new ArgumentNullException(nameof(viewProperty)), 
            isGroupBy);

        /// <summary>
        /// Gets the generator methods.
        /// </summary>
        protected virtual IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy)
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
                FSelect,
                Expression.Constant(OrmType.GetProperty(property) ?? throw new MissingMemberException(OrmType.Name, property)),
                Expression.Constant(viewProperty));
        }

        [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types")]
        CustomAttributeBuilder IBuildable.Builder => GetBuilder();

        /// <summary>
        /// Gets the related <see cref="CustomAttributeBuilder"/>. For mire information see the <see cref="IBuildable"/> interface.
        /// </summary>
        protected abstract CustomAttributeBuilder GetBuilder();

        private readonly MethodInfo FSelect;

        /// <summary>
        /// Creates a new <see cref="ColumnSelectionBaseAttribute"/> instance.
        /// </summary>
        protected ColumnSelectionBaseAttribute(Type ormType, bool required, string? alias, MethodInfo select)
        {
            OrmType  = ormType ?? throw new ArgumentNullException(nameof(ormType));
            FSelect  = select ?? throw new ArgumentNullException(nameof(select));
            Required = required;
            Alias    = alias;         
        }
    }
}