/********************************************************************************
*  AverageOfAttribute.cs                                                        *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Average selection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AverageOfAttribute : AggregateSelectionAttribute
    {
        private static readonly MethodInfo FSelect = GetQueryMethod(bldr => bldr.SelectAvg(null!, null!));

        /// <summary>
        /// Creates a new <see cref="AverageOfAttribute"/> instance.
        /// </summary>
        public AverageOfAttribute(Type ormType, bool required = true, string? column = null): base(ormType, required, column, FSelect)
        {
        }
    }
}