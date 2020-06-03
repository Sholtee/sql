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
    }
}
