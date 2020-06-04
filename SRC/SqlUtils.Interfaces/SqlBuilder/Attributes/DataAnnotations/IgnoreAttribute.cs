/********************************************************************************
*  IgnoreAttribute.cs                                                           *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces.DataAnnotations
{
    /// <summary>
    /// Marks a property not to represent a database column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
