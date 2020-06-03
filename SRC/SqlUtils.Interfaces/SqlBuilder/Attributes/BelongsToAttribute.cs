/********************************************************************************
*  BelongsToAttribute.cs                                                        *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents a simple column selection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BelongsToAttribute: CustomBelongsToAttribute
    {
        /// <summary>
        /// Creates a new <see cref="BelongsToAttribute"/> instance.
        /// </summary>
        public BelongsToAttribute(Type ormType, bool required = true, string? alias = null, Order order = Order.None) : base(ormType, required, alias, order)
        {
        }
    }
}