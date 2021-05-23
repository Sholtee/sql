/********************************************************************************
*  WrappedAttribute.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Marks a list property on a view to be unwrapped on SQL building or to be wrapped on relation mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class WrappedAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="WrappedAttribute"/> instance.
        /// </summary>
        public WrappedAttribute(bool required = true) => Required = required;

        /// <summary>
        /// Indicates whether the fields of a wrapped view are required or not.
        /// </summary>
        public bool Required { get; }
    }
}
