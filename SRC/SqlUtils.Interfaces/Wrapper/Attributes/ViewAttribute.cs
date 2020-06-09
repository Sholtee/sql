/********************************************************************************
*  ViewAttribute.cs                                                             *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Marks a class that represents a view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViewAttribute : Attribute
    {
        /// <summary>
        /// The base table to appear in the FROM clause. If null the base of the view will be used.
        /// </summary>
        public Type? Base { get; set; }
    }
}
