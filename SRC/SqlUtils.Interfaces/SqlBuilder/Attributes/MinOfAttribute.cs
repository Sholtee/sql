/********************************************************************************
*  MinOfAttribute.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Minimum selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MinOfAttribute : AggregateSelectionAttribute
    {
        private static readonly MethodInfo FSelect = GetQueryMethod(bldr => bldr.SelectMin(null!, null!));

        /// <summary>
        /// Creates a new <see cref="MinOfAttribute"/> instance.
        /// </summary>
        public MinOfAttribute(Type ormType, bool required = true, string? alias = null): base(ormType, required, alias, FSelect)
        {
        }
    }
}