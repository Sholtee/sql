/********************************************************************************
*  ColumnSelectionAttribute.cs                                                  *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents a single column selection.
    /// </summary>
    public abstract class ColumnSelectionAttribute : ColumnSelectionBaseAttribute
    {
        private static readonly MethodInfo mSelect = GetQueryMethod(bldr => bldr.Select(null!, null!));

        /// <summary>
        /// Creates a new <see cref="ColumnSelectionAttribute"/>.
        /// </summary>
        protected ColumnSelectionAttribute(Type ormType, bool required, string? alias) : base(ormType, required, alias, mSelect)
        {
        }
    }
}