/********************************************************************************
*  ViewAttribute.cs                                                             *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Marks a class to be used as a view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViewAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="ViewAttribute"/> instance.
        /// </summary>
        public ViewAttribute(Type? @base = null) => Base = @base;

        /// <summary>
        /// The base table to appear in the FROM clause. If null the base of the view will be used.
        /// </summary>
        public Type? Base { get; }
    }
}
