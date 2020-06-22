/********************************************************************************
*  AggregateSelectionAttribute.cs                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents a generic agregate selection.
    /// </summary>
    public abstract class AggregateSelectionAttribute : ColumnSelectionAttribute
    {
        /// <summary>
        /// See <see cref="ColumnSelectionAttribute.GetBuilder"/>.
        /// </summary>
        public override CustomAttributeBuilder GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters) => new CustomAttributeBuilder
        (
            GetType().GetConstructor(new[] 
            { 
                typeof(Type), 
                typeof(bool), 
                typeof(string) 
            }) ?? throw new MissingMethodException(GetType().Name, "Ctor"),
            new object?[]
            {
                OrmType,
                Required,
                customParameters.SingleOrDefault(para => para.Key.Name == nameof(Column)).Value ?? Column,
            }
        );

        /// <summary>
        /// Creates a new <see cref="AggregateSelectionAttribute"/> instance.
        /// </summary>
        protected AggregateSelectionAttribute(Type ormType, bool required, string? column, MethodInfo select) : base(ormType, required, column, select)
        {
        }
    }
}