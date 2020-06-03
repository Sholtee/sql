/********************************************************************************
*  EmptyListMarkerAttribute.cs                                                  *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Marks a list property as "may be empty".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EmptyListMarkerAttribute : Attribute
    {
    }
}