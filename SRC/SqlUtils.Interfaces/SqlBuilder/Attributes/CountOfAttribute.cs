/********************************************************************************
*  CountOfAttribute.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Count selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CountOfAttribute : AggregateSelectionAttribute
    {
        private static readonly MethodInfo FSelect = GetQueryMethod(bldr => bldr.SelectCount(null!, null!));

        /// <summary>
        /// Creates a new <see cref="CountOfAttribute"/>.
        /// </summary>
        public CountOfAttribute(Type ormType, bool required = true, string? alias = null) : base(ormType, required, alias, FSelect)
        {
        }
    }
}