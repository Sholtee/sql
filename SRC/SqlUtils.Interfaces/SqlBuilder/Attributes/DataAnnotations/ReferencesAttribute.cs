/********************************************************************************
*  ReferencesAttribute.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces.DataAnnotations
{
    /// <summary>
    /// Represetns a reference to a foreign table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ReferencesAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="ReferencesAttribute"/> instance.
        /// </summary>
        public ReferencesAttribute(Type type) => Type = type ?? throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// A hivatkozott ORM típus.
        /// </summary>
        public Type Type { get; }
    }
}