/********************************************************************************
*  MaxOfAttribute.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Maximum selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MaxOfAttribute : AggregateSelectionAttribute
    {
        private static readonly MethodInfo FSelect = GetQueryMethod(bldr => bldr.SelectMax(null!, null!));

        /// <summary>
        /// Creates a new <see cref="MaxOfAttribute"/> instance.
        /// </summary>
        public MaxOfAttribute(Type ormType, bool required = true, string? column = null): base(ormType, required, column, FSelect)
        {
        }
    }
}